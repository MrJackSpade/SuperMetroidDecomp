using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;
using System.Collections.Immutable;
using Vortice.Direct3D11;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Fx;
using System.Buffers.Binary;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frame's room backgrounds packed for Tiles.hlsl at register t3 (#1346): a header of three plane records (BG1, BG2
/// and a door's source room), the door window, the BG1 overlay, sky, BG3 and BG2 overlay offsets; then each plane's
/// blocks (#1403), each plane's picture of each definition and the pictures themselves with their sheets' standard
/// colours (#1392), the BG1 and BG2 overlays' pictures (#1348, #1403), the sky's rows (#1347), the BG3 plane's pictures,
/// a Mode 7 floor (#1349, #1403) and a boss body's pictures (#1352, #1403).
/// The GPU resolves every room pixel from these, so nothing is streamed into VRAM. Repacked only when the published
/// planes, their pictures, offsets, door or overlay change.
/// </summary>
internal sealed class D3D11RoomLevelBuffer : IDisposable
{
    /// <summary>
    /// Words per plane record: block offset, width, height, scroll offset X and Y, and the offset of the picture each
    /// definition shows.
    /// </summary>
    private const int RecordWords = 6;

    /// <summary>Words before a packed picture's pixels: its sheet's colours' offset, its width and its pixel count.</summary>
    private const int PictureHeaderWords = 3;
    /// <summary>Plane slots: BG1, BG2 and a door's source room.</summary>
    private const int PlaneSlots = 3;
    /// <summary>Header words: the plane records, the door window origin, and the offsets of the BG1 overlay, the sky rows,
    /// the BG3 plane and the BG2 overlay.</summary>
    private const int HeaderWords = PlaneSlots * RecordWords + 12;
    /// <summary>Header word of the door window's scrolled origin.</summary>
    private const int DoorWindowWord = PlaneSlots * RecordWords;
    /// <summary>
    /// Header word of the BG1 overlay's pictures (#1403), laid out as <see cref="BossBg2Word"/>'s with no picture
    /// where nothing was written, or zero without an overlay.
    /// </summary>
    private const int OverlayWord = DoorWindowWord + 1;
    /// <summary>Header word of the sky's first row word, or zero without a sky (#1347).</summary>
    private const int SkyWord = OverlayWord + 1;
    /// <summary>
    /// Header word of the BG3 plane's pictures (#1403), laid out as <see cref="BossBg2Word"/>'s, or zero without
    /// a plane.
    /// </summary>
    private const int Bg3Word = SkyWord + 1;
    /// <summary>Header word of the BG2 overlay's pictures, laid out as <see cref="OverlayWord"/>'s, or zero without one (#1348).</summary>
    private const int Bg2OverlayWord = Bg3Word + 1;
    /// <summary>Header word of the Mode 7 map's first word, or zero without one (#1349); four tile numbers per word.</summary>
    private const int Mode7MapWord = Bg2OverlayWord + 1;
    /// <summary>
    /// Header word of a boss body's pictures (#1352, #1403), or zero without one: two words per tilemap cell, its picture's
    /// offset, then its corner in the picture, x below y.
    /// </summary>
    private const int BossBg2Word = Mode7MapWord + 1;
    /// <summary>
    /// Header word of the X-ray reveal's first block, or zero without one (#1392): each block its picture's offset, with its
    /// arrangement in the top four bits.
    /// </summary>
    private const int XrayRevealWord = BossBg2Word + 1;
    /// <summary>
    /// Header word of a library background's image (#1392), or zero without one: the image picture's offset, then each
    /// 16x16 cell's picture offset, zero where the image shows.
    /// </summary>
    private const int Bg2ImageWord = XrayRevealWord + 1;
    /// <summary>Header word of the scrolling sky's image, laid out as <see cref="Bg2ImageWord"/>'s, or zero without one (#1392).</summary>
    private const int SkyImageWord = Bg2ImageWord + 1;
    /// <summary>Header word of the image of the message box the frame shows, or zero without one (#1392).</summary>
    private const int MessagePictureWord = SkyImageWord + 1;
    /// <summary>Header word of the Ceres room Mode 7 map's tile picture table, or zero while it draws from VRAM's characters (#1392).</summary>
    private const int Mode7PictureWord = MessagePictureWord + 1;
    /// <summary>The width word's bit marking a two-bit picture, whose pixels keep colour zero's row (#1392).</summary>
    private const uint TwoBppPictureBit = 1u << 16;
    /// <summary>The bit an X-ray reveal block's arrangement starts at.</summary>
    private const int ArrangementShift = 28;
    /// <summary>A sky row word that selects no page: the row shows nothing.</summary>
    private const uint SkyRowHidden = uint.MaxValue;
    /// <summary>Words of a frontend layer's character tile table: three for each character index (#1392).</summary>
    private const int ScreenTableWords = 3 * ScreenLayerPictures.CharacterIndices;
    /// <summary>Words of a Mode 7 map's tile picture table: two for each tile (#1392).</summary>
    private const int Mode7TableWords = 2 * Mode7TilePictures.Tiles;
    /// <summary>Initial buffer size; it grows to fit larger rooms.</summary>
    private const int InitialWords = 4096;

    /// <summary>The device the buffer lives on.</summary>
    private readonly ID3D11Device device;
    /// <summary>What the last <see cref="Pack"/> packed, so an unchanged frame skips the upload.</summary>
    private RoomLevelLayers? packed;
    /// <summary>Where the last <see cref="Pack"/> put each frontend layer's character tile table.</summary>
    private readonly Dictionary<ScreenLayerPictures, int> screenTables = [];
    /// <summary>Where the last <see cref="Pack"/> put each Mode 7 map's tile picture table.</summary>
    private readonly Dictionary<Mode7TilePictures, int> mode7Tables = [];
    /// <summary>Where the last <see cref="Pack"/> put each frontend layer's cells (#1403).</summary>
    private readonly Dictionary<ScreenTileMap, int> screenMaps = [];

    /// <summary>Creates an empty buffer.</summary>
    internal D3D11RoomLevelBuffer(ID3D11Device device)
    {
        this.device = Ensure.NotNull(device);
        Create(InitialWords);
    }

    /// <summary>The packed words of the last <see cref="Pack"/>.</summary>
    internal uint[] Upload { get; private set; }
    /// <summary>The structured buffer.</summary>
    internal ID3D11Buffer Buffer { get; private set; }
    /// <summary>The shader view bound at t3.</summary>
    internal ID3D11ShaderResourceView View { get; private set; }

    /// <summary>Packs the frame's room planes; false when they match the last packing and need no upload.</summary>
    /// <param name="frame">The planes, overlays, BG3, Mode 7 map, boss tilemap and X-ray reveal the frame shows.</param>
    internal bool Pack(RoomLevelLayers frame)
    {
        RoomBackgroundLayer? bg1 = frame.Bg1, bg2 = frame.Bg2;
        RoomDoorBackground? door = frame.Door;
        RoomOverlayPictures? overlay = frame.Overlay, bg2Overlay = frame.Bg2Overlay;
        SkyTilemap? sky = frame.Sky;
        Layer3Cells? bg3 = frame.Bg3;
        AffineFloor? mode7 = frame.Mode7;
        BossBodyPictures? bossBg2 = frame.BossBg2;
        XrayRevealPlane? reveal = frame.XrayReveal;
        RoomBackgroundImagePlane? image = frame.Bg2Image, skyImage = frame.SkyImage;
        SpriteSheetFrame? message = frame.MessagePicture;
        ImmutableArray<ScreenLayerPictures> screens = frame.Screens?.Layers ?? [];
        ImmutableArray<ScreenTileMap> screenCells = frame.Screens?.Maps ?? [];
        ImmutableArray<Mode7TilePictures> mode7s = mode7 is null ? [] : [mode7.Tiles];
        // No dispatch samples t3 without one of these, so such a scene keeps the last packed room unread.
        if (packed == frame || (bg1 is null && bg2 is null && sky is null && bg3 is null && mode7 is null && reveal is null && image is null
            && skyImage is null && message is null && screens.IsEmpty && mode7s.IsEmpty))
            return false;
        RoomBackgroundLayer?[] planes = [bg1, bg2, door?.SourceRoom];
        int words = HeaderWords;
        // Each frontend layer's character tile table comes first, so its word fits the half a dispatch names it in (#1392).
        screenTables.Clear();
        foreach (ScreenLayerPictures screen in screens)
        {
            if (screenTables.TryAdd(screen, words))
                words += ScreenTableWords;
        }
        mode7Tables.Clear();
        foreach (Mode7TilePictures tiles in mode7s)
        {
            if (mode7Tables.TryAdd(tiles, words))
                words += Mode7TableWords;
        }
        if (words > ushort.MaxValue)
            throw new InvalidOperationException($"A frame's {screenTables.Count} frontend layer tile tables do not fit the half word a dispatch names them in.");
        screenMaps.Clear();
        foreach (ScreenTileMap map in screenCells)
        {
            if (screenMaps.TryAdd(map, words))
                words += map.Cells.Length;
        }
        var blockOffsets = new int[PlaneSlots];
        for (int slot = 0; slot < PlaneSlots; slot++)
        {
            if (planes[slot] is not { } layer)
                continue;
            blockOffsets[slot] = words;
            words += layer.Blocks.Cells.Length;
        }
        // Each plane's picture of each definition, then each distinct picture and sheet palette once (#1392).
        var pictureTables = new int[PlaneSlots];
        for (int slot = 0; slot < PlaneSlots; slot++)
        {
            if (planes[slot] is not { } layer)
                continue;
            RoomBlockPictureTable table = layer.Pictures ?? throw new InvalidOperationException("A room plane is drawn once its pictures are resolved.");
            pictureTables[slot] = words;
            words += table.Definitions;
        }
        var pictureWords = new Dictionary<SpriteSheetFrame, int>();
        var colourWords = new Dictionary<SpriteImage, int>();
        int revealOffset = reveal is null ? 0 : words;
        if (reveal is not null)
            words += reveal.Blocks.Length;
        int imageOffset = image is null ? 0 : words;
        if (image is not null)
            words += 1 + image.Image.CellCount;
        int skyImageOffset = skyImage is null ? 0 : words;
        if (skyImage is not null)
            words += 1 + skyImage.Image.CellCount;
        void AddPicture(SpriteSheetFrame picture)
        {
            if (colourWords.TryAdd(picture.Sheet, words))
                words += picture.Sheet.StandardColours.Length;
            if (pictureWords.TryAdd(picture, words))
                words += PictureHeaderWords + 2 * picture.Size.X * picture.Size.Y;
        }
        foreach (RoomBlockPictureTable? table in planes.Select(layer => layer?.Pictures).Append(reveal?.Pictures))
        {
            if (table is null)
                continue;
            for (int definition = 0; definition < table.Definitions; definition++)
            {
                if (table.Picture(definition) is { } picture)
                    AddPicture(picture);
            }
        }
        foreach (RoomBackgroundImagePlane? plane in new[] { image, skyImage })
        {
            if (plane is null)
                continue;
            AddPicture(plane.Image.Picture);
            for (int cell = 0; cell < plane.Image.CellCount; cell++)
            {
                if (plane.Cell(cell) is { } picture)
                    AddPicture(picture);
            }
        }
        if (message is not null)
            AddPicture(message);
        foreach (ScreenLayerPictures screen in screenTables.Keys)
        {
            foreach (ScreenTileSheet sheet in screen.Sheets)
                AddPicture(sheet.Picture);
        }
        foreach (Mode7TilePictures tiles in mode7Tables.Keys)
        {
            foreach (SpriteSheetFrame picture in tiles.Sheets)
                AddPicture(picture);
        }
        ImmutableArray<BgTilePicture?> bodyPictures = bossBg2 is null ? default : [.. bossBg2.Pictures.Select(picture => (BgTilePicture?)picture)];
        int bossBg2Offset = AddTilePictures(bodyPictures, bodyPictures.IsDefault ? 0 : bodyPictures.Length);
        ImmutableArray<BgTilePicture?> bg2OverlayPictures = bg2Overlay?.Pictures ?? default;
        int bg2OverlayOffset = AddTilePictures(bg2OverlayPictures, RoomTilemapOverlay.WordCount);
        ImmutableArray<BgTilePicture?> overlayPictures = overlay?.Pictures ?? default;
        int overlayOffset = AddTilePictures(overlayPictures, RoomTilemapOverlay.WordCount);
        ImmutableArray<BgTilePicture?> bg3Pictures = bg3 is null ? default : [.. bg3.Pictures.Select(picture => (BgTilePicture?)picture)];
        int bg3PicturesOffset = AddTilePictures(bg3Pictures, Layer3Plane.Rows * Layer3Plane.Columns);
        // Reserves two words for each of `count` tile pictures and adds each picture; zero without pictures.
        int AddTilePictures(ImmutableArray<BgTilePicture?> pictures, int count)
        {
            if (pictures.IsDefault)
                return 0;
            int offset = words;
            words += 2 * count;
            for (int word = 0; word < count; word++)
            {
                if (pictures[word] is { } tile)
                    AddPicture(tile.Picture);
            }
            return offset;
        }
        int skyOffset = sky is null ? 0 : words;
        if (sky is not null)
            words += SkyTilemap.Rows * SkyTilemap.Columns;
        int mode7Offset = mode7 is null ? 0 : words;
        if (mode7 is not null)
            words += AffineFloor.CellCount / sizeof(uint);
        if (words > Upload.Length)
        {
            Dispose();
            Create(Math.Max(words, Upload.Length * 2));
        }

        Span<uint> upload = Upload;
        upload.Clear();
        for (int slot = 0; slot < PlaneSlots; slot++)
        {
            if (planes[slot] is not { } layer)
                continue;
            int record = slot * RecordWords;
            RoomBlockCells blocks = layer.Blocks;
            upload[record] = (uint)blockOffsets[slot];
            upload[record + 1] = blocks.Size.X;
            upload[record + 2] = blocks.Size.Y;
            upload[record + 3] = unchecked((ushort)layer.ScrollOffset.X);
            upload[record + 4] = unchecked((ushort)layer.ScrollOffset.Y);
            upload[record + 5] = (uint)pictureTables[slot];
            RoomBlockPictureTable table = layer.Pictures ?? throw new InvalidOperationException("A room plane is drawn once its pictures are resolved.");
            for (int definition = 0; definition < table.Definitions; definition++)
                upload[pictureTables[slot] + definition] = table.Picture(definition) is { } shown ? (uint)pictureWords[shown] : 0;
            // Each block's definition, with its horizontal and vertical mirroring in bits ten and eleven, as Tiles.hlsl reads a block.
            ImmutableArray<RoomBlockCell> cells = blocks.Cells;
            for (int block = 0; block < cells.Length; block++)
                upload[blockOffsets[slot] + block] = (uint)cells[block].Definition
                    | ((cells[block].Flips & LevelBlockFlipFlags.Horizontal) != 0 ? 1u << 10 : 0)
                    | ((cells[block].Flips & LevelBlockFlipFlags.Vertical) != 0 ? 1u << 11 : 0);
        }
        foreach ((SpriteImage sheet, int offset) in colourWords)
            for (int colour = 0; colour < sheet.StandardColours.Length; colour++)
                upload[offset + colour] = sheet.StandardColours[colour].ToWord();
        foreach ((SpriteSheetFrame picture, int offset) in pictureWords)
            PackPicture(upload.Slice(offset, PictureHeaderWords + 2 * picture.Size.X * picture.Size.Y), picture, colourWords[picture.Sheet]);
        foreach ((ScreenLayerPictures screen, int offset) in screenTables)
            PackScreenTable(upload.Slice(offset, ScreenTableWords), screen, pictureWords);
        foreach ((ScreenTileMap map, int offset) in screenMaps)
            PackScreenMap(upload.Slice(offset, map.Cells.Length), map);
        foreach ((Mode7TilePictures tiles, int offset) in mode7Tables)
            PackTilePictures(upload, tiles.Pictures, offset, Mode7TilePictures.Tiles, pictureWords);
        upload[Bg2ImageWord] = (uint)imageOffset;
        upload[SkyImageWord] = (uint)skyImageOffset;
        foreach ((RoomBackgroundImagePlane? plane, int offset) in new[] { (image, imageOffset), (skyImage, skyImageOffset) })
        {
            if (plane is null)
                continue;
            upload[offset] = (uint)pictureWords[plane.Image.Picture];
            for (int cell = 0; cell < plane.Image.CellCount; cell++)
                upload[offset + 1 + cell] = plane.Cell(cell) is { } picture ? (uint)pictureWords[picture] : 0;
        }
        if (door is not null)
            upload[DoorWindowWord] = D3D11ConstantPacking.Halves(door.WindowOrigin.X, door.WindowOrigin.Y);
        upload[OverlayWord] = (uint)overlayOffset;
        PackTilePictures(upload, overlayPictures, overlayOffset, RoomTilemapOverlay.WordCount, pictureWords);
        upload[SkyWord] = (uint)skyOffset;
        if (sky is not null)
            for (int word = 0; word < SkyTilemap.Rows * SkyTilemap.Columns; word++)
                upload[skyOffset + word] = sky.RowShown(word / SkyTilemap.Columns) ? sky.Words[word] : SkyRowHidden;
        upload[Bg2OverlayWord] = (uint)bg2OverlayOffset;
        PackTilePictures(upload, bg2OverlayPictures, bg2OverlayOffset, RoomTilemapOverlay.WordCount, pictureWords);
        upload[Mode7MapWord] = (uint)mode7Offset;
        if (mode7 is not null)
        {
            // Four tile numbers to a word, the first in the low byte.
            ImmutableArray<AffineFloorCell> cells = mode7.Cells;
            for (int word = 0; word < AffineFloor.CellCount / sizeof(uint); word++)
                upload[mode7Offset + word] = (uint)cells[word * 4].Tile | (uint)cells[word * 4 + 1].Tile << 8
                    | (uint)cells[word * 4 + 2].Tile << 16 | (uint)cells[word * 4 + 3].Tile << 24;
        }
        upload[XrayRevealWord] = (uint)revealOffset;
        if (reveal is not null)
        {
            ReadOnlySpan<RevealedBlock> blocks = reveal.Blocks;
            for (int block = 0; block < blocks.Length; block++)
            {
                SpriteSheetFrame picture = reveal.Pictures.Picture(blocks[block].Definition)
                    ?? throw new InvalidOperationException($"The X-ray reveal's block definition {blocks[block].Definition} has no picture.");
                upload[revealOffset + block] = (uint)pictureWords[picture] | ShaderArrangement(blocks[block].Arrangement) << ArrangementShift;
            }
        }
        upload[BossBg2Word] = (uint)bossBg2Offset;
        PackTilePictures(upload, bodyPictures, bossBg2Offset, bodyPictures.IsDefault ? 0 : bodyPictures.Length, pictureWords);
        upload[Bg3Word] = (uint)bg3PicturesOffset;
        upload[MessagePictureWord] = message is null ? 0 : (uint)pictureWords[message];
        upload[Mode7PictureWord] = mode7 is null ? 0 : (uint)mode7Tables[mode7.Tiles];
        PackTilePictures(upload, bg3Pictures, bg3PicturesOffset, Layer3Plane.Rows * Layer3Plane.Columns, pictureWords);
        packed = frame;
        return true;
    }

    /// <summary>
    /// Packs <paramref name="count"/> tile pictures from <paramref name="offset"/>: each its picture's offset, zero where the
    /// word draws from its character, then its corner in the picture, x below y; nothing without pictures.
    /// </summary>
    private static void PackTilePictures(Span<uint> upload, ImmutableArray<BgTilePicture?> pictures, int offset, int count,
        Dictionary<SpriteSheetFrame, int> pictureWords)
    {
        if (pictures.IsDefault)
            return;
        for (int word = 0; word < count; word++)
        {
            if (pictures[word] is not { } tile)
                continue;
            upload[offset + 2 * word] = (uint)pictureWords[tile.Picture];
            upload[offset + 2 * word + 1] = D3D11ConstantPacking.Halves(tile.Corner.X, tile.Corner.Y);
        }
    }

    /// <summary>The room level buffer word of <paramref name="map"/>'s first cell in the last packing (#1403).</summary>
    /// <exception cref="InvalidOperationException">The last packing held no such map.</exception>
    internal uint ScreenMap(ScreenTileMap map) => screenMaps.TryGetValue(map, out int offset) ? (uint)offset
        : throw new InvalidOperationException("A frontend layer's cells were not packed with its frame.");

    /// <summary>
    /// Packs a frontend layer's cells (#1403), one word each, row by row: the cell's tile in the low ten bits, its palette
    /// row above them, then its priority and its horizontal and vertical mirroring, as Tiles.hlsl decodes a cell.
    /// </summary>
    private static void PackScreenMap(Span<uint> words, ScreenTileMap map)
    {
        const int paletteRowShift = 10;
        const uint highPriority = 1u << 13, flipHorizontally = 1u << 14, flipVertically = 1u << 15;
        for (int index = 0; index < map.Cells.Length; index++)
        {
            ScreenTileCell cell = map.Cells[index];
            words[index] = (uint)cell.Tile | (uint)cell.PaletteRow << paletteRowShift | (cell.HighPriority ? highPriority : 0)
                | (cell.FlipHorizontally ? flipHorizontally : 0) | (cell.FlipVertically ? flipVertically : 0);
        }
    }

    /// <summary>The room level buffer word of <paramref name="pictures"/>' character tile table in the last packing (#1392).</summary>
    /// <exception cref="InvalidOperationException">The last packing held no such layer.</exception>
    internal uint ScreenTable(ScreenLayerPictures pictures) => screenTables.TryGetValue(pictures, out int offset) ? (uint)offset
        : throw new InvalidOperationException("A frontend layer's character tiles were not packed with its frame.");

    /// <summary>
    /// Packs a frontend layer's character tile table (#1392): for each character index, its tile's picture, zero where no
    /// artwork shows the character, the corner of its palette row zero tile, x below y, and the pixels from one palette
    /// row's block to the next.
    /// </summary>
    private static void PackScreenTable(Span<uint> table, ScreenLayerPictures screen, Dictionary<SpriteSheetFrame, int> pictureWords)
    {
        for (int index = 0; index < ScreenLayerPictures.CharacterIndices; index++)
        {
            if (screen.Tiles[index] is not { } tile)
                continue;
            Position16<ImagePixels> corner = tile.Sheet.Corner(tile.Character, 0);
            table[3 * index] = (uint)pictureWords[tile.Sheet.Picture];
            table[3 * index + 1] = D3D11ConstantPacking.Halves(corner.X, corner.Y);
            table[3 * index + 2] = (uint)tile.Sheet.RowStride;
        }
    }

    /// <summary>
    /// Packs one picture, a block or a background image: its sheet's colours' offset, its width and pixel count, then each
    /// pixel's colour as Tiles.hlsl packs one, then each pixel's palette index with its tile priority above it.
    /// </summary>
    private static void PackPicture(Span<uint> words, SpriteSheetFrame picture, int colours)
    {
        SpriteImage sheet = picture.Sheet;
        int width = picture.Size.X, pixels = width * picture.Size.Y;
        words[0] = (uint)colours;
        words[1] = (uint)width | (sheet.Colours == SpriteImageColours.TwoBppBackground ? TwoBppPictureBit : 0);
        words[2] = (uint)pixels;
        for (int y = 0; y < picture.Size.Y; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixel = (picture.Corner.Y + y) * sheet.Size.X + picture.Corner.X + x;
                Rgba32 colour = sheet.Pixels[pixel];
                int at = y * width + x;
                words[PictureHeaderWords + at] = colour.R | (uint)colour.G << 8 | (uint)colour.B << 16 | (uint)colour.A << 24;
                words[PictureHeaderWords + pixels + at] = sheet.PaletteIndices[pixel] | (uint)sheet.Priorities[pixel] << 8;
            }
        }
    }

    /// <summary>The code Tiles.hlsl's RevealPicture reads for a revealed block's arrangement.</summary>
    private static uint ShaderArrangement(RevealedBlockArrangement arrangement) => arrangement switch
    {
        RevealedBlockArrangement.AsDefined => 0,
        RevealedBlockArrangement.MirroredHorizontally => 1,
        RevealedBlockArrangement.MirroredVertically => 2,
        RevealedBlockArrangement.MirroredBoth => 3,
        RevealedBlockArrangement.RowsSwapped => 4,
        _ => throw new InvalidDataException($"Unknown revealed block arrangement {arrangement}."),
    };

    /// <summary>Creates the buffer and its view at <paramref name="words"/> words.</summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(Buffer), nameof(View), nameof(Upload))]
    private void Create(int words)
    {
        Buffer = device.CreateBuffer(new BufferDescription(checked((uint)(words * sizeof(uint))), BindFlags.ShaderResource,
            ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, sizeof(uint)));
        View = device.CreateShaderResourceView(Buffer);
        Upload = new uint[words];
    }

    /// <summary>Releases the buffer and its view.</summary>
    public void Dispose()
    {
        View.Dispose();
        Buffer.Dispose();
    }
}
