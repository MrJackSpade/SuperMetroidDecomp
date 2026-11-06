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
    // LayerWindowed marks a layer pass drawn only within its window (#1514), whose shapes WindowShapes holds.
    uint Operation, TilemapWord, LayerWindowed, HorizontalScroll;
    uint VerticalScroll, Reserved5, Reserved6, PriorityFilter;
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
// each ink's colour under the slot's shading (#1512).
StructuredBuffer<uint> Colours : register(t0);
// Room level planes (D3D11RoomLevelBuffer): layer-1, layer-2 and door-source plane records, the door window, the layer-1 overlay,
// and the pre-coloured block pictures the planes' definitions show (#1392).
StructuredBuffer<uint> RoomLevels : register(t3);
// RoomSource: what a layer pass draws: the room's layer 1 or 2 as level data shows it, the sky's rows, the effect
// overlay, a boss body, the X-ray reveal, a library background, the HUD, a pictured plane or a picture grid.
// SourceLayer2Overlay added to it lays words written over layer 2 on top.
static const uint SourceLayer1 = 1, SourceLayer2 = 2, SourceSky = 3, SourceFxOverlay = 4;
static const uint SourceBossBody = 6, SourceXrayReveal = 7, SourceLibraryBackground = 8;
// The HUD's placed pictures (PicturedHud, #1466), at the HUD's place at the top of the screen.
static const uint SourceHud = 10;
// A pictured plane (PicturedPlane, #1467), its table's word in TilemapWord: its size in cells, x below y, the count of its
// pictures, then two words each in drawing order, the picture's offset and its upper-left plane pixel, x below y.
static const uint SourcePicturedPlane = 11;
// A frontend picture grid (PictureGridLayer, #1467), its table at room level word `TilemapWord`: its width and height in
// cells, x below y, then a word per cell row by row, its one-cell picture's offset (zero for none) with its horizontal and
// vertical mirroring in bits 30 and 31. The grid wraps at its edges.
static const uint SourcePictureGrid = 12;
static const uint GridFlipX = 1073741824, GridFlipY = 2147483648, GridPictureMask = 1073741823;
static const uint SourceLayer2Overlay = 256;
static const uint RoomRecordWords = 6, RoomDoorSlot = 2, RoomDoorWindowWord = 18, RoomOverlayWord = 19, RoomSkyWord = 20;
static const uint RoomFxOverlayWord = 21, RoomBg2OverlayWord = 22, RoomFloorSheetsWord = 23, RoomBossBodyWord = 24, RoomXrayRevealWord = 25;
// A library background (RoomBackgroundImagePlane): its image picture's offset, then each 16x16 cell's picture offset.
static const uint RoomBg2ImageWord = 26;
// The scrolling sky's image, laid out as a library background's: its image picture's offset, then each cell's.
static const uint RoomSkyImageWord = 27;
// The image of the message box the frame shows (#1392).
static const uint RoomMessagePictureWord = 28;
// The HUD's placed pictures (#1466): their count, then two words each in drawing order, the picture's offset and its
// upper-left screen pixel, x below y.
static const uint RoomHudWord = 29;
// The X-ray reveal (XrayRevealPlane, #1514): the foreground pixel its first block lies on, x below y, then 17 block columns
// by 16 rows, each its picture's offset with its arrangement in the top four bits: as defined, mirrored horizontally,
// vertically or both, or its tile rows swapped.
static const uint RevealColumns = 17, RevealRows = 16;
// A picture (a block, or a background image): its colour slot, its place in the image texture and its width and height,
// x below y.
static const uint PictureColoursWord = 0, PicturePlaceWord = 1, PictureSizeWord = 2;
// The installed images (D3D11ImageAtlas), each texel the image's pre-coloured pixel, then its ink word: the ink, its
// tile priority above it, and above that whether it is colour zero and whether it keeps its group.
Texture2D<uint2> Images : register(t4);
static const uint InkColourZero = 65536, InkKeepsGroup = 131072;
// The perspective floor's image (D3D11FloorImage): the 1024-pixel floor with its states' pictures laid over it. Each
// texel's ink word keeps the index of the floor picture it comes from.
Texture2D<uint2> Floor : register(t5);
static const uint FloorSheetShift = 20;
RWTexture2D<uint> Output : register(u0);
RWTexture2D<uint2> SpriteLayer : register(u1);

// What a room plane shows at a scrolled position; false outside the room: its block's picture (#1392), mirrored by the
// block's flips, with `within` the picture pixel. Each packed block is its definition with its horizontal and vertical
// mirroring in bits ten and eleven (#1403).
bool RoomPlaneTile(uint slot, uint2 scrolled, out uint entry, out uint2 within, out uint picture)
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
        uint h = (word >> 10) & 1, v = (word >> 11) & 1;
        picture = RoomLevels[RoomLevels[record + 5] + (word & 1023)];
        within = uint2(h != 0 ? 15 - (level.x & 15) : level.x & 15, v != 0 ? 15 - (level.y & 15) : level.y & 15);
    }
    return inside;
}
// What the scrolling sky (SkyPictures, #1403) shows: its image or a rebound cell's picture as `picture`, with `within`
// its pixel, repeating every 2048 pixels down and 256 across; a row selecting no page shows nothing. Its image's record is
// a library background's: the picture, its area, then its cells.
bool SkyPicture(uint2 scrolled, out uint2 within, out uint picture)
{
    uint2 position = uint2(scrolled.x & 255, scrolled.y & 2047);
    uint skyImage = RoomLevels[RoomSkyImageWord];
    uint cell = RoomLevels[skyImage + 2 + (position.y >> 4) * 16 + (position.x >> 4)];
    picture = cell != 0 ? cell : RoomLevels[skyImage];
    within = cell != 0 ? (position & 15) : position;
    return RoomLevels[RoomLevels[RoomSkyWord] + (position.y >> 3)] != 0;
}
// What the effect overlay shows at a scrolled position (OverlayPicture, #1514), from its record at room level word
// RoomFxOverlayWord: its picture's offset, the picture pixel its rectangle's corner shows, the rectangle's size, the
// overlay pixel the corner lies on and the size the overlay repeats at, each x below y. The overlay repeats at that
// size; a position within the rectangle gives `picture`, with `within` its pixel there, and nothing shows elsewhere.
bool FxOverlayPicture(uint2 scrolled, out uint2 within, out uint picture)
{
    picture = 0;
    within = uint2(0, 0);
    uint table = RoomLevels[RoomFxOverlayWord];
    if (table == 0) return false;
    uint source = RoomLevels[table + 1], size = RoomLevels[table + 2];
    uint place = RoomLevels[table + 3], repeat = RoomLevels[table + 4];
    uint2 position = scrolled % uint2(repeat & 65535, repeat >> 16);
    uint2 offset = position - uint2(place & 65535, place >> 16);
    if (offset.x >= (size & 65535) || offset.y >= (size >> 16)) return false;
    picture = RoomLevels[table];
    within = uint2(source & 65535, source >> 16) + offset;
    return true;
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
// A pictured plane's picture at a scrolled position (#1467): the last placed picture covering it, the plane repeating at
// its edges and each picture continuing across them, as `picture`, with `within` its pixel there; false where none does.
bool PlanePicture(uint table, uint2 scrolled, out uint2 within, out uint picture)
{
    picture = 0;
    within = uint2(0, 0);
    uint cells = RoomLevels[table];
    uint2 size = uint2(cells & 65535, cells >> 16) * 8;
    uint2 position = scrolled % size;
    for (uint placed = RoomLevels[table + 1]; placed > 0; placed--)
    {
        uint record = table + 2 + (placed - 1) * 2;
        uint shown = RoomLevels[record];
        uint corner = RoomLevels[record + 1];
        uint2 offset = (position + size - uint2(corner & 65535, corner >> 16)) % size;
        uint extent = RoomLevels[shown + PictureSizeWord];
        if (offset.x >= (extent & 65535) || offset.y >= (extent >> 16)) continue;
        picture = shown;
        within = offset;
        return true;
    }
    return false;
}
// The cell picture of a picture grid (#1467) at a scrolled position, wrapping at the grid's edges, as `picture`, with
// `within` its pixel there, mirrored as the cell mirrors it; false where the cell shows nothing.
bool GridPicture(uint table, uint2 scrolled, out uint2 within, out uint picture)
{
    uint size = RoomLevels[table];
    uint2 cells = uint2(size & 65535, size >> 16);
    uint2 position = scrolled % (cells * 8);
    uint2 cell = position >> 3;
    uint entry = RoomLevels[table + 1 + cell.y * cells.x + cell.x];
    picture = entry & GridPictureMask;
    within = uint2((entry & GridFlipX) != 0 ? 7 - (position.x & 7) : position.x & 7,
        (entry & GridFlipY) != 0 ? 7 - (position.y & 7) : position.y & 7);
    return picture != 0;
}
// A picture written over a room's layer (RoomOverlayPictures, #1403, #1514) at a scrolled position of that layer, from the
// table at room level word `table`: its count, then two words each in the order written, the picture's offset and the
// layer pixel its corner lies on, x below y. The last written picture covering the position gives `picture`, with
// `within` its pixel there; false where none does, which leaves the layer showing.
bool WrittenPicture(uint table, uint2 scrolled, out uint2 within, out uint picture)
{
    picture = 0;
    within = uint2(0, 0);
    if (table == 0) return false;
    for (uint placed = RoomLevels[table]; placed > 0; placed--)
    {
        uint record = table + 1 + (placed - 1) * 2;
        uint shown = RoomLevels[record];
        uint corner = RoomLevels[record + 1];
        uint2 offset = (scrolled - uint2(corner & 65535, corner >> 16)) & 65535;
        uint extent = RoomLevels[shown + PictureSizeWord];
        if (offset.x >= (extent & 65535) || offset.y >= (extent >> 16)) continue;
        picture = shown;
        within = offset;
        return true;
    }
    return false;
}
// The picture written over the background at a scrolled position (#1348, #1514), as WrittenPicture finds it.
bool Bg2OverlayPicture(uint2 scrolled, out uint picture, out uint2 within)
{
    return WrittenPicture(RoomLevels[RoomBg2OverlayWord], scrolled, within, picture);
}
// A boss body (ShownBossBody, #1466) at a scrolled position of its layer: the body's area (#1514), in its table's second
// word, x below y, repeats beyond its edges and holds the layer's 32-row pages left to right and then downward, and the part
// showing the position's cell gives `picture` and `within`, its pixel there. Each part is four words: its picture's
// offset, the layer row its picture's top edge lies on, its first row shown and the rows it spans (that in the high
// half), and the offset of one word per row, a bit per cell it shows. A picture repeats across and down from the layer's
// left edge and its top row, as the one-cell blank does. Every layer-2 sampler reads the boss through this, so a
// color-math pass shows the same body as an ordinary one (#1426).
void BossBodyPicture(uint2 scrolled, out uint2 within, out uint picture)
{
    uint table = RoomLevels[RoomBossBodyWord];
    uint area = RoomLevels[table + 1];
    uint2 size = uint2(area & 65535, area >> 16) >> 3;
    uint2 position = (scrolled & 65535) % (size * 8);
    uint2 tile = position >> 3;
    uint page = (tile.y >> 5) * (size.x >> 5) + (tile.x >> 5);
    uint column = tile.x & 31;
    uint row = page * 32 + (tile.y & 31);
    uint2 layerPixel = uint2(column * 8 + (position.x & 7), row * 8 + (position.y & 7));
    uint parts = RoomLevels[table];
    picture = 0;
    within = uint2(0, 0);
    for (uint part = 0; part < parts; part++)
    {
        uint record = table + 2 + part * 4;
        uint shows = RoomLevels[record + 2];
        uint first = shows & 65535;
        if (row < first || row >= first + (shows >> 16)) continue;
        if (((RoomLevels[RoomLevels[record + 3] + row - first] >> column) & 1) == 0) continue;
        picture = RoomLevels[record];
        uint extent = RoomLevels[picture + PictureSizeWord];
        within = (layerPixel - uint2(0, RoomLevels[record + 1] * 8)) % uint2(extent & 65535, extent >> 16);
        return;
    }
}
// The X-ray reveal's block picture and pixel at a scrolled position of the foreground (#1392, #1514), the background
// moving with the foreground while it shows the reveal; false outside the blocks it reveals.
bool RevealPicture(uint2 scrolled, out uint picture, out uint2 within)
{
    uint table = RoomLevels[RoomXrayRevealWord];
    uint origin = RoomLevels[table];
    uint2 position = (scrolled - uint2(origin & 65535, origin >> 16)) & 65535;
    uint2 block = position >> 4;
    picture = 0;
    within = position & 15;
    if (block.x >= RevealColumns || block.y >= RevealRows) return false;
    uint entry = RoomLevels[table + 1 + block.y * RevealColumns + block.x];
    uint arrangement = entry >> 28;
    picture = entry & 0x0FFFFFFF;
    if (arrangement == 1 || arrangement == 3) within.x = 15 - within.x;
    if (arrangement == 2 || arrangement == 3) within.y = 15 - within.y;
    if (arrangement == 4) within.y = (within.y + 8) & 15;
    // A block copied from beyond the room has no picture and shows nothing.
    return picture != 0;
}
// The picture and pixel a room's library background shows at a scrolled position of the background (#1392): the image's
// area (#1514), in its record's second word, x below y, repeats beyond its edges and holds its 256-pixel pages left to right
// and then downward; a rebound cell's picture, or the image of the pages.
bool Bg2ImagePicture(uint2 scrolled, out uint picture, out uint2 within)
{
    uint image = RoomLevels[RoomBg2ImageWord];
    uint area = RoomLevels[image + 1];
    uint2 extent = uint2(area & 65535, area >> 16);
    uint2 position = (scrolled & 65535) % extent;
    uint page = (position.y >> 8) * (extent.x >> 8) + (position.x >> 8);
    uint2 pagePixel = position & 255;
    uint cell = RoomLevels[image + 2 + page * 256 + (pagePixel.y >> 4) * 16 + (pagePixel.x >> 4)];
    picture = cell != 0 ? cell : RoomLevels[image];
    within = cell != 0 ? (pagePixel & 15) : uint2(pagePixel.x, page * 256 + pagePixel.y);
    return true;
}
// Layer 2 from the room's layer-2 plane or the sky's rows, or layer 1 as RoomBackgrounds resolves it: a door's source
// screen, then words written over layer 1, then the level plane.
bool RoomTile(uint source, uint2 scrolled, out uint entry, out uint2 within, out uint picture)
{
    entry = 0;
    picture = 0;
    within = uint2(0, 0);
    scrolled &= 65535;
    uint slot = source == SourceLayer2 ? 1 : 0;
    bool overlaid = false;
    if (source != SourceLayer2 && source != SourceSky)
    {
        uint door = RoomLevels[RoomDoorWindowWord];
        uint2 doorWithin = (scrolled - uint2(door & 65535, door >> 16)) & 65535;
        if (RoomLevels[RoomDoorSlot * RoomRecordWords + 1] != 0 && doorWithin.x < 256 && doorWithin.y < 192)
            slot = RoomDoorSlot;
        else
            // A picture written over the foreground draws where it lies (#1403, #1514); none where nothing was written.
            overlaid = WrittenPicture(RoomLevels[RoomOverlayWord], scrolled, within, picture);
    }
    bool shown = overlaid;
    if (source == SourceSky)
        shown = SkyPicture(scrolled, within, picture);
    else if (!overlaid)
        shown = RoomPlaneTile(slot, scrolled, entry, within, picture);
    return shown;
}
uint Pack(uint3 c, uint alpha) { return c.r | (c.g << 8) | (c.b << 16) | (alpha << 24); }
uint3 Unpack(uint c) { return uint3(c & 255, (c >> 8) & 255, (c >> 16) & 255); }
// The colour shown where nothing is drawn.
uint Backdrop() { return Colours[0]; }
// The colour an image texel shows when its image is coloured from colour slot `slot` (#1408, #1512): whether it shows a
// colour other than colour zero (`index`), its tile priority, and its ink's colour under the slot's shading. A
// colour-zero pixel shows nothing, or, where it keeps its group, that group's colour zero when its layer shows it.
uint TexelColour(uint2 texel, uint slot, out uint index, out bool high)
{
    uint meta = texel.y;
    uint ink = meta & 255;
    high = ((meta >> 8) & 3) != 0;
    uint table = Colours[1 + slot];
    if ((meta & InkColourZero) != 0)
    {
        index = 0;
        return (meta & InkKeepsGroup) != 0 ? Colours[table + ink] : 0;
    }
    index = 1;
    return Colours[table + ink];
}
// A picture's pixel (#1392, #1408), as TexelColour colours it from the picture's colour slot.
uint PicturePixel(uint picture, uint2 pixel, out uint index, out bool high)
{
    uint place = RoomLevels[picture + PicturePlaceWord];
    uint2 texel = Images.Load(int3(uint2(place & 65535, place >> 16) + pixel, 0));
    return TexelColour(texel, RoomLevels[picture + PictureColoursWord], index, high);
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
// plane it glows over is opaque. That plane's table is in MatrixA, its scroll in MatrixC and MatrixD, and OffsetY marks
// that it is present.
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
    bool coverHigh = false;
    bool coverShown = PlanePicture(map.x, (screen + map.zw) & 65535, coverWithin, coverPicture);
    return coverShown && PicturePixel(coverPicture, coverWithin, coverIndex, coverHigh) != 0 && coverIndex != 0;
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
        // The message's image (#1392) from its top scanline, in the message's text colours (#1408): colour zero shows
        // nothing.
        uint picture = RoomLevels[RoomMessagePictureWord];
        uint height = RoomLevels[picture + PictureSizeWord] >> 16;
        if (picture == 0 || id.y < TilemapWord || id.y >= TilemapWord + height) return;
        uint index;
        bool high;
        uint shown = PicturePixel(picture, uint2(id.x, id.y - TilemapWord), index, high);
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
        bool high;
        uint shown = TexelColour(texel, RoomLevels[RoomLevels[RoomFloorSheetsWord] + (texel.y >> FloorSheetShift)], index, high);
        uint color = index;
        if (transparentOutside) color = 0;
        if (Reserved27 == 2)
        {
            // The floor shows over sprites of priority zero; the frontmost of the two is then brightened by the
            // floor's own colour, unless it is a sprite that does not blend.
            bool blends;
            uint2 obj = SpriteAt(id.xy, blends);
            bool objWins = obj.y != NoSprite && (obj.y != 0 || color == 0);
            uint main = objWins ? obj.x : color != 0 ? shown : Output[id.xy];
            if (color != 0 && (!objWins || blends))
            {
                uint3 sum = min(31, (Unpack(main) >> 3) + (Unpack(shown) >> 3));
                main = Pack((sum << 3) | (sum >> 2), 255);
            }
            Output[id.xy] = main;
            return;
        }
        if (color != 0)
        {
            uint main = shown;
            uint2 sub = SpriteAt(id.xy);
            if (Reserved27 != 0 && sub.y != NoSprite)
            {
                int3 difference = max(0, (int3)(Unpack(main) >> 3) - (int3)(Unpack(sub.x) >> 3));
                uint3 result = (uint3)difference;
                main = Pack((result << 3) | (result >> 2), 255);
            }
            Output[id.xy] = main;
        }
        return;
    }
    if (Operation == OpInsertSprites)
    {
        bool blends;
        uint2 winner = SpriteAt(id.xy, blends);
        if (winner.y != NoSprite && (PriorityFilter == 0 || PriorityFilter - 1 == winner.y))
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
    uint entry = 0;
    uint picture = 0;
    uint2 within = uint2(0, 0);
    uint2 scrolled = (sampleScreen + scroll) & 65535;
    uint source = RoomSource & 255;
    // A word written over layer 2 shows its picture over whatever layer 2 shows there (#1403).
    bool overlaid = (RoomSource & SourceLayer2Overlay) != 0 && Bg2OverlayPicture(scrolled, picture, within);
    if (!overlaid)
    {
        if (source == SourceFxOverlay)
        {
            if (!FxOverlayPicture(scrolled, within, picture)) return;
        }
        else if (source == SourceBossBody)
            BossBodyPicture(scrolled, within, picture);
        else if (source == SourceXrayReveal)
        {
            if (!RevealPicture(scrolled, picture, within)) return;
        }
        else if (source == SourceLibraryBackground)
        {
            if (!Bg2ImagePicture(scrolled, picture, within)) return;
        }
        else if (source == SourceHud)
        {
            if (!HudPicture(id.xy, within, picture)) return;
        }
        else if (source == SourcePicturedPlane)
        {
            if (!PlanePicture(TilemapWord, scrolled, within, picture)) return;
        }
        else if (source == SourcePictureGrid)
        {
            if (!GridPicture(TilemapWord, scrolled, within, picture)) return;
        }
        else
        {
            if (!RoomTile(source, scrolled, entry, within, picture)) return;
        }
    }
    // Every background draws from pictures (#1403); a position without one shows nothing.
    if (picture == 0) return;
    uint color;
    bool high;
    // A block picture keeps no row for its transparent pixels; room planes show colour zero as transparent.
    uint sampled = PicturePixel(picture, within, color, high);
    if (PriorityFilter != 0 && (high ? 1u : 0u) != PriorityFilter - 1) return;
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
