// Whole-frame sprite draws, packed by D3D11SpriteDrawBuffer: word 0 is the part count, then three words per part in
// front-to-back order, then the bound artwork characters (eight words each). No OAM or OBJ VRAM is read.
StructuredBuffer<uint> SpriteDraws : register(t1);
static const uint SpritePartWords = 3;

uint ReadArtworkByte(uint wordOffset, uint byteIndex)
{
    // Byte selection stays explicit for the same FXC /O3 reason as ReadByte.
    uint word = SpriteDraws[wordOffset + (byteIndex >> 2)];
    if ((byteIndex & 2) != 0) word >>= 16;
    if ((byteIndex & 1) != 0) word >>= 8;
    return word & 255;
}

bool ResolveSpritePart(uint part, uint2 screen, out uint2 hit, out uint palette)
{
    hit = uint2(0, 255);
    palette = 255;
    uint record = 1 + part * SpritePartWords;
    uint position = SpriteDraws[record];
    uint shape = SpriteDraws[record + 1];
    int x = ((int)(position << 16)) >> 16;
    int y = ((int)position) >> 16;
    uint2 size = uint2(shape & 255, (shape >> 8) & 255);
    int2 delta = int2(screen) - int2(x, y);
    if (delta.x < 0 || delta.y < 0 || delta.x >= (int)size.x || delta.y >= (int)size.y) return false;
    uint sourceX = (shape & 65536) != 0 ? size.x - 1 - (uint)delta.x : (uint)delta.x;
    uint sourceY = (shape & 131072) != 0 ? size.y - 1 - (uint)delta.y : (uint)delta.y;
    // Characters sit sixteen to a row of the artwork sheet, eight words each.
    uint character = SpriteDraws[record + 2] + ((sourceY >> 3) * 16 + (sourceX >> 3)) * 8;
    uint row = (sourceY & 7) * 2;
    uint shift = 7 - (sourceX & 7);
    uint color = ((ReadArtworkByte(character, row) >> shift) & 1) | (((ReadArtworkByte(character, row + 1) >> shift) & 1) << 1)
        | (((ReadArtworkByte(character, row + 16) >> shift) & 1) << 2) | (((ReadArtworkByte(character, row + 17) >> shift) & 1) << 3);
    if (color == 0) return false;
    palette = (shape >> 18) & 7;
    hit = uint2(Palette(128 + palette * 16 + color), (shape >> 21) & 3);
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
