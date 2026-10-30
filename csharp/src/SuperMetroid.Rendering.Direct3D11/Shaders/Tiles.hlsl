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
    uint Operation, TilemapWord, Reserved1, HorizontalScroll;
    uint VerticalScroll, MapWidth, MapHeight, PriorityFilter;
    uint TransparentZero, Level, AddR, AddG;
    uint AddB, Reserved2, RoomSource, Reserved3;
    int MatrixA, MatrixB, MatrixC, MatrixD;
    int CenterX, CenterY, OffsetX, OffsetY;
    uint FloorOutside, FirstScanline, EndScanline;
    uint Reserved27;
    // A described line displacement (#1468, D3D11LineDisplacement): its kind, its band of lines (first, then the line
    // after its last in the high half), its scale and its shape word.
    uint DisplacementKind, DisplacementBand, DisplacementScale, DisplacementShape;
    uint4 ScanlineParameters[224];
};
// The scene's colours (D3D11SceneColourBuffer): the backdrop, then each colour slot's table's word, then the tables:
// a word of one bit per sixteen-ink group still showing its image's own colours, then each ink's colour.
StructuredBuffer<uint> Colours : register(t0);
// Room level planes (D3D11RoomLevelBuffer): layer-1, layer-2 and door-source plane records, the door window, the layer-1 overlay,
// and the pre-coloured block pictures the planes' definitions show (#1392).
StructuredBuffer<uint> RoomLevels : register(t3);
// RoomSource: what a layer pass draws: the room's layer 1 or 2 as level data shows it, the sky's rows, the layer-3
// plane, a boss body, the X-ray reveal, a library background, the HUD, a pictured plane or a picture grid.
// SourceLayer2Overlay added to it lays words written over layer 2 on top.
static const uint SourceLayer1 = 1, SourceLayer2 = 2, SourceSky = 3, SourceLayer3 = 4;
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
static const uint RoomBg3Word = 21, RoomBg2OverlayWord = 22, RoomFloorSheetsWord = 23, RoomBossBodyWord = 24, RoomXrayRevealWord = 25;
// A library background (RoomBackgroundImagePlane): its image picture's offset, then each 16x16 cell's picture offset.
static const uint RoomBg2ImageWord = 26;
// The scrolling sky's image, laid out as a library background's: its image picture's offset, then each cell's.
static const uint RoomSkyImageWord = 27;
// The image of the message box the frame shows (#1392).
static const uint RoomMessagePictureWord = 28;
// The HUD's placed pictures (#1466): their count, then two words each in drawing order, the picture's offset and its
// upper-left screen pixel, x below y.
static const uint RoomHudWord = 29;
// The X-ray reveal (XrayRevealPlane): 17 block columns by 16 rows, each its picture's offset with its arrangement in the
// top four bits: as defined, mirrored horizontally, vertically or both, or its tile rows swapped.
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
// its pixel, repeating every 2048 pixels down and 256 across; a row selecting no page shows nothing.
bool SkyPicture(uint2 scrolled, out uint2 within, out uint picture)
{
    uint2 position = uint2(scrolled.x & 255, scrolled.y & 2047);
    uint skyImage = RoomLevels[RoomSkyImageWord];
    uint cell = RoomLevels[skyImage + 1 + (position.y >> 4) * 16 + (position.x >> 4)];
    picture = cell != 0 ? cell : RoomLevels[skyImage];
    within = cell != 0 ? (position & 15) : position;
    return RoomLevels[RoomLevels[RoomSkyWord] + (position.y >> 3)] != 0;
}
// The layer-3 plane (Layer3Plane): 32 by 64 cells. A window of `rows` rows from `firstRow` repeats every 256 pixels across.
// Where the plane's pictures are resolved (#1392), `picture` is the word's picture and `within` its pixel in it.
bool Bg3Picture(uint firstRow, uint rows, uint2 scrolled, out uint2 within, out uint picture)
{
    uint index = (firstRow + (scrolled.y >> 3) % rows) * 32 + ((scrolled.x >> 3) & 31);
    uint pictures = RoomLevels[RoomBg3Word];
    picture = RoomLevels[pictures + 2 * index];
    uint corner = RoomLevels[pictures + 2 * index + 1];
    within = (scrolled & 7) + uint2(corner & 65535, corner >> 16);
    return picture != 0;
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
// The picture of a word written over layer 2 (RoomOverlayPictures, #1403) at the cell a `size` layout places the
// scrolled position, laid out as a boss body's pictures with none where nothing was written; `within` its pixel in it.
bool Bg2OverlayPicture(uint2 scrolled, uint2 size, out uint picture, out uint2 within)
{
    uint2 tile = (scrolled & (size * 8 - 1)) >> 3;
    uint index = ((tile.y >> 5) * (size.x >> 5) + (tile.x >> 5)) * 1024 + (tile.y & 31) * 32 + (tile.x & 31);
    uint pictures = RoomLevels[RoomBg2OverlayWord];
    within = scrolled & 7;
    picture = pictures != 0 && index < 2048 ? RoomLevels[pictures + 2 * index] : 0;
    if (picture != 0)
    {
        uint corner = RoomLevels[pictures + 2 * index + 1];
        within += uint2(corner & 65535, corner >> 16);
    }
    return picture != 0;
}
// A boss body (ShownBossBody, #1466) at a scrolled position of its layer at the pass's map size: the map size lays the
// layer's 32-row pages out, and the part showing the position's cell gives `picture` and `within`, its pixel there. Each
// part is four words: its picture's offset, the layer row its picture's top edge lies on, its first row shown and the
// rows it spans (that in the high half), and the offset of one word per row, a bit per cell it shows. A picture repeats
// across and down from the layer's left edge and its top row, as the one-cell blank does. Every layer-2 sampler reads the
// boss through this, so a color-math pass shows the same body as an ordinary one (#1426).
void BossBodyPicture(uint2 scrolled, uint2 size, out uint2 within, out uint picture)
{
    uint2 position = scrolled & 65535 & (size * 8 - 1);
    uint2 tile = position >> 3;
    uint page = (tile.y >> 5) * (size.x >> 5) + (tile.x >> 5);
    uint column = tile.x & 31;
    uint row = page * 32 + (tile.y & 31);
    uint2 layerPixel = uint2(column * 8 + (position.x & 7), row * 8 + (position.y & 7));
    uint table = RoomLevels[RoomBossBodyWord];
    uint parts = RoomLevels[table];
    picture = 0;
    within = uint2(0, 0);
    for (uint part = 0; part < parts; part++)
    {
        uint record = table + 1 + part * 4;
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
// The X-ray reveal's block picture and pixel at a scrolled layer-2 position (#1392); false past the blocks it builds,
// which layer 2's scroll never shows.
bool RevealPicture(uint2 scrolled, out uint picture, out uint2 within)
{
    uint2 position = uint2(scrolled.x & 511, scrolled.y & 255);
    uint2 block = position >> 4;
    picture = 0;
    within = position & 15;
    if (block.x >= RevealColumns || block.y >= RevealRows) return false;
    uint entry = RoomLevels[RoomLevels[RoomXrayRevealWord] + block.y * RevealColumns + block.x];
    uint arrangement = entry >> 28;
    picture = entry & 0x0FFFFFFF;
    if (arrangement == 1 || arrangement == 3) within.x = 15 - within.x;
    if (arrangement == 2 || arrangement == 3) within.y = 15 - within.y;
    if (arrangement == 4) within.y = (within.y + 8) & 15;
    // A block copied from beyond the room has no picture and shows nothing.
    return picture != 0;
}
// The picture and pixel a room's library background shows at a scrolled layer-2 position through a `size` map (#1392): a
// rebound cell's picture, or the image of both pages; false on a page past the two the image pictures.
bool Bg2ImagePicture(uint2 scrolled, uint2 size, out uint picture, out uint2 within)
{
    uint2 position = scrolled & (size * 8 - 1);
    uint2 tile = position >> 3;
    uint page = (tile.y >> 5) * (size.x >> 5) + (tile.x >> 5);
    uint2 pagePixel = position & 255;
    uint image = RoomLevels[RoomBg2ImageWord];
    uint cell = RoomLevels[image + 1 + page * 256 + (pagePixel.y >> 4) * 16 + (pagePixel.x >> 4)];
    picture = cell != 0 ? cell : RoomLevels[image];
    within = cell != 0 ? (pagePixel & 15) : uint2(pagePixel.x, page * 256 + pagePixel.y);
    return page < 2;
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
        uint overlay = RoomLevels[RoomOverlayWord];
        if (RoomLevels[RoomDoorSlot * RoomRecordWords + 1] != 0 && doorWithin.x < 256 && doorWithin.y < 192)
            slot = RoomDoorSlot;
        else if (overlay != 0)
        {
            // A word written over layer 1 draws from its picture (#1403); none where nothing was written.
            uint2 cell = uint2((scrolled.x >> 3) & 63, (scrolled.y >> 3) & 31);
            uint index = (cell.x >> 5) * 1024 + cell.y * 32 + (cell.x & 31);
            picture = RoomLevels[overlay + 2 * index];
            overlaid = picture != 0;
            within = scrolled & 7;
            if (overlaid)
            {
                uint corner = RoomLevels[overlay + 2 * index + 1];
                within += uint2(corner & 65535, corner >> 16);
            }
        }
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
// The colour an image texel shows when its image is coloured from colour slot `slot` (#1408): whether it shows a colour
// other than colour zero (`index`), its tile priority, and its colour: the image's own while its ink's group shows its
// image's own colours, else the ink's colour now. A colour-zero pixel shows nothing, or, where it keeps its group, that
// group's colour zero when its layer shows it.
uint TexelColour(uint2 texel, uint slot, out uint index, out bool high)
{
    uint meta = texel.y;
    uint ink = meta & 255;
    high = ((meta >> 8) & 3) != 0;
    uint table = Colours[1 + slot];
    if ((meta & InkColourZero) != 0)
    {
        index = 0;
        return (meta & InkKeepsGroup) != 0 ? Colours[table + 1 + ink] : 0;
    }
    index = 1;
    bool own = ((Colours[table] >> (ink >> 4)) & 1) != 0;
    return own ? texel.x : Colours[table + 1 + ink];
}
// A picture's pixel (#1392, #1408), as TexelColour colours it from the picture's colour slot.
uint PicturePixel(uint picture, uint2 pixel, out uint index, out bool high)
{
    uint place = RoomLevels[picture + PicturePlaceWord];
    uint2 texel = Images.Load(int3(uint2(place & 65535, place >> 16) + pixel, 0));
    return TexelColour(texel, RoomLevels[picture + PictureColoursWord], index, high);
}

// These kinds match D3D11LineDisplacementKind.cs (#1468).
static const uint DisplacementWave = 1, DisplacementStretch = 2;
// The game's signed sine samples over a quarter turn of its 256-step angle (Trigonometry.SignedSine): 256 times the
// sine, rounded down, with the quarter turn itself at 256. A wave reads these, not the device's sine, so every device
// draws the same lines.
static const int QuarterSine[65] =
{
    0, 6, 12, 18, 25, 31, 37, 43, 49, 56, 62, 68, 74, 80, 86, 92, 97, 103, 109, 115, 120, 126, 131, 136, 142, 147, 152,
    157, 162, 167, 171, 176, 181, 185, 189, 193, 197, 201, 205, 209, 212, 216, 219, 222, 225, 228, 231, 234, 236, 238,
    241, 243, 244, 246, 248, 249, 251, 252, 253, 254, 254, 255, 255, 255, 256
};
// The described shape's displacement of screen line `screenY` (#1468), in pixels. A wave (ScrollWave) pushes each line of
// its band sideways by its amplitude (1/256ths of a pixel) times the sine at the line's angle: the angle at line zero
// advanced by the line's share of the period's whole turn, rounded toward zero. A stretch (ScrollStretch) sags each line
// of its band upward through the plane by its slope (1/65536ths of a row) for each line below the band's first, and
// moves the lines above its band by its shape word.
int2 LineDisplacement(uint screenY)
{
    uint bandFirst = DisplacementBand & 65535, bandEnd = DisplacementBand >> 16;
    if (DisplacementKind == DisplacementWave)
    {
        if (screenY < bandFirst || screenY >= bandEnd) return int2(0, 0);
        uint period = DisplacementShape >> 16;
        uint angle = ((DisplacementShape & 255) + screenY * 256 / period) & 255;
        uint quarter = angle & 127;
        if (quarter > 64) quarter = 128 - quarter;
        int magnitude = (int)(((uint)QuarterSine[quarter] * DisplacementScale) >> 16);
        return int2(angle >= 128 ? -magnitude : magnitude, 0);
    }
    if (DisplacementKind == DisplacementStretch)
    {
        if (screenY < bandFirst) return int2(0, (int)DisplacementShape);
        if (screenY >= bandEnd) return int2(0, 0);
        return int2(0, -(int)(((screenY - bandFirst) * DisplacementScale) >> 16));
    }
    return int2(0, 0);
}
// A screen line's scroll displaced by the pass's described shape, wrapped as a sixteen-bit scroll.
uint2 DisplacedScroll(uint2 scroll, uint screenY) { return (uint2)((int2)scroll + LineDisplacement(screenY)) & 65535; }

#include "SpriteLayer.hlsli"
#include "Xray.hlsli"

// Whether the screen pixel lies where the plane covering a plane added behind it shows (#1467), or everywhere without
// one. An opaque pixel of the covering plane's picture covers, in its picture's priority tier; where no picture lies,
// nothing covers and the tier is the back one. The covering plane's table is in MatrixA, its scroll in MatrixC and
// MatrixD, its priority filter in OffsetX, and OffsetY marks that it is present.
bool MainCoverage(uint2 screen)
{
    uint4 geometry = uint4(CenterX, CenterY, OffsetX, OffsetY);
    if (geometry.w == 0) return true;
    uint4 map = uint4(MatrixA, MatrixB, MatrixC, MatrixD);
    uint2 coverWithin;
    uint coverPicture;
    uint coverIndex = 0;
    bool coverHigh = false;
    bool coverShown = PlanePicture(map.x, (screen + map.zw) & 65535, coverWithin, coverPicture);
    bool coverCovered = coverShown && PicturePixel(coverPicture, coverWithin, coverIndex, coverHigh) != 0 && coverIndex != 0;
    if (geometry.z != 0 && (coverHigh ? 1u : 0u) != geometry.z - 1) return false;
    if (AddG != 0)
    {
        bool coverBlends;
        uint2 coverObj = SpriteAt(screen, coverBlends);
        if (coverObj.y != NoSprite && (!coverCovered || coverObj.y >= (coverHigh ? 3 : 2)))
            return coverBlends;
    }
    return coverCovered;
}

[numthreads(8, 8, 1)]
void Main(uint3 id : SV_DispatchThreadID)
{
    if (id.x >= 256 || id.y >= 224) return;
    if (id.y < FirstScanline || id.y >= EndScanline) return;
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
        uint4 window = ScanlineParameters[id.y];
        uint left = window.x & 255, right = (window.x >> 8) & 255;
        if (id.x >= left && id.x <= right)
        {
            uint pixel = Output[id.xy];
            Output[id.xy] = Pack(min(255, Unpack(pixel) + window.yzw), pixel >> 24);
        }
        return;
    }
    if (Operation == OpFloor)
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
            // AddR adds the sprite's colour to the screen's; the sums are of five-bit channels, before brightness.
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
    uint2 scroll = DisplacedScroll((backgroundMath || Reserved3 != 0) ? ScanlineParameters[id.y].xy : uint2(HorizontalScroll, VerticalScroll), id.y);
    // The mosaic groups screen pixels before the line's own scroll.
    uint2 sampleScreen = id.xy;
    if (Operation == OpLayer && Reserved27 > 1) sampleScreen -= sampleScreen % Reserved27;
    uint entry = 0;
    uint picture = 0;
    uint2 within = uint2(0, 0);
    uint2 scrolled = (sampleScreen + scroll) & 65535;
    uint source = RoomSource & 255;
    // A word written over layer 2 shows its picture over whatever layer 2 shows there (#1403).
    bool overlaid = (RoomSource & SourceLayer2Overlay) != 0 && Bg2OverlayPicture(scrolled, uint2(MapWidth, MapHeight), picture, within);
    if (!overlaid)
    {
        if (source == SourceLayer3)
        {
            if (!Bg3Picture(TilemapWord, MapHeight, scrolled, within, picture)) return;
        }
        else if (source == SourceBossBody)
            BossBodyPicture(scrolled, uint2(MapWidth, MapHeight), within, picture);
        else if (source == SourceXrayReveal)
        {
            if (!RevealPicture(scrolled, picture, within)) return;
        }
        else if (source == SourceLibraryBackground)
        {
            if (!Bg2ImagePicture(scrolled, uint2(MapWidth, MapHeight), picture, within)) return;
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
    bool objectSubscreen = Operation == OpAddLayerBehind && AddR != 0;
    uint2 subObject = objectSubscreen ? SpriteAt(id.xy) : uint2(0, NoSprite);
    bool objectWins = objectSubscreen && subObject.y != NoSprite &&
        (color == 0 || subObject.y >= (high ? 3u : 2u));
    if (color == 0 && TransparentZero != 0 && !objectWins) return;
    if (objectWins) sampled = subObject.x;
    if (Operation == OpAddLayerBehind)
    {
        if (!MainCoverage(id.xy)) return;
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
