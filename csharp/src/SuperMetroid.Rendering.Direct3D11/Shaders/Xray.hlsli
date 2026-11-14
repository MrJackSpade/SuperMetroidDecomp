// A room under a gameplay blend (#1514): its layers and actors in the gameplay scene's order, the window inside which it
// shows unblended (Window.hlsli), the layers each line shows, and outside the window the blend of the frontmost thing
// with what lies behind it or with the blend's colour. D3D11FrameRenderer.DispatchXrayRoom packs the header and lines;
// D3D11BlendBehind names what lies behind.
static const uint BehindOverlay = 1;
static const uint BehindBackground = 2;
static const uint BehindBackgroundAndOverlay = 3;
// The layers a line shows, in the third word of its row's high half (D3D11FrameRenderer.ShownWord).
static const uint ShowsForeground = 1, ShowsBackground = 2, ShowsActors = 4;
// What the blend changes and how, in the Level word (D3D11FrameRenderer.BlendWord). The frontmost thing at a pixel is
// marked with its bit: the foreground, the background, an actor taking part in blends, or the backdrop; an actor that
// never blends is marked with none.
static const uint BlendsForeground = 1, BlendsBackground = 2, BlendsActors = 16, BlendsBackdrop = 32;
static const uint BlendHalves = 64, BlendSubtracts = 128;
// The gameplay scene's layers from back to front (GameplaySceneLayer.Order): the place each holds decides which is
// frontmost at a pixel.
static const int RankActorsBehindRoom = 0, RankBackgroundBackParts = 1, RankForegroundBackParts = 2;
static const int RankActorsBehindFrontParts = 3, RankBackgroundFrontParts = 4, RankForegroundFrontParts = 5;
static const int RankActorsInFront = 6;
// The colour a shown picture's pixel draws, or nothing where no picture or a transparent pixel lies.
uint XraySample(bool shown, uint picture, uint2 within)
{
    uint sample = 0;
    // Every background draws from pictures (#1403); a position without one shows nothing.
    if (shown && picture != 0)
    {
        // A block picture (#1392) keeps no row for its transparent pixels, which show nothing.
        uint index;
        uint colour = PicturePixel(picture, within, index);
        if (index != 0) sample = colour;
    }
    return sample;
}
// The colour part `part` (#1634) of the effect overlay (FxOverlayPicture, #1514) draws at a position; a position it does
// not cover is transparent.
uint XrayOverlayPart(uint2 position, uint part)
{
    uint picture = 0;
    uint2 within = uint2(0, 0);
    bool shown = FxOverlayPicture(position & 65535, part, within, picture);
    return XraySample(shown, picture, within);
}
// The colour the effect overlay draws at a position, with whether it comes from its pictures' part drawn in front
// (#1634): the in-front part's opaque pixel, otherwise the behind part's. The parts are looked up in one loop body: WARP,
// the render checks' reference device, misreads the overlay record's first word when the lookup is emitted twice in a row.
uint2 XrayOverlay(uint2 position)
{
    uint2 shown = uint2(0, 0);
    [loop] for (uint step = 0; step < PictureParts; step++)
    {
        uint part = step == 0 ? PartInFront : PartBehind;
        uint colour = XrayOverlayPart(position, part);
        if ((colour >> 24) != 0)
        {
            shown = uint2(colour, part == PartInFront ? 1 : 0);
            break;
        }
    }
    return shown;
}
// The colour part `part` (#1634) of a room layer (RoomTile), boss body, reveal or library background draws at a position;
// a position the room does not cover is transparent. Pictures written over the background cover it when roomSource carries
// SourceWrittenOverBackground.
uint XrayBackgroundPart(uint roomSource, uint2 position, uint part)
{
    uint entry = 0;
    uint picture = 0;
    uint2 within = uint2(0, 0);
    bool shown = true;
    uint source = roomSource & 255;
    bool overlaid = (roomSource & SourceWrittenOverBackground) != 0 && WrittenOverBackgroundPicture(position & 65535, part, picture, within);
    if (!overlaid)
    {
        if (source == SourceBossBody)
        {
            BossBodyPicture(position, part, within, picture);
            shown = picture != 0;
        }
        else if (source == SourceXrayReveal)
            shown = RevealPicture(position & 65535, part, picture, within);
        else if (source == SourceLibraryBackground)
            shown = LibraryBackgroundPicture(position & 65535, part, picture, within);
        else
            shown = RoomTile(source, position, part, entry, within, picture);
    }
    return XraySample(shown, picture, within);
}
// The colour a room layer, boss body, reveal or library background draws at a position, with whether it comes from the
// pictures drawn in front (#1634): the in-front part's opaque pixel, otherwise the behind part's.
uint2 XrayBackground(uint roomSource, uint2 position)
{
    uint inFront = XrayBackgroundPart(roomSource, position, PartInFront);
    if ((inFront >> 24) != 0) return uint2(inFront, 1);
    return uint2(XrayBackgroundPart(roomSource, position, PartBehind), 0);
}

// Keeps `candidate` as the frontmost thing when it is opaque and lies at or in front of the frontmost so far.
void XrayInsert(uint2 candidate, int candidateRank, uint candidateSource,
    inout uint winner, inout int rank, inout uint source)
{
    if ((candidate.x >> 24) == 0 || candidateRank < rank) return;
    winner = candidate.x; rank = candidateRank; source = candidateSource;
}

// The rank of the actors' layer at depth `depth` (SpriteLayer.hlsli's Depth constants).
int ActorRank(uint depth)
{
    return depth == DepthBehindRoom ? RankActorsBehindRoom
        : depth == DepthBehindFrontParts ? RankActorsBehindFrontParts : RankActorsInFront;
}

uint XrayGameplay(uint2 screen)
{
    uint winner = Backdrop();
    // The HUD's band shows no room; the HUD's own layer draws over it (#1466).
    if (screen.y >= 32)
    {
    uint4 scan = ScanlineParameters[screen.y];
    uint shownLayers = scan.z >> 16;
    uint2 backgroundScreen = screen;
    if (Reserved27 > 1) backgroundScreen -= backgroundScreen % Reserved27;
    // The background's line scroll, from its described line scrolls (#1468).
    uint2 backgroundScroll = LineScrollPair(SlotHorizontal, SlotVertical, screen.y);
    // Inside the window the room shows unblended.
    bool inside = InWindow(screen);
    uint source = BlendsBackdrop;
    int rank = -1;
    // The reveal's blocks, on the background, show only inside the window; the foreground shows only outside it.
    if ((TransparentZero == 0 || inside) && (shownLayers & ShowsBackground) != 0)
    {
        uint2 background = XrayBackground(RoomSource, backgroundScreen + backgroundScroll);
        XrayInsert(background, background.y != 0 ? RankBackgroundFrontParts : RankBackgroundBackParts, BlendsBackground,
            winner, rank, source);
    }
    if ((TransparentZero == 0 || !inside) && (shownLayers & ShowsForeground) != 0)
    {
        uint2 foreground = XrayBackground(SourceForeground, screen + uint2(HorizontalScroll, VerticalScroll));
        XrayInsert(foreground, foreground.y != 0 ? RankForegroundFrontParts : RankForegroundBackParts, BlendsForeground,
            winner, rank, source);
    }
    if ((shownLayers & ShowsActors) != 0)
    {
        bool blends;
        uint2 obj = SpriteAt(screen, blends);
        if (obj.y != NoSprite) XrayInsert(uint2(obj.x, 1), ActorRank(obj.y), blends ? BlendsActors : 0, winner, rank, source);
    }
    if (!inside && (Level & source) != 0)
    {
        // OffsetX marks that layers lie behind what blends, Reserved3 which: the background, the overlay or both, the
        // overlay's in-front pictures over the background and its behind pictures under it (#1634).
        uint behind = 0;
        if (OffsetX != 0 && (Reserved3 == BehindBackground || Reserved3 == BehindBackgroundAndOverlay))
            behind = XrayBackground(RoomSource, backgroundScreen + backgroundScroll).x;
        if (OffsetX != 0 && (Reserved3 == BehindOverlay || Reserved3 == BehindBackgroundAndOverlay) && screen.y >= (uint)CenterY)
        {
            uint2 overlay = XrayOverlay(screen + LineScrollPair(SlotBlendHorizontal, SlotBlendVertical, screen.y));
            if ((overlay.x >> 24) != 0 && (overlay.y != 0 || (behind >> 24) == 0)) behind = overlay.x;
        }
        // Where nothing lies behind, the blend combines with its colour, unhalved where layers lie behind.
        bool combinesBehind = OffsetX != 0 && (behind >> 24) != 0;
        int3 other = combinesBehind ? int3(Unpack(behind) >> 3) : int3(AddR,AddG,AddB);
        int3 value = int3(Unpack(winner) >> 3);
        value = (Level & BlendSubtracts) != 0 ? value - other : value + other;
        if ((Level & BlendHalves) != 0 && (OffsetX == 0 || combinesBehind)) value >>= 1;
        uint3 c = (uint3)clamp(value, 0, 31);
        winner = Pack((c << 3) | (c >> 2), winner >> 24);
    }
    }
    return winner;
}
