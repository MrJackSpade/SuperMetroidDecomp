// These operation IDs and the packed memory layout match D3D11ShaderLayout.cs.
static const uint OpBackdrop = 0, OpBg4 = 1, OpBg2 = 2, OpBrightness = 3, OpFixedAdd = 4;
static const uint OpResolveObj = 5, OpInsertObj = 6;
static const uint OpMode7 = 7;
static const uint OpScanlineAdd = 8;
static const uint OpBgAdd = 9, OpBgSubtract = 10;
static const uint OpSubscreenAdd = 11;
static const uint OpMessage = 12;
static const uint OpXrayHalfColor = 13;
static const uint OpXrayGameplay = 14;
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
    uint Reserved27;
    uint WindowSelection, WindowLogic, WindowEdges, WindowEnabled;
    uint4 ScanlineParameters[224];
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
#include "Xray.hlsli"

bool MainCoverage(uint2 screen)
{
    uint4 geometry = ScanlineParameters[1];
    if (geometry.w == 0) return true;
    uint4 map = ScanlineParameters[0];
    uint2 position = (screen + map.zw) & (geometry.xy * 8 - 1);
    uint2 tile = position >> 3;
    uint page = ((tile.y >> 5) * (geometry.x >> 5) + (tile.x >> 5)) * 1024;
    uint entry = ReadWord(map.x + page + (tile.y & 31) * 32 + (tile.x & 31));
    if (geometry.z != 0 && ((entry >> 13) & 1) != geometry.z - 1) return false;
    uint px = (entry & 16384) != 0 ? 7 - (position.x & 7) : position.x & 7;
    uint py = (entry & 32768) != 0 ? 7 - (position.y & 7) : position.y & 7;
    uint row = ((map.y + (entry & 1023) * 16) & 32767) * 2 + py * 2;
    bool covered = (((ReadByte(row) | ReadByte(row + 1) | ReadByte(row + 16) | ReadByte(row + 17)) >> (7 - px)) & 1) != 0;
    if (AddG != 0)
    {
        uint palette;
        uint2 obj = ResolveObjectWithPalette(screen, palette);
        if (obj.y != 255 && (!covered || obj.y >= ((entry & 8192) != 0 ? 3 : 2)))
            return palette >= 4;
    }
    return covered;
}

// Literal PPU window membership. Main/subscreen admission is resolved per draw;
// the selected plane is skipped before it can overwrite a lower-priority pixel.
bool HardwareWindowMasked(uint x)
{
    bool firstEnabled = (WindowSelection & 2) != 0;
    bool secondEnabled = (WindowSelection & 8) != 0;
    bool first = x >= (WindowEdges & 255) && x <= ((WindowEdges >> 8) & 255);
    bool second = x >= ((WindowEdges >> 16) & 255) && x <= (WindowEdges >> 24);
    if ((WindowSelection & 1) != 0) first = !first;
    if ((WindowSelection & 4) != 0) second = !second;
    if (!firstEnabled) return secondEnabled && second;
    if (!secondEnabled) return first;
    if (WindowLogic == 0) return first || second;
    if (WindowLogic == 1) return first && second;
    if (WindowLogic == 2) return first != second;
    return first == second;
}

[numthreads(8, 8, 1)]
void Main(uint3 id : SV_DispatchThreadID)
{
    if (id.x >= 256 || id.y >= 224) return;
    if (id.y < FirstScanline || id.y >= EndScanline) return;
    if (WindowEnabled != 0 && HardwareWindowMasked(id.x)) return;
    if (Operation == OpXrayGameplay) { Output[id.xy] = XrayGameplay(id.xy); return; }
    if (Operation == OpXrayHalfColor)
    {
        uint pixel = Output[id.xy];
        // Keep the addition carry until after halving, as the PPU does.
        uint3 c = min(31, ((Unpack(pixel) >> 3) + 7) >> 1);
        Output[id.xy] = Pack((c << 3) | (c >> 2), pixel >> 24);
        return;
    }
    if (Operation == OpMessage)
    {
        if (id.y < TilemapWord || id.y >= TilemapWord + HorizontalScroll * 8) return;
        uint y = id.y - TilemapWord;
        uint entry = ScanlineParameters[(y >> 3) * 32 + (id.x >> 3)].x;
        uint px = (entry & 16384) != 0 ? 7 - (id.x & 7) : id.x & 7;
        uint py = (entry & 32768) != 0 ? 7 - (y & 7) : y & 7;
        uint row = ((CharacterWord + (entry & 1023) * 8) & 32767) * 2 + py * 2;
        uint color = ((ReadByte(row) >> (7 - px)) & 1) | (((ReadByte(row + 1) >> (7 - px)) & 1) << 1);
        if (color == 0) return;
        uint index = ((entry >> 10) & 7) * 4 + color;
        if (index == AddB || index == ObjectCount)
        {
            uint word = index == AddB ? AddR : AddG;
            uint3 c = uint3(word & 31, (word >> 5) & 31, (word >> 10) & 31);
            Output[id.xy] = Pack((c << 3) | (c >> 2), 255);
        }
        else Output[id.xy] = Palette(index);
        return;
    }
    if (Operation == OpScanlineAdd)
    {
        uint4 window = ScanlineParameters[id.y];
        uint left = window.x & 255, right = (window.x >> 8) & 255;
        if (id.x >= left && id.x <= right)
        {
            uint pixel = Output[id.xy];
            Output[id.xy] = Pack(min(255, Unpack(pixel) + window.yzw), pixel >> 24);
        }
        return;
    }
    if (Operation == OpMode7)
    {
        int cx = (CenterX << 19) >> 19, cy = (CenterY << 19) >> 19;
        int h = ((OffsetX << 19) >> 19) - cx, v = ((OffsetY << 19) >> 19) - cy;
        h = (h & 8192) != 0 ? h | ~1023 : h & 1023;
        v = (v & 8192) != 0 ? v | ~1023 : v & 1023;
        int physicalY = (int)id.y + 1;
        int startX = ((MatrixA * h) & ~63) + ((MatrixB * physicalY) & ~63) + ((MatrixB * v) & ~63) + (cx << 8);
        int startY = ((MatrixC * h) & ~63) + ((MatrixD * physicalY) & ~63) + ((MatrixD * v) & ~63) + (cy << 8);
        int x = (startX + MatrixA * (int)id.x) >> 8;
        int y = (startY + MatrixC * (int)id.x) >> 8;
        bool outside = (uint)x >= 1024 || (uint)y >= 1024;
        if (outside && FillCharacterZero == 0) return;
        uint wrappedX = (uint)x & 1023, wrappedY = (uint)y & 1023;
        uint character = outside ? 0 : ReadByte(((wrappedY >> 3) * 128 + (wrappedX >> 3)) * 2);
        uint color = ReadByte((character * 64 + (wrappedY & 7) * 8 + (wrappedX & 7)) * 2 + 1);
        if (color != 0)
        {
            uint main = Palette(color);
            uint2 sub = Objects[id.xy];
            if (Reserved27 != 0 && sub.y != 255)
            {
                int3 difference = max(0, (int3)(Unpack(main) >> 3) - (int3)(Unpack(sub.x) >> 3));
                uint3 result = (uint3)difference;
                main = Pack((result << 3) | (result >> 2), 255);
            }
            Output[id.xy] = main;
        }
        return;
    }
    if (Operation == OpResolveObj) { Objects[id.xy] = ResolveObject(id.xy); return; }
    if (Operation == OpInsertObj)
    {
        uint2 winner = Objects[id.xy];
        if (winner.y != 255 && (PriorityFilter == 0 || PriorityFilter - 1 == winner.y))
        {
            // InsertObj's AddR flag selects OBJ on the additive subscreen. Arithmetic
            // is in native five-bit components, before the final master-brightness pass.
            if (AddR != 0)
            {
                uint3 color = min(31, (Unpack(Output[id.xy]) >> 3) + (Unpack(winner.x) >> 3));
                Output[id.xy] = Pack((color << 3) | (color >> 2), 255);
            }
            else if (AddG != 0)
            {
                uint palette;
                ResolveObjectWithPalette(id.xy, palette);
                uint3 fixedColor = uint3(AddB & 31, (AddB >> 5) & 31, (AddB >> 10) & 31);
                uint3 color = min(31, (Unpack(winner.x) >> 3) + fixedColor);
                Output[id.xy] = palette >= 4 ? Pack((color << 3) | (color >> 2), 255) : winner.x;
            }
            else Output[id.xy] = winner.x;
        }
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
    bool backgroundMath = Operation == OpBgAdd || Operation == OpBgSubtract;
    uint2 scroll = (backgroundMath || Reserved3 != 0) ? ScanlineParameters[id.y].xy : uint2(HorizontalScroll, VerticalScroll);
    uint x = (id.x + scroll.x) & (MapWidth * 8 - 1);
    uint y = (id.y + scroll.y) & (MapHeight * 8 - 1);
    uint tileX = x >> 3, tileY = y >> 3;
    uint page = ((tileY >> 5) * (MapWidth >> 5) + (tileX >> 5)) * 1024;
    uint entry = ReadWord(TilemapWord + page + (tileY & 31) * 32 + (tileX & 31));
    if (PriorityFilter != 0 && ((entry >> 13) & 1) != PriorityFilter - 1) return;
    uint sourceX = (entry & 16384) != 0 ? 7 - (x & 7) : x & 7;
    uint sourceY = (entry & 32768) != 0 ? 7 - (y & 7) : y & 7;
    bool fourBpp = Operation == OpBg4 || (Operation == OpSubscreenAdd && Reserved27 != 0);
    uint stride = fourBpp ? 16 : 8;
    uint row = ((CharacterWord + (entry & 1023) * stride) & 32767) * 2 + sourceY * 2;
    uint shift = 7 - sourceX;
    uint color = ((ReadByte(row) >> shift) & 1) | (((ReadByte(row + 1) >> shift) & 1) << 1);
    if (fourBpp)
        color |= (((ReadByte(row + 16) >> shift) & 1) << 2) | (((ReadByte(row + 17) >> shift) & 1) << 3);
    bool objectSubscreen = Operation == OpSubscreenAdd && AddR != 0;
    uint2 subObject = objectSubscreen ? Objects[id.xy] : uint2(0, 255);
    bool objectWins = objectSubscreen && subObject.y != 255 &&
        (color == 0 || subObject.y >= ((entry & 8192) != 0 ? 3 : 2));
    if (color == 0 && TransparentZero != 0 && !objectWins) return;
    uint sampled = Palette(((entry >> 10) & 7) * (fourBpp ? 16 : 4) + color);
    if (objectWins) sampled = subObject.x;
    if (Operation == OpSubscreenAdd)
    {
        if (!MainCoverage(id.xy)) return;
        uint3 sum = min(31, (Unpack(Output[id.xy]) >> 3) + (Unpack(sampled) >> 3));
        Output[id.xy] = Pack((sum << 3) | (sum >> 2), 255);
    }
    else if (backgroundMath)
    {
        uint pixel = Output[id.xy];
        int3 baseColor = int3(Unpack(pixel)), overlay = int3(Unpack(sampled));
        int3 result = Operation == OpBgAdd ? min(255, baseColor + overlay) : max(0, baseColor - overlay);
        Output[id.xy] = Pack(uint3(result), pixel >> 24);
    }
    else Output[id.xy] = sampled;
}
