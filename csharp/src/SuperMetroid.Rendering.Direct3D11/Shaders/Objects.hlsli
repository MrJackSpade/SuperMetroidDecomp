// Whole-frame sprite draws, packed by D3D11SpriteDrawBuffer: word 0 is the record count, then four words per image draw
// in front-to-back order (position, extent, flags, first pixel word), then the drawn images' pre-coloured pixels. No
// OAM, OBJ characters or VRAM is read (#1392).
StructuredBuffer<uint> SpriteDraws : register(t1);
static const uint SpriteRecordWords = 4;
static const uint SpriteFlipX = 1, SpriteFlipY = 2, SpriteRecolour = 512,
    SpritePaletteOverride = 8192;
// The palette row a blending image reports, so colour math treats it as the upper rows' sprites.
static const uint BlendingImageRow = 4;

bool ResolveSpritePart(uint part, uint2 screen, out uint2 hit, out uint palette)
{
    hit = uint2(0, 255);
    palette = 255;
    uint record = 1 + part * SpriteRecordWords;
    uint position = SpriteDraws[record];
    uint extent = SpriteDraws[record + 1];
    uint flags = SpriteDraws[record + 2];
    int x = ((int)(position << 16)) >> 16;
    int y = ((int)position) >> 16;
    uint2 size = uint2(extent & 65535, extent >> 16);
    int2 delta = int2(screen) - int2(x, y);
    if (delta.x < 0 || delta.y < 0 || delta.x >= (int)size.x || delta.y >= (int)size.y) return false;
    uint sourceX = (flags & SpriteFlipX) != 0 ? size.x - 1 - (uint)delta.x : (uint)delta.x;
    uint sourceY = (flags & SpriteFlipY) != 0 ? size.y - 1 - (uint)delta.y : (uint)delta.y;
    // A pre-coloured image: its pixel is the colour, and zero alpha is transparent. While a palette effect has changed a
    // row the frame uses, its words are palette indices and the frame shows the live colours.
    uint pixel = SpriteDraws[SpriteDraws[record + 3] + sourceY * (flags >> 16) + sourceX];
    // Each image pixel carries its own priority and colour math: above an index, which names its row, or in a colour's
    // alpha byte, as its priority plus one plus four where its row takes part in colour math.
    if ((flags & SpriteRecolour) != 0)
    {
        uint index = pixel & 255;
        if (index == 0) return false;
        palette = index >> 4;
        if ((flags & SpritePaletteOverride) != 0)
        {
            // The cartridge ORs the draw's palette bits into each part's own; the override takes the place of those
            // the image was coloured with, and its row decides colour math. A part flagged as naming its own row
            // keeps it, as frontend spritemaps place only their palette-inheriting parts in the draw's palette.
            uint part = (pixel >> 10) & 15;
            uint row = (part & 8) != 0 ? (part & 7) : (part | ((flags >> 10) & 7));
            index = row * 16 + (index & 15);
            palette = row;
        }
        hit = uint2(Palette(128 + index), (pixel >> 8) & 3);
        return true;
    }
    uint alpha = pixel >> 24;
    if (alpha == 0) return false;
    palette = alpha > BlendingImageRow ? BlendingImageRow : 0;
    hit = uint2(pixel | 0xff000000, (alpha - 1) & 3);
    return true;
}

uint2 ResolveObjectWithPalette(uint2 screen, out uint palette)
{
    palette = 255;
    uint2 hit;
    uint partCount = SpriteDraws[0];
    // Front-to-back emission order makes the first opaque pixel the winner, independently
    // of its BG priority. Insertion filters the winner later, not this scan.
    [loop]
    for (uint part = 0; part < partCount; part++)
        if (ResolveSpritePart(part, screen, hit, palette)) return hit;
    palette = 255;
    return uint2(0, 255);
}

uint2 ResolveObject(uint2 screen)
{
    uint palette;
    return ResolveObjectWithPalette(screen, palette);
}
