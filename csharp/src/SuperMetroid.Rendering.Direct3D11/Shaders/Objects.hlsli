// Whole-frame sprite draws, packed by D3D11SpriteDrawBuffer: word 0 is the record count, then five words per image draw
// in front-to-back order (position, extent, flags, first pixel word, first colour word), then the drawn images'
// pixels, then each indexed draw's colours. No OAM, OBJ characters, VRAM or frame palette is read (#1392, #1405).
StructuredBuffer<uint> SpriteDraws : register(t1);
static const uint SpriteRecordWords = 5;
static const uint SpriteFlipX = 1, SpriteFlipY = 2, SpriteRecolour = 512;
static const uint SpriteRowColours = 16, SpriteFollowsPartRows = 65536;
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
    // A pre-coloured image: its pixel is the colour, and zero alpha is transparent. While a draw shows it in other
    // colours, its words are palette indices and the draw's colours give each part row's colours and blending.
    uint pixel = SpriteDraws[SpriteDraws[record + 3] + sourceY * (flags >> 16) + sourceX];
    // Each image pixel carries its own priority and colour math: above an index, which names its row, or in a colour's
    // alpha byte, as its priority plus one plus four where its row takes part in colour math.
    if ((flags & SpriteRecolour) != 0)
    {
        uint index = pixel & 255;
        if (index == 0) return false;
        // A pixel takes its ink row's colours, or its part row's where the draw follows part rows.
        uint colours = SpriteDraws[record + 4];
        uint header = SpriteDraws[colours];
        uint key = (header & SpriteFollowsPartRows) != 0 ? (pixel >> 10) & 15 : index >> 4;
        palette = ((header >> key) & 1) != 0 ? BlendingImageRow : 0;
        hit = uint2(SpriteDraws[colours + 1 + key * SpriteRowColours + (index & 15)], (pixel >> 8) & 3);
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
