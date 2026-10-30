// A room under the X-ray: its layers and sprites in their order, the reveal's window (Window.hlsli), the layers each line shows, and
// the blend with the fixed colour or what lies behind. D3D11FrameRenderer.DispatchXrayRoom packs the header and lines;
// D3D11XrayBehindLayers names what lies behind.
static const uint BehindLayer3 = 1;
static const uint BehindLayer2 = 2;
static const uint BehindLayer2AndLayer3 = 3;
// A room layer (RoomTile) or the layer-3 plane from row `map`; a position the room does not cover is transparent.
// Words written over layer 2 cover it when roomSource carries SourceLayer2Overlay.
uint2 XrayBackground(uint roomSource, uint map, uint2 size, uint2 position)
{
    uint entry = 0;
    uint picture = 0;
    uint2 within = uint2(0, 0);
    bool shown = true;
    uint source = roomSource & 255;
    bool overlaid = (roomSource & SourceLayer2Overlay) != 0 && Bg2OverlayPicture(position & 65535, size, picture, within);
    if (!overlaid)
    {
        if (source == SourceLayer3)
            shown = Bg3Picture(map, size.y, position & 65535, within, picture);
        else if (source == SourceBossBody)
        {
            BossBodyPicture(position, size, within, picture);
            shown = picture != 0;
        }
        else if (source == SourceXrayReveal)
            shown = RevealPicture(position & 65535, picture, within);
        else if (source == SourceLibraryBackground)
            shown = Bg2ImagePicture(position & 65535, size, picture, within);
        else
            shown = RoomTile(source, position, entry, within, picture);
    }
    uint2 sample = uint2(0, 0);
    // Every background draws from pictures (#1403); a position without one shows nothing.
    if (picture != 0)
    {
        // A block picture (#1392) keeps no row for its transparent pixels, which show nothing.
        uint index;
        bool high;
        uint colour = PicturePixel(picture, within, index, high);
        if (shown && index != 0) sample = uint2(colour, high ? 1 : 0);
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
    uint winner = Backdrop();
    // The HUD's band shows no room; the HUD's own layer draws over it (#1466).
    if (screen.y >= 32)
    {
    uint4 scan = ScanlineParameters[screen.y];
    uint mainScreen = scan.z >> 16;
    uint2 bg2Screen = screen;
    if (Reserved27 > 1) bg2Screen -= bg2Screen % Reserved27;
    // Layer 2's line scroll, from its described line scrolls (#1468).
    uint2 bg2Scroll = LineScrollPair(SlotHorizontal, SlotVertical, screen.y);
    // Inside the window (the X-ray cone, #1468) the room shows unblended.
    bool inside = InWindow(screen);
    uint source = 32;
    int rank = -1;
    if ((TransparentZero == 0 || inside) && (mainScreen & 2) != 0)
    {
        uint2 bg = XrayBackground(RoomSource, (uint)MatrixA, uint2(MapWidth,MapHeight), bg2Screen + bg2Scroll);
        XrayInsert(bg, bg.y != 0 ? 5 : 2, 2, winner, rank, source);
    }
    if ((TransparentZero == 0 || !inside) && (mainScreen & 1) != 0)
    {
        uint2 bg = XrayBackground(SourceLayer1, 0, uint2(64,32), screen + uint2(HorizontalScroll,VerticalScroll));
        XrayInsert(bg, bg.y != 0 ? 6 : 3, 1, winner, rank, source);
    }
    if ((mainScreen & 16) != 0)
    {
        bool blends;
        uint2 obj = SpriteAt(screen, blends);
        int objRank = obj.y < 2 ? (int)obj.y : obj.y == 2 ? 4 : 7;
        if (obj.y != NoSprite) XrayInsert(uint2(obj.x, obj.y), objRank, blends ? 16 : 0, winner, rank, source);
    }
    if (!inside && (Level & source) != 0)
    {
        uint sub = 0;
        if (OffsetX != 0 && (Reserved3 == BehindLayer2 || Reserved3 == BehindLayer2AndLayer3))
            sub = XrayBackground(RoomSource, (uint)MatrixA, uint2(MapWidth,MapHeight), bg2Screen + bg2Scroll).x;
        if (OffsetX != 0 && (Reserved3 == BehindLayer3 || Reserved3 == BehindLayer2AndLayer3) && screen.y >= (uint)CenterY)
        {
            uint2 bg3 = XrayBackground(SourceLayer3, (uint)MatrixC, uint2(32,(uint)CenterX),
                screen + LineScrollPair(SlotBlendHorizontal, SlotBlendVertical, screen.y));
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
