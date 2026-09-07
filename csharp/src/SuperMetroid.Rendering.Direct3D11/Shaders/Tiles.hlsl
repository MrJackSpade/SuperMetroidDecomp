// These operation IDs and the packed memory layout match D3D11ShaderLayout.cs.
static const uint OpBackdrop = 0, OpBg4 = 1, OpBg2 = 2, OpBrightness = 3, OpFixedAdd = 4;
static const uint OpResolveObj = 5, OpInsertObj = 6;
static const uint OpMode7 = 7;
static const uint PaletteOffset = 16384;
cbuffer TileParameters : register(b0)
{
    uint Operation, TilemapWord, CharacterWord, HorizontalScroll;
    uint VerticalScroll, MapWidth, MapHeight, PriorityFilter;
    uint TransparentZero, Level, AddR, AddG;
    uint AddB, ObjectCount, ObjectSelection, Reserved3;
    int MatrixA, MatrixB, MatrixC, MatrixD;
    int CenterX, CenterY, OffsetX, OffsetY;
    uint FillCharacterZero, FirstScanline, EndScanline;
};
StructuredBuffer<uint> Memory : register(t0);
RWTexture2D<uint> Output : register(u0);
RWTexture2D<uint2> Objects : register(u1);

uint ReadByte(uint address)
{
    address &= 65535;
    // Keep the two byte-selection bits explicit. With the pinned FXC /O3,
    // (word >> ((address & 3) * 8)) miscompiled the inlined row+17 read:
    // bytecode selected bits 8..15 even when address&3 was 3. The focused
    // fourth-plane fixture and packed-VRAM pixel suite guard this boundary.
    uint word = Memory[address >> 2];
    if ((address & 2) != 0) word >>= 16;
    if ((address & 1) != 0) word >>= 8;
    return word & 255;
}
uint ReadWord(uint address)
{
    address = (address & 32767) * 2;
    return ReadByte(address) | (ReadByte(address + 1) << 8);
}
uint Pack(uint3 c, uint alpha) { return c.r | (c.g << 8) | (c.b << 16) | (alpha << 24); }
uint3 Unpack(uint c) { return uint3(c & 255, (c >> 8) & 255, (c >> 16) & 255); }
uint Palette(uint index)
{
    uint word = Memory[PaletteOffset + index];
    uint3 c = uint3(word & 31, (word >> 5) & 31, (word >> 10) & 31);
    return Pack((c << 3) | (c >> 2), 255);
}

#include "Objects.hlsli"

[numthreads(8, 8, 1)]
void Main(uint3 id : SV_DispatchThreadID)
{
    if (id.x >= 256 || id.y >= 224) return;
    if (id.y < FirstScanline || id.y >= EndScanline) return;
    if (Operation == OpMode7)
    {
        int relativeX = (int)id.x + OffsetX - CenterX;
        int relativeY = (int)id.y + OffsetY - CenterY;
        int x = ((MatrixA * relativeX + MatrixB * relativeY) >> 8) + CenterX;
        int y = ((MatrixC * relativeX + MatrixD * relativeY) >> 8) + CenterY;
        bool outside = (uint)x >= 1024 || (uint)y >= 1024;
        if (outside && FillCharacterZero == 0) return;
        uint wrappedX = (uint)x & 1023, wrappedY = (uint)y & 1023;
        uint character = outside ? 0 : ReadByte(((wrappedY >> 3) * 128 + (wrappedX >> 3)) * 2);
        uint color = ReadByte((character * 64 + (wrappedY & 7) * 8 + (wrappedX & 7)) * 2 + 1);
        if (color != 0) Output[id.xy] = Palette(color);
        return;
    }
    if (Operation == OpResolveObj) { Objects[id.xy] = ResolveObject(id.xy); return; }
    if (Operation == OpInsertObj)
    {
        uint2 winner = Objects[id.xy];
        if (winner.y != 255 && (PriorityFilter == 0 || PriorityFilter - 1 == winner.y)) Output[id.xy] = winner.x;
        return;
    }
    if (Operation == OpBackdrop) { Output[id.xy] = Palette(0); return; }
    if (Operation == OpBrightness)
    {
        uint packed = Output[id.xy];
        if ((packed >> 24) != 0) Output[id.xy] = Pack(Unpack(packed) * Level / 15, packed >> 24);
        return;
    }
    if (Operation == OpFixedAdd)
    {
        uint3 c = min(31, (Unpack(Output[id.xy]) * 31 + 127) / 255 + uint3(AddR, AddG, AddB));
        Output[id.xy] = Pack((c << 3) | (c >> 2), 255); return;
    }
    uint x = (id.x + HorizontalScroll) & (MapWidth * 8 - 1);
    uint y = (id.y + VerticalScroll) & (MapHeight * 8 - 1);
    uint tileX = x >> 3, tileY = y >> 3;
    uint page = ((tileY >> 5) * (MapWidth >> 5) + (tileX >> 5)) * 1024;
    uint entry = ReadWord(TilemapWord + page + (tileY & 31) * 32 + (tileX & 31));
    if (PriorityFilter != 0 && ((entry >> 13) & 1) != PriorityFilter - 1) return;
    uint sourceX = (entry & 16384) != 0 ? 7 - (x & 7) : x & 7;
    uint sourceY = (entry & 32768) != 0 ? 7 - (y & 7) : y & 7;
    uint stride = Operation == OpBg4 ? 16 : 8;
    uint row = ((CharacterWord + (entry & 1023) * stride) & 32767) * 2 + sourceY * 2;
    uint shift = 7 - sourceX;
    uint color = ((ReadByte(row) >> shift) & 1) | (((ReadByte(row + 1) >> shift) & 1) << 1);
    if (Operation == OpBg4)
        color |= (((ReadByte(row + 16) >> shift) & 1) << 2) | (((ReadByte(row + 17) >> shift) & 1) << 3);
    if (color == 0 && TransparentZero != 0) return;
    Output[id.xy] = Palette(((entry >> 10) & 7) * (Operation == OpBg4 ? 16 : 4) + color);
}
