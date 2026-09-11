// Literal X-ray Mode-1 composition. Header/scanline packing is documented by
// DispatchXrayGameplay; no CPU-colored pixels or priority raster are uploaded.
// Word addresses shared with SnesPpuLayout's gameplay tilemap definitions.
static const uint XrayGameplayBg1MapWord = 20480;
static const uint XrayGameplayHudMapWord = 22528;
// D3D11GameplaySubscreenKind header contract.
static const uint SubscreenCapturedBg3 = 1;
static const uint SubscreenGameplayBg2 = 2;
uint2 XrayBackground(uint map, uint chars, uint2 size, uint2 position, bool fourBit, bool opaqueZero)
{
    position &= size * 8 - 1;
    uint2 tile = position >> 3;
    uint page = ((tile.y >> 5) * (size.x >> 5) + (tile.x >> 5)) * 1024;
    uint entry = ReadWord(map + page + (tile.y & 31) * 32 + (tile.x & 31));
    uint px = (entry & 16384) != 0 ? 7 - (position.x & 7) : position.x & 7;
    uint py = (entry & 32768) != 0 ? 7 - (position.y & 7) : position.y & 7;
    uint row = ((chars + (entry & 1023) * (fourBit ? 16 : 8)) & 32767) * 2 + py * 2;
    uint shift = 7 - px;
    uint color = ((ReadByte(row) >> shift) & 1) | (((ReadByte(row + 1) >> shift) & 1) << 1);
    if (fourBit) color |= (((ReadByte(row + 16) >> shift) & 1) << 2) | (((ReadByte(row + 17) >> shift) & 1) << 3);
    return uint2(color != 0 || opaqueZero ? Palette(((entry >> 10) & 7) * (fourBit ? 16 : 4) + color) : 0, (entry >> 13) & 1);
}

void XrayInsert(uint2 candidate, int candidateRank, uint candidateSource,
    inout uint winner, inout int rank, inout uint source)
{
    if ((candidate.x >> 24) == 0 || candidateRank < rank) return;
    winner = candidate.x; rank = candidateRank; source = candidateSource;
}

uint XrayGameplay(uint2 screen)
{
    uint winner = Palette(0);
    if (screen.y < 32)
    {
        uint2 hud = XrayBackground(XrayGameplayHudMapWord, (uint)MatrixB, uint2(32,32), screen, false, true);
        winner = hud.x;
    }
    else
    {
    uint4 scan = ScanlineParameters[screen.y];
    bool inside = screen.x >= (scan.z & 255) && screen.x <= ((scan.z >> 8) & 255);
    uint source = 32;
    int rank = -1;
    if ((TransparentZero == 0 || inside) && (PriorityFilter & 2) != 0)
    {
        uint2 bg = XrayBackground((uint)MatrixA, CharacterWord, uint2(MapWidth,MapHeight), screen + scan.xy, true, false);
        XrayInsert(bg, bg.y != 0 ? 5 : 2, 2, winner, rank, source);
    }
    if ((TransparentZero == 0 || !inside) && (PriorityFilter & 1) != 0)
    {
        uint2 bg = XrayBackground(XrayGameplayBg1MapWord, TilemapWord, uint2(64,32), screen + uint2(HorizontalScroll,VerticalScroll), true, false);
        XrayInsert(bg, bg.y != 0 ? 6 : 3, 1, winner, rank, source);
    }
    if ((PriorityFilter & 16) != 0)
    {
        uint palette;
        uint2 obj = ResolveObjectWithPalette(screen, palette);
        int objRank = obj.y < 2 ? (int)obj.y : obj.y == 2 ? 4 : 7;
        XrayInsert(obj, objRank, palette >= 4 ? 16 : 0, winner, rank, source);
    }
    if (!inside && (Level & source) != 0)
    {
        uint sub = 0;
        if (OffsetX != 0 && Reserved3 == SubscreenGameplayBg2)
            sub = XrayBackground((uint)MatrixA, CharacterWord, uint2(MapWidth,MapHeight), screen + scan.xy, true, false).x;
        if (OffsetX != 0 && Reserved3 == SubscreenCapturedBg3 && screen.y >= (uint)CenterY)
            sub = XrayBackground((uint)MatrixC, (uint)MatrixD, uint2(32,(uint)CenterX),
                screen + uint2(scan.w & 65535, scan.w >> 16), false, false).x;
        bool useSub = OffsetX != 0 && (sub >> 24) != 0;
        int3 other = useSub ? int3(Unpack(sub) >> 3) : int3(AddR,AddG,AddB);
        int3 value = int3(Unpack(winner) >> 3);
        value = (Level & 128) != 0 ? value - other : value + other;
        if ((Level & 64) != 0 && (OffsetX == 0 || useSub)) value >>= 1;
        uint3 c = (uint3)clamp(value, 0, 31);
        winner = Pack((c << 3) | (c >> 2), winner >> 24);
    }
    }
    return winner;
}
