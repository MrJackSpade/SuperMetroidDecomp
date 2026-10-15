// Raw OAM follows VRAM and CGRAM in the upload. No CPU sprite raster is supplied.
static const uint OamWordOffset = 16640;
static const uint4 ObjectSizes[8] = {
    uint4(8,8,16,16), uint4(8,8,32,32), uint4(8,8,64,64), uint4(16,16,32,32),
    uint4(16,16,64,64), uint4(32,32,64,64), uint4(16,32,32,64), uint4(16,32,32,32)
};

// Whole-frame sprite draws, packed by D3D11SpriteDrawPacking: word 0 is the part count, then four words per
// part in front-to-back order, then the bound artwork characters (eight words each).
StructuredBuffer<uint> SpriteDraws : register(t1);
static const uint SpritePartWords = 4;

uint ReadOamByte(uint address)
{
    uint word = Memory[OamWordOffset + (address >> 2)];
    if ((address & 2) != 0) word >>= 16;
    if ((address & 1) != 0) word >>= 8;
    return word & 255;
}

uint ReadArtworkByte(uint wordOffset, uint byteIndex)
{
    // Byte selection stays explicit for the same FXC /O3 reason as ReadByte.
    uint word = SpriteDraws[wordOffset + (byteIndex >> 2)];
    if ((byteIndex & 2) != 0) word >>= 16;
    if ((byteIndex & 1) != 0) word >>= 8;
    return word & 255;
}

uint SpritePartOrdinal(uint part)
{
    return SpriteDraws[1 + part * SpritePartWords + 2];
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
    uint character = SpriteDraws[record + 3] + ((sourceY >> 3) * 16 + (sourceX >> 3)) * 8;
    uint row = (sourceY & 7) * 2;
    uint shift = 7 - (sourceX & 7);
    uint color = ((ReadArtworkByte(character, row) >> shift) & 1) | (((ReadArtworkByte(character, row + 1) >> shift) & 1) << 1)
        | (((ReadArtworkByte(character, row + 16) >> shift) & 1) << 2) | (((ReadArtworkByte(character, row + 17) >> shift) & 1) << 3);
    if (color == 0) return false;
    palette = (shape >> 18) & 7;
    hit = uint2(Palette(128 + palette * 16 + color), (shape >> 21) & 3);
    return true;
}

bool ResolveOamSprite(uint index, uint2 screen, out uint2 hit, out uint palette)
{
    hit = uint2(0, 255);
    palette = 255;
    uint high = ReadOamByte(512 + index / 4) >> ((index % 4) * 2);
    uint4 sizeMode = ObjectSizes[(ObjectSelection >> 5) & 7];
    uint2 size = (high & 2) != 0 ? sizeMode.zw : sizeMode.xy;
    int x = (int)(ReadOamByte(index * 4) | ((high & 1) << 8));
    int y = (int)ReadOamByte(index * 4 + 1);
    if (x >= 256) x -= 512;
    if (y >= 224) y -= 256;
    int2 delta = int2(screen) - int2(x, y);
    if (delta.x < 0 || delta.y < 0 || delta.x >= (int)size.x || delta.y >= (int)size.y) return false;
    uint attributes = ReadOamByte(index * 4 + 3);
    uint sourceX = (attributes & 64) != 0 ? size.x - 1 - (uint)delta.x : (uint)delta.x;
    uint sourceY = (attributes & 128) != 0 ? size.y - 1 - (uint)delta.y : (uint)delta.y;
    uint name = ReadOamByte(index * 4 + 2) | ((attributes & 1) << 8);
    uint tile = (name & 256) | ((name + (sourceY >> 3) * 16 + (sourceX >> 3)) & 255);
    uint wordAddress = ((ObjectSelection & 7) << 13) + (tile & 255) * 16;
    if ((tile & 256) != 0) wordAddress += (((ObjectSelection >> 3) & 3) + 1) << 12;
    uint row = (wordAddress & 32767) * 2 + (sourceY & 7) * 2;
    uint shift = 7 - (sourceX & 7);
    uint color = ((ReadByte(row) >> shift) & 1) | (((ReadByte(row + 1) >> shift) & 1) << 1)
        | (((ReadByte(row + 16) >> shift) & 1) << 2) | (((ReadByte(row + 17) >> shift) & 1) << 3);
    if (color == 0) return false;
    palette = (attributes >> 1) & 7;
    hit = uint2(Palette(128 + palette * 16 + color), (attributes >> 4) & 3);
    return true;
}

uint2 ResolveObjectWithPalette(uint2 screen, out uint palette)
{
    palette = 255;
    uint2 hit;
    uint partCount = SpriteDraws[0];
    uint part = 0;
    // Front-to-back emission order makes the first opaque pixel the winner, independently
    // of its BG priority. Insertion filters the winner later, not this scan. A draw part is
    // in front of OAM sprite `ordinal` and every later one.
    [loop]
    for (uint index = 0; index < ObjectCount; index++)
    {
        [loop]
        for (; part < partCount && SpritePartOrdinal(part) <= index; part++)
            if (ResolveSpritePart(part, screen, hit, palette)) return hit;
        if (ResolveOamSprite(index, screen, hit, palette)) return hit;
    }
    [loop]
    for (; part < partCount; part++)
        if (ResolveSpritePart(part, screen, hit, palette)) return hit;
    palette = 255;
    return uint2(0, 255);
}

uint2 ResolveObject(uint2 screen)
{
    uint palette;
    return ResolveObjectWithPalette(screen, palette);
}
