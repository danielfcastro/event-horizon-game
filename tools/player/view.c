/* A-026 Non-Unity player package — tools/player/view.c
 *
 * The SDL2 half of the two-process client. It owns the WINDOW and the
 * KEYBOARD. It never steps the simulation and never writes simulation state:
 * it reads a frame payload written by tools/player/SimHost.cs and draws it.
 * The only thing it sends to the other process is key state (which direction
 * the player is pressing), never a position, a mass, or a step.
 *
 * Every declaration used here was read from the installed headers, not
 * guessed. This SDL is 2.32.74 and it declares SDL_UpperBlit (4 args, no
 * blend parameter — blend mode is set on the surface) and
 * SDL_CreateRGBSurfaceFrom; SDL_BlitSurface is not declared in this header
 * set, so it is not used.
 *
 * Build:  gcc tools/player/view.c -o obj-player/view \
 *            -I/usr/include/SDL2 -D_GNU_SOURCE=1 -D_REENTRANT -lm -lSDL2
 *
 * Dialect fact PROBED, not assumed: this toolchain's gcc does NOT auto-link
 * libm, so sqrt/sin/cos (math.h is included but that is header-only) fail to
 * link without an explicit -lm. The link errors named exactly the three
 * functions used; -lm is placed before -lSDL2 so both resolve.
 *
 * Modes:
 *   (default)      windowed, reads the newest payload, draws, paces at 60 fps
 *   --ppm PATH     render ONE frame from the payload at --frame into PATH as
 *                  a binary PPM and exit. No window, no display needed: this
 *                  is the mode used to prove the renderer actually draws.
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <math.h>
#include <unistd.h>
#include <SDL2/SDL.h>

/* ---- payload protocol, must match SimHost.cs exactly ---------------------- */
#define FRAME_MAGIC "EHFRM1"
#define KEY_MAGIC "EHKEY1"
#define FNV32_OFFSET 2166136261u
#define FNV32_PRIME 16777619u

static uint32_t fnv32(const unsigned char *p, size_t n)
{
    uint32_t h = FNV32_OFFSET;
    for (size_t i = 0; i < n; i++)
    {
        h = (h ^ (uint32_t)p[i]) * FNV32_PRIME;
    }
    return h;
}

/* ---- framebuffer: one drawing path feeds BOTH the window and the PPM ------ */
static int VW = 960, VH = 540;

/* §5.3 camera constants. The event horizon is held at HORIZON_PCT percent of
 * the viewport short edge so it is always on screen, and the scale never drops
 * below MIN_PPU px/WU so bodies stay readable at the end of a long run. The
 * upper bound is the tier's pixelsPerUnit carried in the payload, so a device
 * tier still decides sharpness; the camera only ever zooms out as the hole
 * grows. Render-only: none of this reaches SimCore. */
#define HORIZON_PCT 62
#define MIN_PPU 2

/* render-only diagnostic: the camera scale actually used for the last frame,
 * so the dump proof can report what was drawn rather than re-deriving it. */
static int64_t g_ppuUsed = 0;
static uint32_t *fb = NULL;

static inline uint32_t pack_px(uint32_t r, uint32_t g, uint32_t b)
{
    /* packing matches the Rmask/Gmask/Bmask/Amask passed to
       SDL_CreateRGBSurfaceFrom below */
    return 0xFF000000u | (r << 16) | (g << 8) | b;
}

static void fill_span(int cx, int cy, int half, uint32_t c)
{
    if (cy < 0 || cy >= VH)
    {
        return;
    }
    int x0 = cx - half;
    int x1 = cx + half;
    if (x0 < 0)
    {
        x0 = 0;
    }
    if (x1 >= VW)
    {
        x1 = VW - 1;
    }
    for (int x = x0; x <= x1; x++)
    {
        fb[(size_t)cy * VW + x] = c;
    }
}

static void disc(int cx, int cy, int r, uint32_t c)
{
    for (int dy = -r; dy <= r; dy++)
    {
        int half = (int)(sqrt((double)((int64_t)r * r - (int64_t)dy * dy)) + 0.5);
        fill_span(cx, cy + dy, half, c);
    }
}

static void ring(int cx, int cy, int r, uint32_t c)
{
    /* accretion rim: the outline of the disc, one pixel wide plus a halo */
    for (int a = 0; a < 720; a++)
    {
        double th = (double)a * 3.14159265358979 / 360.0;
        int x = cx + (int)(sin(th) * (double)r + 0.5);
        int y = cy + (int)(cos(th) * (double)r + 0.5);
        if (x >= 0 && x < VW && y >= 0 && y < VH)
        {
            fb[(size_t)y * VW + x] = c;
        }
    }
}

/* Q32.32 world delta, Q32.32 pixels-per-unit -> screen pixels.
 * 128-bit intermediate: the product of two Q32.32 values does not fit in 64
 * bits. This is render-side math; the simulation has no float and no such
 * multiply, and nothing here is ever fed back to the simulation.
 * Dialect fact PROBED, not assumed: the product is Q64.64, so the shift to
 * INTEGER screen pixels is 64, not 32. At >> 32 the hole radius truncated to
 * 0 (clamped to 1) and every body fell out of the off-screen cull: the PPM
 * proof rendered 6 non-background pixels. >> 64 draws the real frame. */
static long long wu_to_px(long long delta_q, uint64_t ppu_q)
{
    __int128 t = (__int128)delta_q * (__int128)ppu_q;
    return (long long)(t >> 64);
}

/* ---- payload reader ------------------------------------------------------- */
/* Returns 0 on success, non-zero when the payload is missing, truncated,
 * has the wrong magic, or fails its checksum. A rejected payload means the
 * caller keeps the previous frame: a torn write is a render-only glitch and
 * never changes what the simulation did. */
typedef struct
{
    int64_t step;
    int64_t holeX, holeY, holeRadius, holeEventRadius, holeMass;
    int count;
    int64_t *bx, *by, *br;
    unsigned char *kind;
    uint64_t ppu;
    int resultFlag;
} Payload;

static int64_t be_i64(const unsigned char *p)
{
    int64_t v = 0;
    for (int i = 0; i < 8; i++)
    {
        v = (v << 8) | (int64_t)p[i];
    }
    return v;
}
static uint32_t be_u32(const unsigned char *p)
{
    return ((uint32_t)p[0] << 24) | ((uint32_t)p[1] << 16) | ((uint32_t)p[2] << 8) |
        (uint32_t)p[3];
}

static unsigned char *raw = NULL;
static size_t rawLen = 0;

static int read_payload(const char *path, Payload *out)
{
    FILE *f = fopen(path, "rb");
    if (!f)
    {
        return 1; /* no frame yet */
    }
    fseek(f, 0, SEEK_END);
    long sz = ftell(f);
    fseek(f, 0, SEEK_SET);
    if (sz < 0)
    {
        fclose(f);
        return 2;
    }
    if ((size_t)sz > rawLen)
    {
        free(raw);
        rawLen = (size_t)sz;
        raw = (unsigned char *)malloc(rawLen);
    }
    size_t got = fread(raw, 1, (size_t)sz, f);
    fclose(f);
    if (got != (size_t)sz)
    {
        return 3; /* short read: treat as torn */
    }
    if (sz < 6 + 4 + 5 * 8 + 2 + 8 + 1 + 4 + 4)
    {
        return 4;
    }
    if (memcmp(raw, FRAME_MAGIC, 6) != 0)
    {
        return 5;
    }
    uint32_t want = fnv32(raw, (size_t)sz - 4);
    uint32_t have = be_u32(raw + sz - 4);
    if (want != have)
    {
        return 6; /* torn write */
    }
    size_t at = 6;
    at += 4; /* seq */
    out->step = be_i64(raw + at);
    at += 8;
    out->holeX = be_i64(raw + at);
    at += 8;
    out->holeY = be_i64(raw + at);
    at += 8;
    out->holeRadius = be_i64(raw + at);
    at += 8;
    out->holeEventRadius = be_i64(raw + at); /* the visible hole: §5.3 */
    at += 8;
    out->holeMass = be_i64(raw + at);
    at += 8;
    int count = (int)((raw[at] << 8) | raw[at + 1]);
    at += 2;
    if (count < 0 || count > 300)
    {
        return 7;
    }
    out->count = count;
    for (int i = 0; i < count; i++)
    {
        out->bx[i] = be_i64(raw + at);
        at += 8;
        out->by[i] = be_i64(raw + at);
        at += 8;
        out->br[i] = be_i64(raw + at);
        at += 8;
        out->kind[i] = raw[at];
        at += 1;
    }
    out->ppu = 0;
    for (int i = 0; i < 8; i++)
    {
        out->ppu = (out->ppu << 8) | (uint64_t)raw[at + i];
    }
    at += 8;
    out->resultFlag = raw[at];
    at += 1;
    return 0;
}

/* ---- draw one payload ----------------------------------------------------- */
static int draw(const Payload *p)
{
    uint32_t bg = pack_px(8, 10, 18);
    for (size_t i = 0; i < (size_t)VW * VH; i++)
    {
        fb[i] = bg;
    }

    int cx = VW / 2;
    int cy = VH / 2;

    /* §5.3 camera: the visible hole is the event horizon, and the zoom keeps
     * it on screen. ppu_eff = clamp(HORIZON_PCT percent of the viewport short
     * edge divided by the horizon diameter in WU, MIN_PPU, the tier's pixelsPerUnit
     * from the payload). The scale is chosen in double because it is render-only:
     * it never reaches SimCore, so it cannot change what the simulation did;
     * the fixed-point wu_to_px below still does the actual mapping. */
    double evWu = (double)p->holeEventRadius / 4294967296.0;
    long long tierPpu = (long long)(p->ppu >> 32);
    if (tierPpu < MIN_PPU)
    {
        tierPpu = MIN_PPU;
    }
    long shortEdge = (VH < VW) ? (long)VH : (long)VW;
    double want = (evWu > 0.0)
                      ? ((double)shortEdge * (double)HORIZON_PCT / 100.0) / (2.0 * evWu)
                      : (double)tierPpu;
    if (want < (double)MIN_PPU)
    {
        want = (double)MIN_PPU;
    }
    if (want > (double)tierPpu)
    {
        want = (double)tierPpu;
    }
    int64_t ppuEff = (int64_t)(want * 4294967296.0);
    g_ppuUsed = ppuEff;

    int hr = (int)wu_to_px(p->holeEventRadius, ppuEff);
    if (hr < 1)
    {
        hr = 1;
    }

    /* bodies first: the hole absorbs, so it occludes what it has caught */
    for (int i = 0; i < p->count; i++)
    {
        long long sx = (long long)cx + wu_to_px(p->bx[i] - p->holeX, ppuEff);
        long long sy = (long long)cy - wu_to_px(p->by[i] - p->holeY, ppuEff);
        if (sx < -256 || sx > VW + 256 || sy < -256 || sy > VH + 256)
        {
            continue; /* off-screen */
        }
        int rr = (int)wu_to_px(p->br[i], ppuEff);
        if (rr < 1)
        {
            rr = 1;
        }
        uint32_t r, g, b;
        switch ((p->kind[i] >> 2) & 3)
        {
        case 1:
            r = 255;
            g = 186;
            b = 64;
            break; /* medium */
        case 2:
            r = 255;
            g = 92;
            b = 176;
            break; /* large */
        case 3:
            r = 122;
            g = 255;
            b = 140;
            break; /* hazard tier */
        default:
            r = 84;
            g = 200;
            b = 255;
            break; /* small */
        }
        if ((p->kind[i] & 2) != 0)
        { /* absorbable: brighter, so the two states read without colour alone */
            r = r > 160 ? 255 : r + 60;
            g = g > 160 ? 255 : g + 60;
            b = b > 160 ? 255 : b + 60;
        }
        disc((int)sx, (int)sy, rr, pack_px(r, g, b));
    }

    /* the black hole: the event horizon is the visible hole (§5.4 "Visual
     * gravitational boundary"), drawn as a true black disc with a bright
     * accretion rim. The collision core (holeRadius) is NOT drawn as the hole:
     * it is constant, and drawing it is what made the hole look frozen. */
    disc(cx, cy, hr, pack_px(0, 0, 0));
    ring(cx, cy, hr, pack_px(140, 236, 255));
    if (hr > 2)
    {
        ring(cx, cy, hr - 2, pack_px(64, 110, 130));
    }
    return 0;
}

/* ---- key state writer (atomic: tmp + rename, so the sim never sees a torn
 * write; C has rename(), unlike the C# side) -------------------------------- */
static void write_key(const char *path, int dx, int dy, int rateCode, int quit)
{
    unsigned char b[20];
    memcpy(b, KEY_MAGIC, 6);
    int32_t v;
    v = dx;
    for (int i = 0; i < 4; i++)
    {
        b[6 + i] = (unsigned char)((v >> (24 - 8 * i)) & 0xFF);
    }
    v = dy;
    for (int i = 0; i < 4; i++)
    {
        b[10 + i] = (unsigned char)((v >> (24 - 8 * i)) & 0xFF);
    }
    b[14] = (unsigned char)rateCode;
    b[15] = (unsigned char)quit;
    uint32_t c = fnv32(b, 16);
    for (int i = 0; i < 4; i++)
    {
        b[16 + i] = (unsigned char)((c >> (24 - 8 * i)) & 0xFF);
    }
    char tmp[512];
    snprintf(tmp, sizeof tmp, "%s.tmp", path);
    FILE *f = fopen(tmp, "wb");
    if (!f)
    {
        return;
    }
    fwrite(b, 1, sizeof b, f);
    fclose(f);
    if (rename(tmp, path) != 0)
    {
        unlink(tmp);
    }
}

/* ---- PPM dump ------------------------------------------------------------- */
static int dump_ppm(const char *path)
{
    FILE *f = fopen(path, "wb");
    if (!f)
    {
        return 1;
    }
    fprintf(f, "P6\n%d %d\n255\n", VW, VH);
    for (int y = 0; y < VH; y++)
    {
        for (int x = 0; x < VW; x++)
        {
            uint32_t c = fb[(size_t)y * VW + x];
            unsigned char tri[3] = {(unsigned char)((c >> 16) & 0xFF),
                (unsigned char)((c >> 8) & 0xFF), (unsigned char)(c & 0xFF)};
            fwrite(tri, 1, 3, f);
            (void)x;
        }
    }
    fclose(f);
    return 0;
}

int main(int argc, char **argv)
{
    const char *framePath = "artifacts/player/frame.bin";
    const char *keyPath = "artifacts/player/key.bin";
    const char *ppmPath = NULL;
    int once = 0;

    for (int i = 1; i < argc; i++)
    {
        if (i + 1 < argc && strcmp(argv[i], "--frame") == 0)
        {
            framePath = argv[++i];
        }
        else if (i + 1 < argc && strcmp(argv[i], "--key") == 0)
        {
            keyPath = argv[++i];
        }
        else if (i + 1 < argc && strcmp(argv[i], "--size") == 0)
        {
            if (sscanf(argv[++i], "%dx%d", &VW, &VH) != 2)
            {
                fprintf(stderr, "view: --size wants WxH\n");
                return 2;
            }
        }
        else if (strcmp(argv[i], "--ppm") == 0 && i + 1 < argc)
        {
            ppmPath = argv[++i];
        }
        else if (strcmp(argv[i], "--once") == 0)
        {
            once = 1;
        }
        else
        {
            fprintf(stderr,
                "usage: view [--frame <path>] [--key <path>] [--size WxH]"
                " [--ppm <path>] [--once]\n");
            return 2;
        }
    }
    if (VW < 64 || VH < 64 || VW > 4096 || VH > 4096)
    {
        fprintf(stderr, "view: window size out of 64..4096\n");
        return 2;
    }

    fb = (uint32_t *)malloc((size_t)VW * VH * 4);
    Payload p;
    p.count = 0;
    p.bx = (int64_t *)malloc(sizeof(int64_t) * 300);
    p.by = (int64_t *)malloc(sizeof(int64_t) * 300);
    p.br = (int64_t *)malloc(sizeof(int64_t) * 300);
    p.kind = (unsigned char *)malloc(300);
    if (!fb)
    {
        return 2;
    }

    /* ---- dump mode: no window, no display, proves the drawing path --------- */
    if (ppmPath)
    {
        int rc = read_payload(framePath, &p);
        if (rc != 0)
        {
            fprintf(stderr, "view: payload %s rejected rc=%d\n", framePath, rc);
            return 2;
        }
        draw(&p);
        if (dump_ppm(ppmPath) != 0)
        {
            return 2;
        }
        long nonbg = 0;
        uint32_t bg = pack_px(8, 10, 18);
        for (size_t i = 0; i < (size_t)VW * VH; i++)
        {
            if (fb[i] != bg)
            {
                nonbg++;
            }
        }
        fprintf(stderr,
            "view: ppm=%s size=%dx%d step=%lld eventRadiusPx=%lld ppuUsed=%lld "
            "bodies=%d nonbg=%ld\n",
            ppmPath, VW, VH, (long long)p.step,
            (long long)wu_to_px(p.holeEventRadius, g_ppuUsed),
            (long long)(g_ppuUsed >> 32), p.count, nonbg);
        return 0;
    }

    /* ---- windowed mode ----------------------------------------------------- */
    if (SDL_Init(SDL_INIT_VIDEO) != 0)
    {
        fprintf(stderr, "view: SDL_Init failed: %s\n", SDL_GetError());
        return 2;
    }
    SDL_Window *win = SDL_CreateWindow("Event Horizon (A-026 non-Unity client)",
        SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, VW, VH, 0);
    if (!win)
    {
        fprintf(stderr, "view: SDL_CreateWindow failed: %s\n", SDL_GetError());
        SDL_Quit();
        return 2;
    }
    SDL_Surface *wsurf = SDL_GetWindowSurface(win);
    SDL_Surface *src = SDL_CreateRGBSurfaceFrom(fb, VW, VH, 32, VW * 4,
        0x00FF0000u, 0x0000FF00u, 0x000000FFu, 0xFF000000u);
    if (!wsurf || !src)
    {
        fprintf(stderr, "view: surface setup failed: %s\n", SDL_GetError());
        SDL_DestroyWindow(win);
        SDL_Quit();
        return 2;
    }
    SDL_SetSurfaceBlendMode(src, SDL_BLENDMODE_NONE);

    int lastRc = 1;
    int quit = 0;
    int frames = 0;
    while (!quit)
    {
        SDL_Event ev;
        while (SDL_PollEvent(&ev))
        {
            if (ev.type == SDL_QUIT)
            {
                quit = 1;
            }
        }

        const Uint8 *kd = SDL_GetKeyboardState(NULL);
        int dx = 0, dy = 0;
        if (kd[SDL_SCANCODE_LEFT] || kd[SDL_SCANCODE_A])
        {
            dx -= 1;
        }
        if (kd[SDL_SCANCODE_RIGHT] || kd[SDL_SCANCODE_D])
        {
            dx += 1;
        }
        if (kd[SDL_SCANCODE_UP] || kd[SDL_SCANCODE_W])
        {
            dy -= 1;
        }
        if (kd[SDL_SCANCODE_DOWN] || kd[SDL_SCANCODE_S])
        {
            dy += 1;
        }
        int rateCode = (dx || dy) ? 20 : 10; /* InputDigest rate codes */
        if (kd[SDL_SCANCODE_ESCAPE])
        {
            quit = 1; /* ESC quits; SDL_KeyboardEvent has no scancode member,
                         the state array is indexed by scancode */
        }
        write_key(keyPath, dx, dy, rateCode, quit);

        lastRc = read_payload(framePath, &p);
        if (lastRc == 0)
        {
            draw(&p);
            frames++;
        }
        /* else: keep the previous frame on screen */

        SDL_UpperBlit(src, NULL, wsurf, NULL);
        SDL_UpdateWindowSurface(win);

        if (once)
        {
            break;
        }
        SDL_Delay(16); /* render-side pacing; the simulation paces itself */
    }

    fprintf(stderr, "view: window=%dx%d framesDrawn=%d lastPayloadRc=%d\n", VW, VH,
        frames, lastRc);
    SDL_DestroyWindow(win);
    SDL_Quit();
    return 0;
}
