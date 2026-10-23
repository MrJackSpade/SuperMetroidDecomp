// These operation IDs and the packed memory layout match D3D11ShaderLayout.cs.
static const uint OpBackdrop = 0, OpBg4 = 1, OpBg2 = 2, OpBrightness = 3, OpFixedAdd = 4;
static const uint OpResolveObj = 5, OpInsertObj = 6;
static const uint OpMode7 = 7;
static const uint OpScanlineAdd = 8;
static const uint OpBgAdd = 9, OpBgSubtract = 10;
static const uint OpSubscreenAdd = 11;
static const uint OpMessage = 12;
static const uint OpXrayGameplay = 14;
static const uint PaletteOffset = 16384;
cbuffer TileParameters : register(b0)
{
    uint Operation, TilemapWord, CharacterWord, HorizontalScroll;
    uint VerticalScroll, MapWidth, MapHeight, PriorityFilter;
    uint TransparentZero, Level, AddR, AddG;
    uint AddB, MessageDarkIndex, RoomSource, Reserved3;
    int MatrixA, MatrixB, MatrixC, MatrixD;
    int CenterX, CenterY, OffsetX, OffsetY;
    uint FillCharacterZero, FirstScanline, EndScanline;
    uint Reserved27;
    uint WindowSelection, WindowLogic, WindowEdges, WindowEnabled;
    uint4 ScanlineParameters[224];
    // A subscreen add's main coverage layer's character tile table (#1392), or zero while it reads VRAM's characters.
    uint CoverageTable;
};
StructuredBuffer<uint> Memory : register(t0);
// Room level planes (D3D11RoomLevelBuffer): BG1, BG2 and door-source plane records, the door window, the BG1 overlay,
// and the pre-coloured block pictures the planes' definitions show (#1392).
StructuredBuffer<uint> RoomLevels : register(t3);
// RoomSource: 0 samples the VRAM tilemap; otherwise the room's BG1 or BG2 as level data shows it, the sky's rows,
// the BG3 plane, or a boss body's BG2 tilemap. RoomSourceBg2Overlay added to it lays words written over BG2 on top.
static const uint RoomSourceBg1 = 1, RoomSourceBg2 = 2, RoomSourceSky = 3, RoomSourceBg3 = 4, RoomSourceMode7Map = 5;
static const uint RoomSourceBossBg2 = 6, RoomSourceXrayReveal = 7, RoomSourceBg2Image = 8;
// A frontend layer's VRAM tilemap drawn from its characters' pre-coloured tiles (#1392), its table's word in RoomSource's
// high half: three words a character index, the tile's picture (zero where no artwork shows the character), the corner
// of its palette row zero tile, x below y, and the pixels from one palette row's block to the next.
static const uint RoomSourceScreenTiles = 9;
static const uint RoomSourceBg2Overlay = 256;
static const uint RoomRecordWords = 7, RoomDoorSlot = 2, RoomDoorWindowWord = 21, RoomOverlayWord = 22, RoomSkyWord = 23;
static const uint RoomBg3Word = 24, RoomBg2OverlayWord = 25, RoomMode7MapWord = 26, RoomBossBg2Word = 27, RoomXrayRevealWord = 28;
// A library background (RoomBackgroundImagePlane): its image picture's offset, then each 16x16 cell's picture offset.
static const uint RoomBg2ImageWord = 29;
// The scrolling sky's image, laid out as a library background's: its image picture's offset, then each cell's.
static const uint RoomSkyImageWord = 30;
// A boss body's pictures (#1392): two words per tilemap word, its picture's offset (zero where it draws from its
// character), then its corner in the picture, x below y.
static const uint RoomBossBg2PicturesWord = 31;
// The BG2 overlay's pictures (#1392), laid out as a boss body's.
static const uint RoomBg2OverlayPicturesWord = 32;
// The image of the message box the frame shows (#1392).
static const uint RoomMessagePictureWord = 33;
// The picture of a Ceres room's Mode 7 tiles (#1392), sixteen 8x8 tiles to a row, or zero while the map draws from VRAM.
static const uint RoomMode7PictureWord = 34;
// The BG1 overlay's pictures (#1392), laid out as a boss body's.
static const uint RoomBg1OverlayPicturesWord = 35;
// The X-ray reveal (XrayRevealPlane): 17 block columns by 16 rows, each its picture's offset with its arrangement in the
// top four bits: as defined, mirrored horizontally, vertically or both, or its tile rows swapped.
static const uint RevealColumns = 17, RevealRows = 16;
// A picture (a block, or a background image): its sheet's standard colours' offset, its width, its pixel count, then
// its colours, then each pixel's palette index and, above it, its tile priority.
static const uint PictureColoursWord = 0, PictureWidthWord = 1, PictureCountWord = 2, PicturePixelsWord = 3;
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
// What a room plane shows at a scrolled position; false outside the room. A plane with pictures (#1392) shows its
// block's picture, mirrored by the block's flips, and `within` is the picture pixel; otherwise it shows a tile and the
// pixel within it, a mirrored block showing its definition's opposite quadrant with that tile's own flip toggled
// (LevelBlockTilemapExpander).
bool RoomPlaneTile(uint slot, uint2 scrolled, out uint entry, out uint2 within, out uint picture)
{
    uint record = slot * RoomRecordWords;
    uint width = RoomLevels[record + 1], height = RoomLevels[record + 2];
    uint2 level = (scrolled - uint2(RoomLevels[record + 4], RoomLevels[record + 5])) & 65535;
    entry = 0;
    picture = 0;
    within = level & 7;
    bool inside = level.x < width * 16 && level.y < height * 16;
    if (inside)
    {
        uint word = RoomLevels[RoomLevels[record] + (level.y >> 4) * width + (level.x >> 4)];
        uint h = (word >> 10) & 1, v = (word >> 11) & 1;
        uint pictures = RoomLevels[record + 6];
        if (pictures != 0)
        {
            picture = RoomLevels[pictures + (word & 1023)];
            within = uint2(h != 0 ? 15 - (level.x & 15) : level.x & 15, v != 0 ? 15 - (level.y & 15) : level.y & 15);
        }
        else
        {
            uint quadrant = ((((level.y >> 3) & 1) ^ v) << 1) | (((level.x >> 3) & 1) ^ h);
            entry = RoomLevels[RoomLevels[record + 3] + (word & 1023) * 4 + quadrant] ^ (h << 14) ^ (v << 15);
        }
    }
    return inside;
}
// The tile a sky row shows (SkyTilemap): 32-tile rows repeating every 2048 pixels down; a row selecting no page
// shows nothing. Where the sky is drawn from its image (#1392), `picture` is the image or a rebound cell's picture and
// `within` its pixel.
bool SkyTile(uint2 scrolled, out uint entry, out uint2 within, out uint picture)
{
    picture = 0;
    uint word = RoomLevels[RoomLevels[RoomSkyWord] + ((scrolled.y & 2047) >> 3) * 32 + ((scrolled.x >> 3) & 31)];
    bool shown = word != 4294967295u;
    entry = shown ? word : 0;
    within = scrolled & 7;
    uint skyImage = RoomLevels[RoomSkyImageWord];
    if (skyImage != 0)
    {
        uint2 position = uint2(scrolled.x & 255, scrolled.y & 2047);
        uint cell = RoomLevels[skyImage + 1 + (position.y >> 4) * 16 + (position.x >> 4)];
        picture = cell != 0 ? cell : RoomLevels[skyImage];
        within = cell != 0 ? (position & 15) : position;
    }
    return shown;
}
// The BG3 plane (Layer3Plane): 32 by 64 words. A window of `rows` rows from `firstRow` repeats every 256 pixels across.
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
// A word written over BG2 at the tilemap word a `size` layout places the scrolled position (RoomTilemapOverlay), with
// `within` its pixel; where the overlay's pictures are resolved (#1392), `picture` is its word's picture and `within` the
// pixel in it.
bool Bg2OverlayWord(uint2 scrolled, uint2 size, out uint entry, out uint picture, out uint2 within)
{
    uint2 tile = (scrolled & (size * 8 - 1)) >> 3;
    uint index = ((tile.y >> 5) * (size.x >> 5) + (tile.x >> 5)) * 1024 + (tile.y & 31) * 32 + (tile.x & 31);
    entry = index < 2048 ? RoomLevels[RoomLevels[RoomBg2OverlayWord] + index] : 4294967295u;
    within = scrolled & 7;
    picture = 0;
    uint pictures = RoomLevels[RoomBg2OverlayPicturesWord];
    if (entry != 4294967295u && pictures != 0)
    {
        picture = RoomLevels[pictures + 2 * index];
        uint corner = RoomLevels[pictures + 2 * index + 1];
        if (picture != 0)
            within += uint2(corner & 65535, corner >> 16);
    }
    return entry != 4294967295u;
}
// A boss body's tilemap (BossBg2Tilemap) in the hardware's screen order, at the pass's map size. Every BG2 sampler
// reads the boss through this, so a color-math pass shows the same body as an ordinary one (#1426).
// Where the body's pictures are resolved (#1392), `picture` is its word's picture and `within` its pixel in it.
uint BossBg2Word(uint2 scrolled, uint2 size, out uint2 within, out uint picture)
{
    uint2 position = scrolled & 65535 & (size * 8 - 1);
    uint2 tile = position >> 3;
    uint page = ((tile.y >> 5) * (size.x >> 5) + (tile.x >> 5)) * 1024;
    uint index = page + (tile.y & 31) * 32 + (tile.x & 31);
    within = position & 7;
    picture = 0;
    uint pictures = RoomLevels[RoomBossBg2PicturesWord];
    if (pictures != 0)
    {
        picture = RoomLevels[pictures + 2 * index];
        uint corner = RoomLevels[pictures + 2 * index + 1];
        if (picture != 0)
            within += uint2(corner & 65535, corner >> 16);
    }
    return RoomLevels[RoomLevels[RoomBossBg2Word] + index];
}
// The X-ray reveal's block picture and pixel at a scrolled BG2 position (#1392); false past the blocks it builds, which
// BG2's scroll never shows.
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
    return true;
}
// The picture and pixel a room's library background shows at a scrolled BG2 position through a `size` map (#1392): a
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
// BG2 from the room's layer-2 plane or the sky's rows, or BG1 as RoomBackgrounds resolves it: a door's source
// screen, then words written over BG1, then the level plane.
bool RoomTile(uint source, uint2 scrolled, out uint entry, out uint2 within, out uint picture)
{
    entry = 0;
    picture = 0;
    within = uint2(0, 0);
    scrolled &= 65535;
    uint slot = source == RoomSourceBg2 ? 1 : 0;
    bool overlaid = false;
    if (source != RoomSourceBg2 && source != RoomSourceSky)
    {
        uint door = RoomLevels[RoomDoorWindowWord];
        uint2 doorWithin = (scrolled - uint2(door & 65535, door >> 16)) & 65535;
        uint overlay = RoomLevels[RoomOverlayWord];
        if (RoomLevels[RoomDoorSlot * RoomRecordWords + 1] != 0 && doorWithin.x < 256 && doorWithin.y < 192)
            slot = RoomDoorSlot;
        else if (overlay != 0)
        {
            uint2 cell = uint2((scrolled.x >> 3) & 63, (scrolled.y >> 3) & 31);
            uint index = (cell.x >> 5) * 1024 + cell.y * 32 + (cell.x & 31);
            uint written = RoomLevels[overlay + index];
            overlaid = written != 4294967295u;
            entry = written;
            within = scrolled & 7;
            // Where the overlay's pictures are resolved (#1392), its word draws from its picture.
            uint pictures = RoomLevels[RoomBg1OverlayPicturesWord];
            if (overlaid && pictures != 0)
            {
                picture = RoomLevels[pictures + 2 * index];
                uint corner = RoomLevels[pictures + 2 * index + 1];
                if (picture != 0)
                    within += uint2(corner & 65535, corner >> 16);
            }
        }
    }
    bool shown = overlaid;
    if (source == RoomSourceSky)
        shown = SkyTile(scrolled, entry, within, picture);
    else if (!overlaid)
        shown = RoomPlaneTile(slot, scrolled, entry, within, picture);
    return shown;
}
uint Pack(uint3 c, uint alpha) { return c.r | (c.g << 8) | (c.b << 16) | (alpha << 24); }
uint3 Unpack(uint c) { return uint3(c & 255, (c >> 8) & 255, (c >> 16) & 255); }
uint Palette(uint index)
{
    uint word = Memory[PaletteOffset + index];
    uint3 c = uint3(word & 31, (word >> 5) & 31, (word >> 10) & 31);
    return Pack((c << 3) | (c >> 2), 255);
}
// A block picture's pixel (#1392): its background palette index (0 transparent), its tile priority, and its colour,
// the picture's own while the pixel's palette row still holds its sheet's standard colours, else the row's live colour.
uint PicturePixel(uint picture, uint2 pixel, out uint index, out bool high)
{
    uint width = RoomLevels[picture + PictureWidthWord];
    uint at = pixel.y * (width & 65535) + pixel.x;
    uint meta = RoomLevels[picture + PicturePixelsWord + RoomLevels[picture + PictureCountWord] + at];
    uint full = meta & 255;
    high = (meta >> 8) != 0;
    // A two-bit picture (#1392) keeps colour zero's row: `index` is its colour, and colour zero shows that row's live
    // colour where the layer shows colour zero, as the characters did.
    bool twoBpp = (width >> 16) != 0;
    index = twoBpp ? (full & 3) : full;
    if (index == 0) return twoBpp ? Palette(full) : 0;
    uint standard = RoomLevels[picture + PictureColoursWord];
    uint row = full & ~15u;
    bool same = true;
    [loop] for (uint colour = 1; colour < 16; colour++)
        same = same && (Memory[PaletteOffset + row + colour] & 32767) == RoomLevels[standard + row + colour];
    return same ? RoomLevels[picture + PicturePixelsWord + at] : Palette(full);
}

#include "Objects.hlsli"
#include "Xray.hlsli"

// A frontend layer's cell at the scrolled position (#1403), packed row by row from room level word `TilemapWord`, drawn
// from the pre-coloured tile of its character in `table` (#1392): `picture` is the tile's sheet picture, and `within` the
// pixel in it under the cell's palette row, mirrored as the cell mirrors it.
uint ScreenTile(uint2 sampleScreen, uint2 scroll, uint table, out uint2 within, out uint picture)
{
    uint x = (sampleScreen.x + scroll.x) & (MapWidth * 8 - 1);
    uint y = (sampleScreen.y + scroll.y) & (MapHeight * 8 - 1);
    uint tileX = x >> 3, tileY = y >> 3;
    uint entry = RoomLevels[TilemapWord + tileY * MapWidth + tileX];
    uint at = table + (entry & 1023) * 3;
    picture = RoomLevels[at];
    uint corner = RoomLevels[at + 1];
    uint2 pixel = uint2((entry & 16384) != 0 ? 7 - (x & 7) : x & 7, (entry & 32768) != 0 ? 7 - (y & 7) : y & 7);
    within = pixel + uint2(corner & 65535, (corner >> 16) + ((entry >> 10) & 7) * RoomLevels[at + 2]);
    return entry;
}

bool MainCoverage(uint2 screen)
{
    uint4 geometry = uint4(CenterX, CenterY, OffsetX, OffsetY);
    if (geometry.w == 0) return true;
    uint4 map = uint4(MatrixA, MatrixB, MatrixC, MatrixD);
    uint2 position = (screen + map.zw) & (geometry.xy * 8 - 1);
    uint2 tile = position >> 3;
    // The coverage layer's cells, packed row by row from room level word map.x (#1403).
    uint entry = RoomLevels[map.x + tile.y * geometry.x + tile.x];
    if (geometry.z != 0 && ((entry >> 13) & 1) != geometry.z - 1) return false;
    uint px = (entry & 16384) != 0 ? 7 - (position.x & 7) : position.x & 7;
    uint py = (entry & 32768) != 0 ? 7 - (position.y & 7) : position.y & 7;
    // The coverage layer draws from its characters' pre-coloured tiles (#1392): an opaque pixel of its tile covers.
    uint at = CoverageTable + (entry & 1023) * 3;
    uint corner = RoomLevels[at + 1];
    uint index;
    bool ignored;
    bool covered = RoomLevels[at] != 0 && PicturePixel(RoomLevels[at], uint2(px + (corner & 65535),
        py + (corner >> 16) + ((entry >> 10) & 7) * RoomLevels[at + 2]), index, ignored) != 0 && index != 0;
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
    if (Operation == OpMessage)
    {
        // The message's image (#1392) from its top scanline: colour zero shows nothing; the two colours bank $85
        // installs show in their place; another colour shows the image's own where its row is standard, else the live one.
        uint picture = RoomLevels[RoomMessagePictureWord];
        uint width = RoomLevels[picture + PictureWidthWord] & 65535;
        uint height = RoomLevels[picture + PictureCountWord] / width;
        if (picture == 0 || id.y < TilemapWord || id.y >= TilemapWord + height) return;
        uint2 pixel = uint2(id.x, id.y - TilemapWord);
        uint full = RoomLevels[picture + PicturePixelsWord + RoomLevels[picture + PictureCountWord] + pixel.y * width + pixel.x] & 255;
        if ((full & 3) == 0) return;
        if (full == AddB || full == MessageDarkIndex)
        {
            uint word = full == AddB ? AddR : AddG;
            uint3 c = uint3(word & 31, (word >> 5) & 31, (word >> 10) & 31);
            Output[id.xy] = Pack((c << 3) | (c >> 2), 255);
        }
        else
        {
            uint index;
            bool high;
            Output[id.xy] = PicturePixel(picture, pixel, index, high);
        }
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
        bool transparentOutside = outside && FillCharacterZero == 0;
        uint wrappedX = (uint)x & 1023, wrappedY = (uint)y & 1023;
        // Policy 0: transparent overflow; 1: character zero; 2: ten-bit map wrap.
        uint cell = (wrappedY >> 3) * 128 + (wrappedX >> 3);
        // The floor's map lives in the room buffer, four tile numbers per word (#1403).
        uint mapped = (RoomLevels[RoomLevels[RoomMode7MapWord] + (cell >> 2)] >> ((cell & 3) * 8)) & 255;
        uint character = outside && FillCharacterZero == 1 ? 0 : mapped;
        // The floor draws its tiles' pre-coloured pictures (#1392), each tile's picture and corner in a table; `shown` is
        // the colour a nonzero pixel shows. A tile nothing pictured shows draws nothing.
        uint tiles = RoomLevels[RoomMode7PictureWord];
        uint color = 0, shown = 0;
        uint picture = RoomLevels[tiles + character * 2];
        if (picture != 0)
        {
            uint corner = RoomLevels[tiles + character * 2 + 1];
            uint index;
            bool high;
            shown = PicturePixel(picture, uint2((corner & 65535) + (wrappedX & 7), (corner >> 16) + (wrappedY & 7)), index, high);
            color = index;
        }
        if (transparentOutside) color = 0;
        if (Reserved27 == 2)
        {
            // TM=BG1|OBJ, TS=BG1: select the main winner before adding BG1.
            // Priority-zero OBJ is behind BG1; OBJ palettes zero through three
            // are never eligible for SNES color arithmetic.
            uint palette;
            uint2 obj = ResolveObjectWithPalette(id.xy, palette);
            bool objWins = obj.y != 255 && (obj.y != 0 || color == 0);
            uint main = objWins ? obj.x : color != 0 ? shown : Output[id.xy];
            if (color != 0 && (!objWins || palette >= 4))
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
    // BG2-only grouping happens before the current scanline's HDMA scroll.
    uint2 sampleScreen = id.xy;
    if (Operation == OpBg4 && Reserved27 > 1) sampleScreen -= sampleScreen % Reserved27;
    uint entry = 0;
    uint picture = 0;
    uint2 within = uint2(0, 0);
    uint2 scrolled = (sampleScreen + scroll) & 65535;
    uint source = RoomSource & 255;
    uint written = 0;
    if ((RoomSource & RoomSourceBg2Overlay) != 0 && Bg2OverlayWord(scrolled, uint2(MapWidth, MapHeight), written, picture, within))
        entry = written;
    else if (source == RoomSourceBg3)
    {
        if (!Bg3Picture(TilemapWord, MapHeight, scrolled, within, picture)) return;
    }
    else if (source == RoomSourceBossBg2)
        entry = BossBg2Word(scrolled, uint2(MapWidth, MapHeight), within, picture);
    else if (source == RoomSourceXrayReveal)
    {
        if (!RevealPicture(scrolled, picture, within)) return;
    }
    else if (source == RoomSourceBg2Image)
    {
        if (!Bg2ImagePicture(scrolled, uint2(MapWidth, MapHeight), picture, within)) return;
    }
    else if (source == RoomSourceScreenTiles)
    {
        entry = ScreenTile(sampleScreen, scroll, RoomSource >> 16, within, picture);
        // A word naming a character no artwork the screen loads shows draws nothing.
        if (picture == 0) return;
    }
    else if (source != 0)
    {
        if (!RoomTile(source, scrolled, entry, within, picture)) return;
    }
    else
    {
        uint x = (sampleScreen.x + scroll.x) & (MapWidth * 8 - 1);
        uint y = (sampleScreen.y + scroll.y) & (MapHeight * 8 - 1);
        uint tileX = x >> 3, tileY = y >> 3;
        uint page = ((tileY >> 5) * (MapWidth >> 5) + (tileX >> 5)) * 1024;
        entry = ReadWord(TilemapWord + page + (tileY & 31) * 32 + (tileX & 31));
        within = uint2(x & 7, y & 7);
    }
    bool fourBpp = Operation == OpBg4;
    uint color;
    bool high;
    uint sampled;
    if (picture != 0)
    {
        // A block picture keeps no row for its transparent pixels; room planes show colour zero as transparent.
        sampled = PicturePixel(picture, within, color, high);
        // A frontend layer's tile takes its word's priority (#1392).
        if (source == RoomSourceScreenTiles) high = (entry & 8192) != 0;
        if (PriorityFilter != 0 && (high ? 1u : 0u) != PriorityFilter - 1) return;
    }
    else
    {
        high = (entry & 8192) != 0;
        if (PriorityFilter != 0 && (high ? 1u : 0u) != PriorityFilter - 1) return;
        uint sourceX = (entry & 16384) != 0 ? 7 - within.x : within.x;
        uint sourceY = (entry & 32768) != 0 ? 7 - within.y : within.y;
        uint stride = fourBpp ? 16 : 8;
        uint row = ((CharacterWord + (entry & 1023) * stride) & 32767) * 2 + sourceY * 2;
        uint shift = 7 - sourceX;
        color = ((ReadByte(row) >> shift) & 1) | (((ReadByte(row + 1) >> shift) & 1) << 1);
        if (fourBpp)
            color |= (((ReadByte(row + 16) >> shift) & 1) << 2) | (((ReadByte(row + 17) >> shift) & 1) << 3);
        sampled = Palette(((entry >> 10) & 7) * (fourBpp ? 16 : 4) + color);
    }
    bool objectSubscreen = Operation == OpSubscreenAdd && AddR != 0;
    uint2 subObject = objectSubscreen ? Objects[id.xy] : uint2(0, 255);
    bool objectWins = objectSubscreen && subObject.y != 255 &&
        (color == 0 || subObject.y >= (high ? 3u : 2u));
    if (color == 0 && TransparentZero != 0 && !objectWins) return;
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
