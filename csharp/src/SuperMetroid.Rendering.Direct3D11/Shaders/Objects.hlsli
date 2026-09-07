// Raw OAM follows VRAM and CGRAM in the upload. No CPU sprite raster is supplied.
static const uint OamWordOffset = 16640;
static const uint4 ObjectSizes[8] = {
    uint4(8,8,16,16), uint4(8,8,32,32), uint4(8,8,64,64), uint4(16,16,32,32),
    uint4(16,16,64,64), uint4(32,32,64,64), uint4(16,32,32,64), uint4(16,32,32,32)
};

uint ReadOamByte(uint address)
{
    uint word = Memory[OamWordOffset + (address >> 2)];
    if ((address & 2) != 0) word >>= 16;
    if ((address & 1) != 0) word >>= 8;
    return word & 255;
}

uint2 ResolveObject(uint2 screen)
{
    // Ascending OAM order makes the first opaque pixel the winner, independently
    // of its BG priority. Insertion filters the winner later, not this scan.
    [loop]
    for (uint index = 0; index < ObjectCount; index++)
    {
        uint high = ReadOamByte(512 + index / 4) >> ((index % 4) * 2);
        uint4 sizeMode = ObjectSizes[(ObjectSelection >> 5) & 7];
        uint2 size = (high & 2) != 0 ? sizeMode.zw : sizeMode.xy;
        int x = (int)(ReadOamByte(index * 4) | ((high & 1) << 8));
        int y = (int)ReadOamByte(index * 4 + 1);
        if (x >= 256) x -= 512;
        if (y >= 224) y -= 256;
        int2 delta = int2(screen) - int2(x, y);
        if (delta.x < 0 || delta.y < 0 || delta.x >= (int)size.x || delta.y >= (int)size.y) continue;
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
        if (color == 0) continue;
        return uint2(Palette(128 + ((attributes >> 1) & 7) * 16 + color), (attributes >> 4) & 3);
    }
    return uint2(0, 255);
}
