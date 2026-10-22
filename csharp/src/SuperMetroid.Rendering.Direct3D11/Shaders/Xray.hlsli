// Literal X-ray Mode-1 composition. Header/scanline packing is documented by
// DispatchXrayGameplay; no CPU-colored pixels or priority raster are uploaded.
// D3D11GameplaySubscreenKind header contract.
static const uint SubscreenCapturedBg3 = 1;
static const uint SubscreenGameplayBg2 = 2;
static const uint SubscreenGameplayBg2AndCapturedBg3 = 3;
// A room source (RoomTile), the BG3 plane from row `map`, or the VRAM tilemap at `map`; a room position the room
// does not cover is transparent. Words written over BG2 cover it when roomSource carries RoomSourceBg2Overlay.
uint2 XrayBackground(uint roomSource, uint map, uint chars, uint2 size, uint2 position, bool fourBit, bool opaqueZero)
{
    uint entry = 0;
    uint picture = 0;
    uint2 within = uint2(0, 0);
    bool shown = true;
    uint source = roomSource & 255;
    uint written = 0;
    if ((roomSource & RoomSourceBg2Overlay) != 0 && Bg2OverlayWord(position & 65535, size, written))
    {
        entry = written;
        within = position & 7;
    }
    else if (source == RoomSourceBg3)
        entry = Bg3Word(map, size.y, position & 65535, within);
    else if (source == RoomSourceBossBg2)
        entry = BossBg2Word(position, size, within, picture);
    else if (source == RoomSourceXrayReveal)
        shown = RevealPicture(position & 65535, picture, within);
    else if (source == RoomSourceBg2Image)
        shown = Bg2ImagePicture(position & 65535, size, picture, within);
    else if (source != 0)
        shown = RoomTile(source, position, entry, within, picture);
    else
    {
        position &= size * 8 - 1;
        uint2 tile = position >> 3;
        uint page = ((tile.y >> 5) * (size.x >> 5) + (tile.x >> 5)) * 1024;
        entry = ReadWord(map + page + (tile.y & 31) * 32 + (tile.x & 31));
        within = position & 7;
    }
    uint2 sample = uint2(0, 0);
    if (picture != 0)
    {
        // A block picture (#1392) keeps no row for its transparent pixels, which show nothing.
        uint index;
        bool high;
        uint colour = PicturePixel(picture, within, index, high);
        if (shown && index != 0) sample = uint2(colour, high ? 1 : 0);
    }
    else
    {
        uint px = (entry & 16384) != 0 ? 7 - within.x : within.x;
        uint py = (entry & 32768) != 0 ? 7 - within.y : within.y;
        uint row = ((chars + (entry & 1023) * (fourBit ? 16 : 8)) & 32767) * 2 + py * 2;
        uint shift = 7 - px;
        uint color = ((ReadByte(row) >> shift) & 1) | (((ReadByte(row + 1) >> shift) & 1) << 1);
        if (fourBit) color |= (((ReadByte(row + 16) >> shift) & 1) << 2) | (((ReadByte(row + 17) >> shift) & 1) << 3);
        if (shown) sample = uint2(color != 0 || opaqueZero ? Palette(((entry >> 10) & 7) * (fourBit ? 16 : 4) + color) : 0, (entry >> 13) & 1);
    }
    return sample;
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
        uint2 hud = XrayBackground(RoomSourceBg3, 0, (uint)MatrixB, uint2(32,32), screen, false, true);
        winner = hud.x;
    }
    else
    {
    uint4 scan = ScanlineParameters[screen.y];
    uint mainScreen = scan.z >> 16;
    uint2 bg2Screen = screen;
    if (Reserved27 > 1) bg2Screen -= bg2Screen % Reserved27;
    bool inside = screen.x >= (scan.z & 255) && screen.x <= ((scan.z >> 8) & 255);
    uint source = 32;
    int rank = -1;
    if ((TransparentZero == 0 || inside) && (mainScreen & 2) != 0)
    {
        uint2 bg = XrayBackground(RoomSource, (uint)MatrixA, CharacterWord, uint2(MapWidth,MapHeight), bg2Screen + scan.xy, true, false);
        XrayInsert(bg, bg.y != 0 ? 5 : 2, 2, winner, rank, source);
    }
    if ((TransparentZero == 0 || !inside) && (mainScreen & 1) != 0)
    {
        uint2 bg = XrayBackground(RoomSourceBg1, 0, TilemapWord, uint2(64,32), screen + uint2(HorizontalScroll,VerticalScroll), true, false);
        XrayInsert(bg, bg.y != 0 ? 6 : 3, 1, winner, rank, source);
    }
    if ((mainScreen & 16) != 0)
    {
        uint palette;
        uint2 obj = ResolveObjectWithPalette(screen, palette);
        int objRank = obj.y < 2 ? (int)obj.y : obj.y == 2 ? 4 : 7;
        XrayInsert(obj, objRank, palette >= 4 ? 16 : 0, winner, rank, source);
    }
    if (!inside && (Level & source) != 0)
    {
        uint sub = 0;
        if (OffsetX != 0 && (Reserved3 == SubscreenGameplayBg2 || Reserved3 == SubscreenGameplayBg2AndCapturedBg3))
            sub = XrayBackground(RoomSource, (uint)MatrixA, CharacterWord, uint2(MapWidth,MapHeight), bg2Screen + scan.xy, true, false).x;
        if (OffsetX != 0 && (Reserved3 == SubscreenCapturedBg3 || Reserved3 == SubscreenGameplayBg2AndCapturedBg3) && screen.y >= (uint)CenterY)
        {
            uint2 bg3 = XrayBackground(RoomSourceBg3, (uint)MatrixC, (uint)MatrixD, uint2(32,(uint)CenterX),
                screen + uint2(scan.w & 65535, scan.w >> 16), false, false);
            if ((bg3.x >> 24) != 0 && (bg3.y != 0 || (sub >> 24) == 0)) sub = bg3.x;
        }
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
