// These operation IDs match D3D11TileOperation.cs. Colours holds the scene's backdrop and each picture's colours (#1408).
static const uint OpBackdrop = 0, OpLayer = 1, OpBrightness = 3, OpAddColour = 4;
static const uint OpInsertSprites = 6;
static const uint OpFloor = 7;
static const uint OpLineColourAdd = 8;
static const uint OpAddLayer = 9, OpSubtractLayer = 10;
static const uint OpAddLayerBehind = 11;
static const uint OpMessageText = 12;
static const uint OpXrayRoom = 14;
cbuffer TileParameters : register(b0)
{
    // LayerWindowed marks a layer pass drawn only within its window (#1514), whose shapes WindowShapes holds. DrawnDepth
    // is the depth of what a pass draws (#1514, #1634): zero for everything; for a sprite insertion, one more than the depth
    // whose sprites it inserts; for a layer pass, which of its pictures' parts it draws (DrawsBehindPart or
    // DrawsInFrontPart).
    uint Operation, TilemapWord, LayerWindowed, HorizontalScroll;
    uint VerticalScroll, Reserved5, Reserved6, DrawnDepth;
    uint TransparentZero, Level, AddR, AddG;
    uint AddB, Reserved2, RoomSource, Reserved3;
    int MatrixA, MatrixB, MatrixC, MatrixD;
    int CenterX, CenterY, OffsetX, OffsetY;
    uint FloorOutside, FirstScanline, EndScanline;
    uint Reserved27;
    uint Reserved28, Reserved29, Reserved30, Reserved31;
    uint4 ScanlineParameters[224];
    // The window an operation tests pixels against (Window.hlsli, #1468): a header row and two rows per shape.
    uint4 WindowShapes[9];
    // The pass's described line scrolls (#1468): four slots and their data rows; LineScroll reads them.
    uint4 LineScrolls[24];
};
// The scene's colours (D3D11SceneColourBuffer): the backdrop, then each colour slot's table's word, then the tables:
// the colour each of the slot's image's colour keys shows under the slot's shading (#1512, #1633).
StructuredBuffer<uint> Colours : register(t0);
// Room level planes (D3D11RoomLevelBuffer): the foreground, background and door-source plane records, the door window, the foreground overlay,
// and the pre-coloured block pictures the planes' definitions show (#1392).
StructuredBuffer<uint> RoomLevels : register(t3);
// RoomSource: what a layer pass draws: the room's foreground or background as level data shows it, the sky's rows, the effect
// overlay, a boss body, the X-ray reveal, a library background, the HUD, a pictured plane or a picture grid.
// SourceWrittenOverBackground added to it lays words written over the background on top.
static const uint SourceForeground = 1, SourceBackground = 2, SourceSky = 3, SourceFxOverlay = 4;
static const uint SourceBossBody = 6, SourceXrayReveal = 7, SourceLibraryBackground = 8;
// The HUD's placed pictures (PicturedHud, #1466), at the HUD's place at the top of the screen.
static const uint SourceHud = 10;
// A pictured plane (PicturedPlane, #1467, #1555), its table's word in TilemapWord: the count of its pictures, then two
// words each in drawing order, the picture's offset and its upper-left plane pixel, x below y, each a signed half. The
// plane does not repeat.
static const uint SourcePicturedPlane = 11;
// A frontend picture grid (PictureGridLayer, #1467), its table at room level word `TilemapWord`: its width and height in
// cells, x below y, then a word per cell row by row, its one-cell picture's offset (zero for none) with its horizontal and
// vertical mirroring in bits 30 and 31. The grid does not repeat (#1555).
static const uint SourcePictureGrid = 12;
static const uint GridFlipX = 1073741824, GridFlipY = 2147483648, GridPictureMask = 1073741823;
static const uint SourceWrittenOverBackground = 256;
static const uint RoomRecordWords = 6, RoomDoorSlot = 2, RoomDoorWindowWord = 18, RoomOverlayWord = 19;
static const uint RoomFxOverlayWord = 20, RoomWrittenOverBackgroundWord = 21, RoomFloorSheetsWord = 22, RoomBossBodyWord = 23, RoomXrayRevealWord = 24;
// A picture of a room's tiles is installed as two parts (LayeredPicture, #1634): the part drawn behind the actors that
// pass in front of the room's back tiles and the part drawn in front of them. Wherever a table names such a picture it
// names both, PictureParts words, the behind part's offset first; either is zero where that part shows nothing. A source
// is asked for one part's picture at a time.
static const uint PictureParts = 2, PartBehind = 0, PartInFront = 1;
// What a layer pass's DrawnDepth chooses: both parts, the in-front part over the behind part, or one part alone.
static const uint DrawsBothParts = 0, DrawsBehindPart = 1, DrawsInFrontPart = 2;
// A library background (RoomBackgroundImagePlane, #1555): its image picture's parts, the image's width and height and the
// area its rectangles lie on, each x below y, the count of its rectangles, three words for each (its corner on the area,
// the image pixel that corner shows and its size, each x below y), then each 16x16 cell's picture's parts, row by row
// across the image, the behind part's word marked by CellShowsOwnPicture where the cell shows its own picture.
static const uint RoomLibraryBackgroundWord = 25;
static const uint ImageSizeWord = 2, ImageAreaWord = 3, ImagePieceCountWord = 4, ImageHeaderWords = 5;
static const uint ImagePieceWords = 3;
static const uint CellShowsOwnPicture = 2147483648, CellPictureMask = 2147483647;
// The scrolling sky's image, laid out as a library background's.
static const uint RoomSkyImageWord = 26;
// The image of the message box the frame shows (#1392).
static const uint RoomMessagePictureWord = 27;
// The HUD's placed pictures (#1466): their count, then two words each in drawing order, the picture's offset and its
// upper-left screen pixel, x below y.
static const uint RoomHudWord = 28;
// The X-ray reveal (XrayRevealPlane, #1514): the foreground pixel its first block lies on, x below y, then 17 block columns
// by 16 rows, each its picture's parts, the first with the block's arrangement in the top four bits: as defined, mirrored
// horizontally, vertically or both, or its tile rows swapped.
static const uint RevealColumns = 17, RevealRows = 16;
// A picture (a block, or a background image): its colour slot, its place in the image texture and its width and height,
// x below y.
static const uint PictureColoursWord = 0, PicturePlaceWord = 1, PictureSizeWord = 2;
// The installed images (D3D11ImageAtlas), each texel the image's pixel, then its key word (D3D11ImageAtlas.KeyWord): the
// place of the pixel's colour key among its image's in the low sixteen bits (#1633), and above it whether it is clear.
Texture2D<uint2> Images : register(t4);
static const uint KeyPlaceMask = 65535, KeyClear = 65536;
// The perspective floor's image (D3D11FloorImage): the 1024-pixel floor with its states' pictures laid over it. Each
// texel's key word keeps the index of the floor picture it comes from.
Texture2D<uint2> Floor : register(t5);
static const uint FloorSheetShift = 20;
// How a floor pass blends, in Reserved27 (D3D11FrameRenderer.FloorBlendWord): not at all, subtracting the frontmost
// sprite, or adding the floor's own colour over itself and the blending sprites.
static const uint FloorBlendsNot = 0, FloorSubtractsSprites = 1, FloorAddsOwnColour = 2;
RWTexture2D<uint> Output : register(u0);
RWTexture2D<uint2> SpriteLayer : register(u1);

// What a room plane shows at a scrolled position; false outside the room: part `part` of its block's picture (#1392,
// #1634), mirrored by the block's flips, with `within` the picture pixel. Each packed block is its definition with its
// horizontal and vertical mirroring in bits sixteen and seventeen (#1403, #1538).
bool RoomPlaneTile(uint slot, uint2 scrolled, uint part, out uint entry, out uint2 within, out uint picture)
{
    uint record = slot * RoomRecordWords;
    uint width = RoomLevels[record + 1], height = RoomLevels[record + 2];
    uint2 level = (scrolled - uint2(RoomLevels[record + 3], RoomLevels[record + 4])) & 65535;
    entry = 0;
    picture = 0;
    within = level & 7;
    bool inside = level.x < width * 16 && level.y < height * 16;
    if (inside)
    {
        uint word = RoomLevels[RoomLevels[record] + (level.y >> 4) * width + (level.x >> 4)];
        uint h = (word >> 16) & 1, v = (word >> 17) & 1;
        picture = RoomLevels[RoomLevels[record + 5] + (word & 65535) * PictureParts + part];
        within = uint2(h != 0 ? 15 - (level.x & 15) : level.x & 15, v != 0 ? 15 - (level.y & 15) : level.y & 15);
    }
    return inside;
}
// Part `part` of what a background image's record at room level word `image` shows at image pixel `pixel` (#1392, #1634):
// the picture of the 16x16 cell lying there where the cell shows its own, otherwise the image's, as `picture`, with
// `within` its pixel. The image's width, in its record, counts its cells across.
void BackgroundImagePicture(uint image, uint2 pixel, uint part, out uint2 within, out uint picture)
{
    uint columns = (RoomLevels[image + ImageSizeWord] & 65535) >> 4;
    uint cells = image + ImageHeaderWords + RoomLevels[image + ImagePieceCountWord] * ImagePieceWords;
    uint cell = cells + ((pixel.y >> 4) * columns + (pixel.x >> 4)) * PictureParts;
    bool own = (RoomLevels[cell] & CellShowsOwnPicture) != 0;
    picture = own ? RoomLevels[cell + part] & CellPictureMask : RoomLevels[image + part];
    within = own ? (pixel & 15) : pixel;
}
// What the scrolling sky (SkyPictures, #1403, #1514) shows: part `part` of its image or of a rebound cell's picture as
// `picture`, with `within` its pixel, repeating every 2048 pixels down and 256 across; every band of it shows one of its
// pages (#1611). Its image's record is a library background's.
void SkyPicture(uint2 scrolled, uint part, out uint2 within, out uint picture)
{
    uint2 position = uint2(scrolled.x & 255, scrolled.y & 2047);
    // The sky's one rectangle is its whole image at the area's corner, so the image pixel is the position.
    BackgroundImagePicture(RoomLevels[RoomSkyImageWord], position, part, within, picture);
}
// What the effect overlay shows at a scrolled position (OverlayPicture, #1514, #1706), from its record at room level word
// RoomFxOverlayWord: its kind, then an atmosphere's picture or a liquid's surface strip, then a liquid's body, each as its
// parts (#1634) and its width and height, x below y. Every picture repeats across at its width. An atmosphere's picture
// repeats down at its height. A liquid's scrolled row counts down from its strip's top line, a signed half: the strip
// shows its rows, nothing shows above it and the body repeats down below it; a liquid without its strip shows the body
// on every row, at the place in its repeat it has below the strip. Gives part `part` of the picture shown as `picture`,
// with `within` its pixel there. It computes every case and leaves by one exit: written with early returns inside its
// liquid branches, the WARP device drew some of an atmosphere's pixels from the wrong place, differently from one device
// to the next (#1706).
static const uint FxOverlayAtmosphere = 0, FxOverlayLiquidWithSurface = 1, FxOverlayLiquidBodyOnly = 2;
static const uint FxOverlayPictureWords = PictureParts + 1;
bool FxOverlayPicture(uint2 scrolled, uint part, out uint2 within, out uint picture)
{
    uint table = RoomLevels[RoomFxOverlayWord];
    uint kind = RoomLevels[table];
    uint strip = table + 1, body = strip + FxOverlayPictureWords;
    uint stripSize = RoomLevels[strip + PictureParts], bodySize = RoomLevels[body + PictureParts];
    uint2 stripExtent = max(uint2(stripSize & 65535, stripSize >> 16), uint2(1, 1));
    uint2 bodyExtent = max(uint2(bodySize & 65535, bodySize >> 16), uint2(1, 1));
    // A liquid's scrolled row counts down from its strip's top line as a signed half; lifted by half the halves' range it
    // is never negative, so the arithmetic stays unsigned.
    uint lifted = (scrolled.y + 32768) & 65535;
    bool liquid = kind != FxOverlayAtmosphere;
    bool showsStrip = kind == FxOverlayLiquidWithSurface;
    bool aboveStrip = lifted < 32768;
    bool inStrip = showsStrip && !aboveStrip && lifted - 32768 < stripExtent.y;
    bool inBody = liquid && !inStrip && !(showsStrip && aboveStrip);
    // The body's row: the rows below the strip's last, counted from a whole number of bodies above the lift.
    uint shift = 32768 + stripExtent.y;
    uint whole = (shift + bodyExtent.y - 1) / bodyExtent.y * bodyExtent.y;
    uint2 atStrip = uint2(scrolled.x % stripExtent.x, inStrip ? lifted - 32768 : scrolled.y % stripExtent.y);
    uint2 atBody = uint2(scrolled.x % bodyExtent.x, (lifted + whole - shift) % bodyExtent.y);
    bool shown = table != 0 && (!liquid || inStrip || inBody);
    within = shown ? (inBody ? atBody : atStrip) : uint2(0, 0);
    picture = shown ? RoomLevels[(inBody ? body : strip) + part] : 0;
    return shown;
}
// The HUD's picture at a screen position (#1466): the last placed picture covering it, as `picture`, with `within` its
// pixel there; false where no picture covers it, which shows nothing.
bool HudPicture(uint2 screen, out uint2 within, out uint picture)
{
    picture = 0;
    within = uint2(0, 0);
    uint table = RoomLevels[RoomHudWord];
    if (table == 0) return false;
    for (uint placed = RoomLevels[table]; placed > 0; placed--)
    {
        uint record = table + 1 + (placed - 1) * 2;
        uint shown = RoomLevels[record];
        uint corner = RoomLevels[record + 1];
        uint2 origin = uint2(corner & 65535, corner >> 16);
        uint extent = RoomLevels[shown + PictureSizeWord];
        if (screen.x < origin.x || screen.y < origin.y) continue;
        uint2 offset = screen - origin;
        if (offset.x >= (extent & 65535) || offset.y >= (extent >> 16)) continue;
        picture = shown;
        within = offset;
        return true;
    }
    return false;
}
// Two signed sixteen-bit halves of a word, the low half as x (#1555).
int2 SignedHalves(uint word)
{
    return int2(int(word << 16) >> 16, int(word) >> 16);
}
// The plane pixel a screen pixel shows under a pictured plane's or picture grid's scroll offset (#1555), whose two signed
// halves arrive in the low halves of `scroll`'s words: the plane does not repeat.
int2 PlanePosition(uint2 screen, uint2 scroll)
{
    return int2(screen) + SignedHalves((scroll.x & 65535) | (scroll.y << 16));
}
// A pictured plane's picture at a plane position (#1467, #1555): the last placed picture covering it, as `picture`, with
// `within` its pixel there; false where none does. The plane does not repeat.
bool PlanePicture(uint table, int2 position, out uint2 within, out uint picture)
{
    picture = 0;
    within = uint2(0, 0);
    for (uint placed = RoomLevels[table]; placed > 0; placed--)
    {
        uint record = table + 1 + (placed - 1) * 2;
        uint shown = RoomLevels[record];
        int2 offset = position - SignedHalves(RoomLevels[record + 1]);
        uint extent = RoomLevels[shown + PictureSizeWord];
        if (offset.x < 0 || offset.y < 0 || offset.x >= int(extent & 65535) || offset.y >= int(extent >> 16)) continue;
        picture = shown;
        within = uint2(offset);
        return true;
    }
    return false;
}
// The cell picture of a picture grid (#1467, #1555) at a plane position, as `picture`, with `within` its pixel there,
// mirrored as the cell mirrors it; false off the grid's cells, which do not repeat, and where the cell shows nothing.
bool GridPicture(uint table, int2 scrolled, out uint2 within, out uint picture)
{
    picture = 0;
    within = uint2(0, 0);
    uint size = RoomLevels[table];
    int2 cells = int2(size & 65535, size >> 16);
    if (scrolled.x < 0 || scrolled.y < 0 || scrolled.x >= cells.x * 8 || scrolled.y >= cells.y * 8) return false;
    uint2 position = uint2(scrolled);
    uint2 cell = position >> 3;
    uint entry = RoomLevels[table + 1 + cell.y * uint(cells.x) + cell.x];
    picture = entry & GridPictureMask;
    within = uint2((entry & GridFlipX) != 0 ? 7 - (position.x & 7) : position.x & 7,
        (entry & GridFlipY) != 0 ? 7 - (position.y & 7) : position.y & 7);
    return picture != 0;
}
// A picture written over a room's layer (RoomOverlayPictures, #1403, #1514) at a scrolled position of that layer, from the
// table at room level word `table`: its count, then WrittenPictureWords words each in the order written, the picture's
// parts (#1634), the layer pixel its corner lies on and its width and height, each x below y. The last written picture
// covering the position gives part `part` of it as `picture`, with `within` its pixel there, and covers the layer there
// whether or not that part shows anything; false where none does, which leaves the layer showing.
static const uint WrittenPictureWords = PictureParts + 2;
bool WrittenPicture(uint table, uint2 scrolled, uint part, out uint2 within, out uint picture)
{
    picture = 0;
    within = uint2(0, 0);
    if (table == 0) return false;
    for (uint placed = RoomLevels[table]; placed > 0; placed--)
    {
        uint record = table + 1 + (placed - 1) * WrittenPictureWords;
        uint corner = RoomLevels[record + PictureParts];
        uint2 offset = (scrolled - uint2(corner & 65535, corner >> 16)) & 65535;
        uint extent = RoomLevels[record + PictureParts + 1];
        if (offset.x >= (extent & 65535) || offset.y >= (extent >> 16)) continue;
        picture = RoomLevels[record + part];
        within = offset;
        return true;
    }
    return false;
}
// Part `part` of the picture written over the background at a scrolled position (#1348, #1514, #1634), as WrittenPicture
// finds it.
bool WrittenOverBackgroundPicture(uint2 scrolled, uint part, out uint picture, out uint2 within)
{
    return WrittenPicture(RoomLevels[RoomWrittenOverBackgroundWord], scrolled, part, within, picture);
}
// Part `part` of the X-ray reveal's block picture and its pixel at a scrolled position of the foreground (#1392, #1514,
// #1634), the background moving with the foreground while it shows the reveal; false outside the blocks it reveals and
// where the block's picture has no such part.
bool RevealPicture(uint2 scrolled, uint part, out uint picture, out uint2 within)
{
    uint table = RoomLevels[RoomXrayRevealWord];
    uint origin = RoomLevels[table];
    uint2 position = (scrolled - uint2(origin & 65535, origin >> 16)) & 65535;
    uint2 block = position >> 4;
    picture = 0;
    within = position & 15;
    if (block.x >= RevealColumns || block.y >= RevealRows) return false;
    uint record = table + 1 + (block.y * RevealColumns + block.x) * PictureParts;
    uint arrangement = RoomLevels[record] >> 28;
    picture = RoomLevels[record + part] & 0x0FFFFFFF;
    if (arrangement == 1 || arrangement == 3) within.x = 15 - within.x;
    if (arrangement == 2 || arrangement == 3) within.y = 15 - within.y;
    if (arrangement == 4) within.y = (within.y + 8) & 15;
    // A block copied from beyond the room has no picture and shows nothing.
    return picture != 0;
}
// Part `part` of the picture a room's library background shows at a position of the background, and its pixel (#1392,
// #1555, #1634): the image's area (#1514), x below y, repeats beyond its edges, and the rectangle lying at the position
// shows the image pixel that far from its corner's; a rebound cell's picture, or the image. False where no rectangle lies.
bool LibraryBackgroundPicture(uint2 scrolled, uint part, out uint picture, out uint2 within)
{
    uint image = RoomLevels[RoomLibraryBackgroundWord];
    uint area = RoomLevels[image + ImageAreaWord];
    uint2 extent = uint2(area & 65535, area >> 16);
    uint2 position = (scrolled & 65535) % extent;
    uint pieces = RoomLevels[image + ImagePieceCountWord];
    picture = 0;
    within = uint2(0, 0);
    for (uint piece = 0; piece < pieces; piece++)
    {
        uint record = image + ImageHeaderWords + piece * ImagePieceWords;
        uint corner = RoomLevels[record], source = RoomLevels[record + 1], size = RoomLevels[record + 2];
        uint2 offset = position - uint2(corner & 65535, corner >> 16);
        if (offset.x >= (size & 65535) || offset.y >= (size >> 16)) continue;
        BackgroundImagePicture(image, uint2(source & 65535, source >> 16) + offset, part, within, picture);
        return true;
    }
    return false;
}
// Part `part` of the background from the room's background plane or the sky's rows, or of the foreground as
// RoomBackgrounds resolves it: a door's source screen, then words written over the foreground, then the level plane. A
// written picture covers the plane where it lies, in both parts (#1634).
bool RoomTile(uint source, uint2 scrolled, uint part, out uint entry, out uint2 within, out uint picture)
{
    entry = 0;
    picture = 0;
    within = uint2(0, 0);
    scrolled &= 65535;
    uint slot = source == SourceBackground ? 1 : 0;
    bool overlaid = false;
    if (source != SourceBackground && source != SourceSky)
    {
        uint door = RoomLevels[RoomDoorWindowWord];
        uint2 doorWithin = (scrolled - uint2(door & 65535, door >> 16)) & 65535;
        if (RoomLevels[RoomDoorSlot * RoomRecordWords + 1] != 0 && doorWithin.x < 256 && doorWithin.y < 192)
            slot = RoomDoorSlot;
        else
            // A picture written over the foreground draws where it lies (#1403, #1514); none where nothing was written.
            overlaid = WrittenPicture(RoomLevels[RoomOverlayWord], scrolled, part, within, picture);
    }
    bool shown = overlaid;
    if (source == SourceSky)
    {
        SkyPicture(scrolled, part, within, picture);
        shown = true;
    }
    else if (!overlaid)
        shown = RoomPlaneTile(slot, scrolled, part, entry, within, picture);
    return shown;
}
uint Pack(uint3 c, uint alpha) { return c.r | (c.g << 8) | (c.b << 16) | (alpha << 24); }
uint3 Unpack(uint c) { return uint3(c & 255, (c >> 8) & 255, (c >> 16) & 255); }
// The colour shown where nothing is drawn.
uint Backdrop() { return Colours[0]; }
// The colour an image texel shows when its image is coloured from colour slot `slot` (#1408, #1512, #1633): whether it is
// opaque rather than clear (`index`), and its colour. A visible pixel shows the colour its colour key shows under the
// slot's shading. A clear pixel has no key and is never recoloured: a see-through layer leaves it out, and a layer drawn
// opaque shows its stored colour as it is, the colour it keeps of its own or black (#1544). A pixel is only a colour:
// which pictures lie in front is the pass's choice of pictures (#1634).
uint TexelColour(uint2 texel, uint slot, out uint index)
{
    index = 0;
    uint colour = texel.x | 0xff000000;
    if ((texel.y & KeyClear) == 0)
    {
        index = 1;
        colour = Colours[Colours[1 + slot] + (texel.y & KeyPlaceMask)];
    }
    return colour;
}
// A picture's pixel (#1392, #1408), as TexelColour colours it from the picture's colour slot.
uint PicturePixel(uint picture, uint2 pixel, out uint index)
{
    uint place = RoomLevels[picture + PicturePlaceWord];
    uint2 texel = Images.Load(int3(uint2(place & 65535, place >> 16) + pixel, 0));
    return TexelColour(texel, RoomLevels[picture + PictureColoursWord], index);
}
// A boss body part's depth (D3D11RoomLevelBuffer.DepthWord, #1634): drawn behind the sprites, or in front of them.
static const uint BossBodyBehindSprites = 0, BossBodyInFrontOfSprites = 1;
static const uint BossBodyPartWords = 5;
// A boss body (ShownBossBody, #1466, #1514, #1540, #1609) at a scrolled position of its layer: its parts are placed on the
// layer with nothing repeating beyond them, and layered in their table's order, each drawn over the parts before it. The
// frontmost part whose picture is opaque at the position shows there, its pixel `within`; where none is, the frontmost part
// reaching it gives its clear pixel, and a position no part reaches shows nothing. Each boss part is drawn whole at its
// depth (#1634): its picture is the layer's behind part (`part` PartBehind) for a boss part drawn behind the sprites and
// its in-front part (PartInFront) for one drawn in front of them, so `picture` is the shown boss part's picture when its
// depth is the one `part` asks for, and zero otherwise.
// Each boss part is five words: its picture's offset, the body pixel the picture's top-left corner lies on, how much of
// the picture shows across and down from that corner, each x below y, its depth, and the offset of the pixels it hides or
// zero (#1652): its cells across below its cells down, then each 8x8 cell's hidden pixel columns, a byte a cell, four
// cells a word. A hidden pixel shows nothing of the part, as a clear one does. Every background sampler reads the boss
// through this, so a blending pass shows the same body as an ordinary one (#1426).
bool BossBodyHides(uint hidden, uint2 offset)
{
    if (hidden == 0) return false;
    uint columns = RoomLevels[hidden] & 65535;
    uint cell = (offset.y >> 3) * columns + (offset.x >> 3);
    uint bits = (RoomLevels[hidden + 1 + (cell >> 2)] >> ((cell & 3) * 8)) & 255;
    return ((bits >> (offset.x & 7)) & 1) != 0;
}
void BossBodyPicture(uint2 scrolled, uint part, out uint2 within, out uint picture)
{
    uint table = RoomLevels[RoomBossBodyWord];
    uint2 position = scrolled & 65535;
    uint shownPicture = 0, shownDepth = BossBodyBehindSprites;
    within = uint2(0, 0);
    for (uint placed = RoomLevels[table]; placed > 0; placed--)
    {
        uint record = table + 1 + (placed - 1) * BossBodyPartWords;
        uint corner = RoomLevels[record + 1], size = RoomLevels[record + 2];
        uint2 offset = position - uint2(corner & 65535, corner >> 16);
        if (offset.x >= (size & 65535) || offset.y >= (size >> 16)) continue;
        uint shown = RoomLevels[record];
        uint index;
        PicturePixel(shown, offset, index);
        if (BossBodyHides(RoomLevels[record + 4], offset)) index = 0;
        if (shownPicture == 0 || index != 0)
        {
            shownPicture = shown;
            shownDepth = RoomLevels[record + 3];
            within = offset;
        }
        if (index != 0) break;
    }
    uint askedDepth = part == PartInFront ? BossBodyInFrontOfSprites : BossBodyBehindSprites;
    picture = shownDepth == askedDepth ? shownPicture : 0;
}

/// A pass's described line scrolls (#1468, D3D11LineScrolls): LineScrolls' first four rows are slots, each one axis of
// one plane (a pass's own plane horizontally then vertically, then the plane an X-ray pass blends with); each slot's
// data rows follow them. A slot row holds its kind in the low byte of x with its base scroll in the high half, its band
// (first line, then the line after its last in the high half) in y, its scale in z and its shape word in w.
// These kinds match D3D11LineScrollKind.cs.
static const uint ScrollBandsKind = 0, ScrollWaveKind = 1, ScrollStretchKind = 2, ScrollSampledKind = 3;
static const uint LineScrollSlots = 4, LineScrollDataRows = 5;
static const uint SlotHorizontal = 0, SlotVertical = 1, SlotBlendHorizontal = 2, SlotBlendVertical = 3;
// The game's signed sine samples over a quarter turn of its 256-step angle (Trigonometry.SignedSine): 256 times the
// sine, rounded down, with the quarter turn itself at 256. A wave reads these, not the device's sine, so every device
// draws the same lines.
static const int QuarterSine[65] =
{
    0, 6, 12, 18, 25, 31, 37, 43, 49, 56, 62, 68, 74, 80, 86, 92, 97, 103, 109, 115, 120, 126, 131, 136, 142, 147, 152,
    157, 162, 167, 171, 176, 181, 185, 189, 193, 197, 201, 205, 209, 212, 216, 219, 222, 225, 228, 231, 234, 236, 238,
    241, 243, 244, 246, 248, 249, 251, 252, 253, 254, 254, 255, 255, 255, 256
};
// Word `index` of slot `slot`'s data rows.
uint LineScrollData(uint slot, uint index)
{
    return LineScrolls[LineScrollSlots + slot * LineScrollDataRows + index / 4][index & 3];
}
// The scroll slot `slot` gives screen line `screenY`, a sixteen-bit scroll.
// Bands (ScrollBands): the line takes the scroll of the last band beginning on or above it, or the base above the
// first; each band's data word holds its scroll, then its first line in the high half, and w counts them.
// A wave (ScrollWave): each line of its band moves by its amplitude (1/256ths of a pixel) times the sine at the line's
// angle, the angle at line zero (w's low byte) advanced by the line's share of the period's (w's high half) whole turn,
// rounded toward zero.
// A stretch (ScrollStretch): each line of its band falls behind by its slope (1/65536ths of a row) for each line below
// the band's first, and the lines above its band move by w.
// A sampled wave (SampledScrollWave): each line of its band moves by its sample times its amplitude (1/256ths of a
// pixel), rounded down; line y reads sample (phase + y) mod period, the phase in w's low half and the period in its
// high half, the samples signed bytes four to a data word from the low byte up.
uint LineScroll(uint slot, uint screenY)
{
    uint4 shape = LineScrolls[slot];
    uint kind = shape.x & 255, scroll = shape.x >> 16;
    uint bandFirst = shape.y & 65535, bandEnd = shape.y >> 16;
    bool inBand = screenY >= bandFirst && screenY < bandEnd;
    int moved = 0;
    if (kind == ScrollBandsKind)
    {
        [loop] for (uint band = 0; band < shape.w; band++)
        {
            uint word = LineScrollData(slot, band);
            if ((word >> 16) > screenY) break;
            scroll = word & 65535;
        }
    }
    else if (kind == ScrollWaveKind)
    {
        if (inBand)
        {
            uint period = shape.w >> 16;
            uint angle = ((shape.w & 255) + screenY * 256 / period) & 255;
            uint quarter = angle & 127;
            if (quarter > 64) quarter = 128 - quarter;
            int magnitude = (int)(((uint)QuarterSine[quarter] * shape.z) >> 16);
            moved = angle >= 128 ? -magnitude : magnitude;
        }
    }
    else if (kind == ScrollStretchKind)
    {
        if (screenY < bandFirst) moved = (int)shape.w;
        else if (inBand) moved = -(int)(((screenY - bandFirst) * shape.z) >> 16);
    }
    else if (kind == ScrollSampledKind)
    {
        if (inBand)
        {
            uint index = ((shape.w & 65535) + screenY) % (shape.w >> 16);
            int sample = ((int)(LineScrollData(slot, index / 4) << (24 - (index & 3) * 8))) >> 24;
            moved = (sample * (int)shape.z) >> 8;
        }
    }
    return (uint)((int)scroll + moved) & 65535;
}
// A plane's scroll on screen line `screenY` from its two slots, horizontal then vertical.
uint2 LineScrollPair(uint horizontal, uint vertical, uint screenY)
{
    return uint2(LineScroll(horizontal, screenY), LineScroll(vertical, screenY));
}

#include "SpriteLayer.hlsli"
#include "Window.hlsli"
#include "Xray.hlsli"

// Whether a glowing plane (AddedPicturedPlaneLayer, #1467, #1514) covers the screen pixel: everywhere when it names
// nothing it glows over. Where one of the sprites it glows over shows (AddG marks them; the sprite layer holds them), it
// covers the pixel if the sprite takes part in blends and leaves it otherwise; elsewhere it covers the pixels where the
// plane it glows over is opaque. That plane's table is in MatrixA, its signed scroll offset in MatrixC and MatrixD, and
// OffsetY marks that it is present.
bool GlowCovers(uint2 screen)
{
    bool overPlane = OffsetY != 0, overSprites = AddG != 0;
    if (!overPlane && !overSprites) return true;
    if (overSprites)
    {
        bool blends;
        uint2 sprite = SpriteAt(screen, blends);
        if (sprite.y != NoSprite) return blends;
    }
    if (!overPlane) return false;
    uint4 map = uint4(MatrixA, MatrixB, MatrixC, MatrixD);
    uint2 coverWithin;
    uint coverPicture;
    uint coverIndex = 0;
    bool coverShown = PlanePicture(map.x, int2(screen) + int2(map.zw), coverWithin, coverPicture);
    return coverShown && PicturePixel(coverPicture, coverWithin, coverIndex) != 0 && coverIndex != 0;
}

// Part `part` of the picture a layer pass's source (RoomSource) shows at a pixel, as `picture`, with `within` its pixel
// there (#1634): false where the source shows no picture of that part. `sampleScreen` is the screen pixel the pass samples
// (the mosaic's), `scroll` its scroll and `screen` the pixel drawn. A word written over the background shows its picture
// over whatever the background shows there (#1403).
bool SourcePicture(uint part, uint2 sampleScreen, uint2 scroll, uint2 screen, out uint2 within, out uint picture)
{
    uint entry = 0;
    picture = 0;
    within = uint2(0, 0);
    uint2 scrolled = (sampleScreen + scroll) & 65535;
    uint source = RoomSource & 255;
    bool shown = false;
    if ((RoomSource & SourceWrittenOverBackground) != 0 && WrittenOverBackgroundPicture(scrolled, part, picture, within))
        shown = true;
    else if (source == SourceFxOverlay)
        shown = FxOverlayPicture(scrolled, part, within, picture);
    else if (source == SourceBossBody)
    {
        BossBodyPicture(scrolled, part, within, picture);
        shown = true;
    }
    else if (source == SourceXrayReveal)
        shown = RevealPicture(scrolled, part, picture, within);
    else if (source == SourceLibraryBackground)
        shown = LibraryBackgroundPicture(scrolled, part, picture, within);
    // The HUD's, a pictured plane's and a picture grid's pictures are whole, drawn in one pass (#1634): they have no
    // in-front part, and show their one picture as the part behind.
    else if (source == SourceHud)
        shown = part == PartBehind && HudPicture(screen, within, picture);
    else if (source == SourcePicturedPlane)
        shown = part == PartBehind && PlanePicture(TilemapWord, PlanePosition(sampleScreen, scroll), within, picture);
    else if (source == SourcePictureGrid)
        shown = part == PartBehind && GridPicture(TilemapWord, PlanePosition(sampleScreen, scroll), within, picture);
    else
        shown = RoomTile(source, scrolled, part, entry, within, picture);
    // Every background draws from pictures (#1403); a position without one shows nothing.
    return shown && picture != 0;
}

// The colour a layer pass draws at a pixel and whether it is opaque rather than clear (`index`), from the pictures of the
// parts it draws (DrawnDepth, #1634); false where none of them has a picture there. A pass drawing both parts shows the
// in-front part's opaque pixel over the behind part's pixel; where neither part is opaque the two show alike, as a part
// keeps the picture's clear pixels (RoomTileCanvas.SplitSheet), and the behind part's shows.
bool LayerPixel(uint2 sampleScreen, uint2 scroll, uint2 screen, out uint colour, out uint index)
{
    colour = 0;
    index = 0;
    uint2 within = uint2(0, 0);
    uint picture = 0;
    bool found = false;
    bool inFront = DrawnDepth != DrawsBehindPart && SourcePicture(PartInFront, sampleScreen, scroll, screen, within, picture);
    if (inFront)
    {
        colour = PicturePixel(picture, within, index);
        found = true;
    }
    // The behind part shows unless only the in-front part is drawn or the in-front part's pixel is opaque; where only the
    // in-front part lies here and its pixel shows nothing, that clear pixel stands.
    if (DrawnDepth != DrawsInFrontPart && !(inFront && index != 0))
    {
        uint2 behindWithin = uint2(0, 0);
        uint behindPicture = 0;
        if (SourcePicture(PartBehind, sampleScreen, scroll, screen, behindWithin, behindPicture))
        {
            colour = PicturePixel(behindPicture, behindWithin, index);
            found = true;
        }
    }
    return found;
}

[numthreads(8, 8, 1)]
void Main(uint3 id : SV_DispatchThreadID)
{
    if (id.x >= 256 || id.y >= 224) return;
    if (id.y < FirstScanline || id.y >= EndScanline) return;
    if (Operation == OpLayer && LayerWindowed != 0 && !InWindow(id.xy)) return;
    if (Operation == OpXrayRoom) { Output[id.xy] = XrayGameplay(id.xy); return; }
    if (Operation == OpMessageText)
    {
        // The message's picture (#1392, #1555) with its top-left corner at screen pixel (HorizontalScroll, TilemapWord), in
        // the message's text colours (#1408): a clear pixel shows nothing, and nothing shows outside the picture.
        uint picture = RoomLevels[RoomMessagePictureWord];
        if (picture == 0 || id.x < HorizontalScroll || id.y < TilemapWord) return;
        uint size = RoomLevels[picture + PictureSizeWord];
        uint2 within = id.xy - uint2(HorizontalScroll, TilemapWord);
        if (within.x >= (size & 65535) || within.y >= (size >> 16)) return;
        uint index;
        uint shown = PicturePixel(picture, within, index);
        if (index != 0) Output[id.xy] = shown;
        return;
    }
    if (Operation == OpLineColourAdd)
    {
        // A colour added through a window (ColourAddWindowLayer, #1468): the colour's five-bit channels in AddR, AddG
        // and AddB. When MatrixC holds a count of steps, the colour strengthens down the screen: none above line
        // MatrixA, then one step more every MatrixB lines up to MatrixC, with MatrixD steps added to every line.
        if (InWindow(id.xy))
        {
            uint3 colour = uint3(AddR, AddG, AddB);
            if (MatrixC > 0)
            {
                uint firstLine = (uint)MatrixA, linesPerStep = (uint)MatrixB, steps = (uint)MatrixC;
                uint strength = id.y < firstLine ? 0u : min(steps, 1u + (id.y - firstLine) / linesPerStep);
                uint scale = (uint)max(0, (int)strength + MatrixD);
                colour = min(31u, (colour * scale + steps / 2u) / steps);
            }
            uint pixel = Output[id.xy];
            Output[id.xy] = Pack(min(255, Unpack(pixel) + ((colour << 3) | (colour >> 2))), pixel >> 24);
        }
        return;
    }
    if (Operation == OpFloor)
    {
        // The floor (FloorView, #1514): the pixel's distance from the floor's centre, once the floor is scrolled, turned
        // and scaled by the matrix the header's matrix words hold as floats, lands that far from the centre on the floor.
        // `precise` keeps each product and sum separately rounded, so every device lands each pixel on the same texel.
        precise float2 centre = float2(CenterX, CenterY);
        precise float2 fromCentre = float2((int)id.x + OffsetX, (int)id.y + OffsetY) - centre;
        precise float2 onFloor = centre + float2(
            asfloat(MatrixA) * fromCentre.x + asfloat(MatrixB) * fromCentre.y,
            asfloat(MatrixC) * fromCentre.x + asfloat(MatrixD) * fromCentre.y);
        int x = (int)floor(onFloor.x), y = (int)floor(onFloor.y);
        bool outside = (uint)x >= 1024 || (uint)y >= 1024;
        bool transparentOutside = outside && FloorOutside == 0;
        // Outside the floor, it shows nothing or repeats.
        uint2 at = uint2((uint)x & 1023, (uint)y & 1023);
        uint2 texel = Floor.Load(int3(at, 0));
        // `shown` is the colour a nonzero pixel shows.
        uint index;
        uint shown = TexelColour(texel, RoomLevels[RoomLevels[RoomFloorSheetsWord] + (texel.y >> FloorSheetShift)], index);
        uint color = index;
        if (transparentOutside) color = 0;
        if (Reserved27 == FloorAddsOwnColour)
        {
            // The floor shows over sprites drawn behind it (#1514); the frontmost of the two is then brightened by the
            // floor's own colour, unless it is a sprite that does not blend.
            bool blends;
            uint2 sprite = SpriteAt(id.xy, blends);
            bool spriteInFront = sprite.y != NoSprite && (sprite.y != DepthBehindRoom || color == 0);
            uint composed = spriteInFront ? sprite.x : color != 0 ? shown : Output[id.xy];
            if (color != 0 && (!spriteInFront || blends))
            {
                uint3 sum = min(31, (Unpack(composed) >> 3) + (Unpack(shown) >> 3));
                composed = Pack((sum << 3) | (sum >> 2), 255);
            }
            Output[id.xy] = composed;
            return;
        }
        if (color != 0)
        {
            uint composed = shown;
            uint2 sprite = SpriteAt(id.xy);
            if (Reserved27 == FloorSubtractsSprites && sprite.y != NoSprite)
            {
                int3 difference = max(0, (int3)(Unpack(composed) >> 3) - (int3)(Unpack(sprite.x) >> 3));
                uint3 result = (uint3)difference;
                composed = Pack((result << 3) | (result >> 2), 255);
            }
            Output[id.xy] = composed;
        }
        return;
    }
    if (Operation == OpInsertSprites)
    {
        bool blends;
        uint2 winner = SpriteAt(id.xy, blends);
        if (winner.y != NoSprite && (DrawnDepth == 0 || DrawnDepth - 1 == winner.y))
        {
            // AddR makes the sprites glow, adding each one's colour to the screen's; AddG tints the sprites that blend by
        // AddB's five-bit colour. The sums are of five-bit channels, before brightness (#1514).
            if (AddR != 0)
            {
                uint3 color = min(31, (Unpack(Output[id.xy]) >> 3) + (Unpack(winner.x) >> 3));
                Output[id.xy] = Pack((color << 3) | (color >> 2), 255);
            }
            else if (AddG != 0)
            {
                uint3 fixedColor = uint3(AddB & 31, (AddB >> 5) & 31, (AddB >> 10) & 31);
                uint3 color = min(31, (Unpack(winner.x) >> 3) + fixedColor);
                Output[id.xy] = blends ? Pack((color << 3) | (color >> 2), 255) : winner.x;
            }
            else Output[id.xy] = winner.x;
        }
        return;
    }
    if (Operation == OpBackdrop) { Output[id.xy] = Backdrop(); return; }
    if (Operation == OpBrightness)
    {
        uint packed = Output[id.xy];
        if ((packed >> 24) != 0) Output[id.xy] = Pack(Unpack(packed) * Level / 15, packed >> 24);
        return;
    }
    if (Operation == OpAddColour)
    {
        uint3 c = min(31, (Unpack(Output[id.xy]) * 31 + 127) / 255 + uint3(AddR, AddG, AddB));
        Output[id.xy] = Pack((c << 3) | (c >> 2), 255); return;
    }
    bool backgroundMath = Operation == OpAddLayer || Operation == OpSubtractLayer;
    // Background math and passes that mark Reserved3 take each line's scroll from their described line scrolls (#1468).
    uint2 scroll = (backgroundMath || Reserved3 != 0) ? LineScrollPair(SlotHorizontal, SlotVertical, id.y) : uint2(HorizontalScroll, VerticalScroll);
    // The mosaic groups screen pixels before the line's own scroll.
    uint2 sampleScreen = id.xy;
    if (Operation == OpLayer && Reserved27 > 1) sampleScreen -= sampleScreen % Reserved27;
    // A room layer's back tiles and its front tiles are drawn as layers of their own (#1514): each pass draws the pictures
    // of the part it chooses (#1634).
    uint color;
    uint sampled;
    if (!LayerPixel(sampleScreen, scroll, id.xy, sampled, color)) return;
    // A see-through layer leaves its clear pixels out; a layer drawn opaque shows a clear pixel's stored colour, unshaded,
    // black where it keeps none (#1544, #1633).
    if (color == 0 && TransparentZero != 0) return;
    if (Operation == OpAddLayerBehind)
    {
        if (!GlowCovers(id.xy)) return;
        uint3 sum = min(31, (Unpack(Output[id.xy]) >> 3) + (Unpack(sampled) >> 3));
        Output[id.xy] = Pack((sum << 3) | (sum >> 2), 255);
    }
    else if (backgroundMath)
    {
        uint pixel = Output[id.xy];
        int3 baseColor = int3(Unpack(pixel)), overlay = int3(Unpack(sampled));
        int3 result = Operation == OpAddLayer ? min(255, baseColor + overlay) : max(0, baseColor - overlay);
        Output[id.xy] = Pack(uint3(result), pixel >> 24);
    }
    else Output[id.xy] = sampled;
}
