using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;
using System.Collections.Immutable;
using Vortice.Direct3D11;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frame's room backgrounds packed for Tiles.hlsl at register t3 (#1346): a header of three plane records (layers 1
/// and 2 and a door's source room), the door window, the layer-1 overlay, sky, layer-3 and layer-2 overlay offsets; then each plane's
/// blocks (#1403), each plane's picture of each block and the pictures themselves with their sheets' standard
/// colours (#1392), the layer-1 and layer-2 overlays' pictures (#1348, #1403), the sky's rows (#1347), the layer-3 plane's pictures,
/// a Mode 7 floor (#1349, #1403), a boss body's parts (#1352, #1466) and the HUD's placed pictures (#1466).
/// The GPU resolves every room pixel from these and the image texture. Repacked only when the published
/// planes, their pictures, offsets, door or overlay change.
/// </summary>
internal sealed class D3D11RoomLevelBuffer : IDisposable
{
    /// <summary>
    /// Words per plane record: block offset, width, height, scroll offset X and Y, and the offset of the picture each
    /// block shows.
    /// </summary>
    private const int RecordWords = 6;

    /// <summary>Words of a packed picture: its colour slot, its place in the image texture and its width and height.</summary>
    private const int PictureHeaderWords = 3;
    /// <summary>Plane slots: layers 1 and 2 and a door's source room.</summary>
    private const int PlaneSlots = 3;
    /// <summary>Header words: the plane records, the door window origin, and the offsets of the layer-1 overlay, the sky rows,
    /// the layer-3 plane, the layer-2 overlay and the other tables.</summary>
    private const int HeaderWords = PlaneSlots * RecordWords + 12;
    /// <summary>Header word of the door window's scrolled origin.</summary>
    private const int DoorWindowWord = PlaneSlots * RecordWords;
    /// <summary>
    /// Header word of the layer-1 overlay's pictures (#1403), or zero without an overlay: two words per tilemap cell, its
    /// picture's offset, zero where nothing was written, then its corner in the picture, x below y.
    /// </summary>
    private const int OverlayWord = DoorWindowWord + 1;
    /// <summary>Header word of the sky's rows, one word each, nonzero where a row selects a page, or zero without a sky (#1347, #1403).</summary>
    private const int SkyWord = OverlayWord + 1;
    /// <summary>
    /// Header word of the layer-3 plane's pictures (#1403), laid out as <see cref="OverlayWord"/>'s, or zero without
    /// a plane.
    /// </summary>
    private const int Bg3Word = SkyWord + 1;
    /// <summary>Header word of the layer-2 overlay's pictures, laid out as <see cref="OverlayWord"/>'s, or zero without one (#1348).</summary>
    private const int Bg2OverlayWord = Bg3Word + 1;
    /// <summary>
    /// Header word of the perspective floor's sheets' colour slots, one word each in the order the floor image numbers
    /// its sheets, or zero without a floor (#1408).
    /// </summary>
    private const int FloorSheetsWord = Bg2OverlayWord + 1;
    /// <summary>
    /// Header word of a boss body's parts (#1352, #1466), or zero without one: the count of parts, then four words each (its
    /// picture's offset, the layer row its picture's top edge lies on, its first row shown with the rows it spans in the
    /// high half, and the offset of its row masks), then each part's row masks, a bit per cell it shows.
    /// </summary>
    private const int BossBodyWord = FloorSheetsWord + 1;
    /// <summary>Words of a boss body part's record.</summary>
    private const int BossBodyPartWords = 4;
    /// <summary>
    /// Header word of the X-ray reveal's first block, or zero without one (#1392): each block its picture's offset, with its
    /// arrangement in the top four bits.
    /// </summary>
    private const int XrayRevealWord = BossBodyWord + 1;
    /// <summary>
    /// Header word of a library background's image (#1392), or zero without one: the image picture's offset, then each
    /// 16x16 cell's picture offset, zero where the image shows.
    /// </summary>
    private const int Bg2ImageWord = XrayRevealWord + 1;
    /// <summary>Header word of the scrolling sky's image, laid out as <see cref="Bg2ImageWord"/>'s, or zero without one (#1392).</summary>
    private const int SkyImageWord = Bg2ImageWord + 1;
    /// <summary>Header word of the image of the message box the frame shows, or zero without one (#1392).</summary>
    private const int MessagePictureWord = SkyImageWord + 1;
    /// <summary>
    /// Header word of the HUD's placed pictures (#1466), or zero without a HUD: the count of pictures, then two words each in
    /// drawing order, its picture's offset and its upper-left pixel on the screen, x below y.
    /// </summary>
    private const int HudWord = MessagePictureWord + 1;
    /// <summary>Words of a placed HUD picture's record.</summary>
    private const int HudPictureWords = 2;
    /// <summary>The bit an X-ray reveal block's arrangement starts at.</summary>
    private const int ArrangementShift = 28;
    /// <summary>Words of a frontend layer's tile table: three for each tile its cells name (#1392).</summary>
    private const int ScreenTableWords = 3 * ScreenLayerPictures.CharacterIndices;
    /// <summary>Initial buffer size; it grows to fit larger rooms.</summary>
    private const int InitialWords = 4096;

    /// <summary>The device the buffer lives on.</summary>
    private readonly ID3D11Device device;
    /// <summary>What the last <see cref="Pack"/> packed, so an unchanged frame skips the upload.</summary>
    private RoomLevelLayers? packed;
    /// <summary>The image texture's generation the last <see cref="Pack"/> placed pictures in.</summary>
    private int packedGeneration = -1;
    /// <summary>What the last <see cref="Collect"/> enumerated.</summary>
    private RoomLevelLayers? collected;
    /// <summary>The pictures the last <see cref="Collect"/> enumerated, so an unchanged frame names them without a walk.</summary>
    private readonly List<SpriteSheetFrame> collectedPictures = [];
    /// <summary>Where the last <see cref="Pack"/> put each frontend layer's tile table.</summary>
    private readonly Dictionary<ScreenLayerPictures, int> screenTables = [];
    /// <summary>The colour slot of each image and how it is coloured, in slot order, as the last <see cref="Pack"/> numbered them.</summary>
    private readonly List<D3D11ColourSlot> colourSlots = [];
    /// <summary>Where the last <see cref="Pack"/> put each frontend layer's cells (#1403).</summary>
    private readonly Dictionary<ScreenTileMap, int> screenMaps = [];
    /// <summary>
    /// Where the last <see cref="Pack"/> put each picture layer's placed pictures (#1467): their count, then two words each
    /// in drawing order, the picture's offset and its top-left layer pixel, x below y.
    /// </summary>
    private readonly Dictionary<PlacedPictures, int> placedTables = [];
    /// <summary>
    /// Where the last <see cref="Pack"/> put each picture grid (#1467): its width and height in cells, x below y, then one
    /// word per cell row by row, its one-cell picture's offset (zero for none) with its horizontal and vertical mirroring in
    /// bits 30 and 31.
    /// </summary>
    private readonly Dictionary<PictureGrid, int> gridTables = [];
    /// <summary>The bit of a packed grid cell marking horizontal mirroring.</summary>
    private const uint GridFlipHorizontally = 1u << 30;
    /// <summary>The bit of a packed grid cell marking vertical mirroring.</summary>
    private const uint GridFlipVertically = 1u << 31;

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

    /// <summary>
    /// Adds every picture <paramref name="frame"/> draws to <paramref name="needed"/>, so the image texture can hold them
    /// with the frame's sprites before <see cref="Pack"/> places them.
    /// </summary>
    internal void Collect(RoomLevelLayers frame, ISet<SpriteSheetFrame> needed)
    {
        if (collected != frame)
        {
            collectedPictures.Clear();
            foreach ((SpriteSheetFrame picture, _) in Pictures(frame))
                collectedPictures.Add(picture);
            collected = frame;
        }
        foreach (SpriteSheetFrame picture in collectedPictures)
            needed.Add(picture);
    }

    /// <summary>
    /// Packs the frame's room planes against the image texture's places; false when they match the last packing and the
    /// texture has kept every picture where it was.
    /// </summary>
    /// <param name="frame">The planes, overlays, layer 3, floor, boss body, X-ray reveal and HUD the frame shows.</param>
    /// <param name="images">The image texture, holding every picture <see cref="Collect"/> named.</param>
    internal bool Pack(RoomLevelLayers frame, D3D11ImageAtlas images)
    {
        RoomBackgroundLayer? bg1 = frame.Bg1, bg2 = frame.Bg2;
        RoomDoorBackground? door = frame.Door;
        RoomOverlayPictures? overlay = frame.Overlay, bg2Overlay = frame.Bg2Overlay;
        SkyPictures? sky = frame.Sky;
        Layer3Cells? bg3 = frame.Bg3;
        AffineFloor? mode7 = frame.Floor;
        ShownBossBody? bossBody = frame.BossBody;
        XrayRevealPlane? reveal = frame.XrayReveal;
        RoomBackgroundImagePlane? image = frame.Bg2Image, skyImage = sky?.Image;
        SpriteSheetFrame? message = frame.MessagePicture;
        PicturedHud? hud = frame.Hud;
        ImmutableArray<ScreenLayerPictures> screens = frame.Screens?.Layers ?? [];
        ImmutableArray<ScreenTileMap> screenCells = frame.Screens?.Maps ?? [];
        ImmutableArray<PlacedPictures> placedLayers = frame.Pictures?.Placed ?? [];
        ImmutableArray<PictureGrid> grids = frame.Pictures?.Grids ?? [];
        if (packed == frame && packedGeneration == images.Generation)
            return false;
        // No dispatch samples t3 without one of these, so such a scene keeps the last packed room unread. It colours no
        // picture either, so the last room's colour slots go: the scene's colours are packed for none of its images, and a
        // later frame showing that room again packs it afresh.
        if (bg1 is null && bg2 is null && sky is null && bg3 is null && mode7 is null && reveal is null && image is null
            && message is null && screens.IsEmpty && hud is null && placedLayers.IsEmpty && grids.IsEmpty)
        {
            colourSlots.Clear();
            packed = null;
            return false;
        }
        RoomBackgroundLayer?[] planes = [bg1, bg2, door?.SourceRoom];
        int words = HeaderWords;
        // Each frontend layer's tile table comes first, so its word fits the half a dispatch names it in (#1392).
        screenTables.Clear();
        foreach (ScreenLayerPictures screen in screens)
        {
            if (screenTables.TryAdd(screen, words))
                words += ScreenTableWords;
        }
        if (words > ushort.MaxValue)
            throw new InvalidOperationException($"A frame's {screenTables.Count} frontend layer tile tables do not fit the half word a dispatch names them in.");
        screenMaps.Clear();
        foreach (ScreenTileMap map in screenCells)
        {
            if (screenMaps.TryAdd(map, words))
                words += map.Cells.Length;
        }
        placedTables.Clear();
        foreach (PlacedPictures layer in placedLayers)
        {
            if (placedTables.TryAdd(layer, words))
                words += 1 + layer.Pictures.Length * HudPictureWords;
        }
        gridTables.Clear();
        foreach (PictureGrid grid in grids)
        {
            if (gridTables.TryAdd(grid, words))
                words += 1 + grid.Cells.Length;
        }
        var blockOffsets = new int[PlaneSlots];
        for (int slot = 0; slot < PlaneSlots; slot++)
        {
            if (planes[slot] is not { } layer)
                continue;
            blockOffsets[slot] = words;
            words += layer.Blocks.Cells.Length;
        }
        // Each plane's picture of each block (#1392, #1431).
        var pictureTables = new int[PlaneSlots];
        for (int slot = 0; slot < PlaneSlots; slot++)
        {
            if (planes[slot] is not { } layer)
                continue;
            RoomBlockPictureTable table = layer.Pictures ?? throw new InvalidOperationException("A room plane is drawn once its pictures are resolved.");
            pictureTables[slot] = words;
            words += table.Blocks;
        }
        int revealOffset = reveal is null ? 0 : words;
        if (reveal is not null)
            words += reveal.Blocks.Length;
        int imageOffset = image is null ? 0 : words;
        if (image is not null)
            words += 1 + image.CellCount;
        int skyImageOffset = skyImage is null ? 0 : words;
        if (skyImage is not null)
            words += 1 + skyImage.CellCount;
        int hudOffset = hud is null ? 0 : words;
        if (hud is not null)
            words += 1 + hud.Pictures.Length * HudPictureWords;
        int bossBodyOffset = bossBody is null ? 0 : words;
        if (bossBody is not null)
            words += 1 + bossBody.Parts.Length * BossBodyPartWords + bossBody.Parts.Sum(part => part.Shows.Rows.Length);
        ImmutableArray<BgTilePicture?> bg2OverlayPictures = bg2Overlay?.Pictures ?? default;
        int bg2OverlayOffset = ReserveTilePictures(bg2OverlayPictures, RoomWrittenOverlay.CellCount);
        ImmutableArray<BgTilePicture?> overlayPictures = overlay?.Pictures ?? default;
        int overlayOffset = ReserveTilePictures(overlayPictures, RoomWrittenOverlay.CellCount);
        ImmutableArray<BgTilePicture?> bg3Pictures = bg3 is null ? default : [.. bg3.Pictures.Select(picture => (BgTilePicture?)picture)];
        int bg3PicturesOffset = ReserveTilePictures(bg3Pictures, Layer3Plane.Rows * Layer3Plane.Columns);
        // Reserves two words for each of `count` tile pictures; zero without pictures.
        int ReserveTilePictures(ImmutableArray<BgTilePicture?> pictures, int count)
        {
            if (pictures.IsDefault)
                return 0;
            int offset = words;
            words += 2 * count;
            return offset;
        }
        int skyOffset = sky is null ? 0 : words;
        if (sky is not null)
            words += SkyPictures.Rows;
        int floorOffset = mode7 is null ? 0 : words;
        if (mode7 is not null)
            words += mode7.Tiles.Sheets.Length;
        // Each picture's header once, and each image and how it is coloured once as a colour slot (#1408).
        var pictureWords = new Dictionary<SpriteSheetFrame, int>(ReferenceEqualityComparer.Instance);
        var slots = new Dictionary<D3D11ColourSlot, int>();
        var pictureSlots = new Dictionary<SpriteSheetFrame, int>(ReferenceEqualityComparer.Instance);
        foreach ((SpriteSheetFrame picture, D3D11PictureColouring colouring) in Pictures(frame))
        {
            var slot = new D3D11ColourSlot(picture.Sheet, colouring);
            slots.TryAdd(slot, slots.Count);
            if (pictureWords.TryAdd(picture, words))
            {
                pictureSlots[picture] = slots[slot];
                words += PictureHeaderWords;
            }
        }
        // The floor image draws from its sheets directly, so they hold colour slots but no place in the image texture.
        foreach (SpriteSheetFrame sheet in mode7?.Tiles.Sheets ?? [])
            slots.TryAdd(new D3D11ColourSlot(sheet.Sheet, D3D11PictureColouring.Frame), slots.Count);
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
            for (int block = 0; block < table.Blocks; block++)
                upload[pictureTables[slot] + block] = table.Picture(block) is { } shown ? (uint)pictureWords[shown] : 0;
            // Each block's index, with its horizontal and vertical mirroring in bits ten and eleven, as Tiles.hlsl reads a block.
            ImmutableArray<RoomBlockCell> cells = blocks.Cells;
            for (int block = 0; block < cells.Length; block++)
                upload[blockOffsets[slot] + block] = (uint)cells[block].Definition
                    | ((cells[block].Flips & LevelBlockFlipFlags.Horizontal) != 0 ? 1u << 10 : 0)
                    | ((cells[block].Flips & LevelBlockFlipFlags.Vertical) != 0 ? 1u << 11 : 0);
        }
        colourSlots.Clear();
        colourSlots.AddRange(slots.OrderBy(slot => slot.Value).Select(slot => slot.Key));
        foreach ((SpriteSheetFrame picture, int offset) in pictureWords)
        {
            upload[offset] = (uint)pictureSlots[picture];
            upload[offset + 1] = images.Place(picture);
            upload[offset + 2] = D3D11ConstantPacking.Halves(picture.Size.X, picture.Size.Y);
        }
        foreach ((ScreenLayerPictures screen, int offset) in screenTables)
            PackScreenTable(upload.Slice(offset, ScreenTableWords), screen, pictureWords);
        foreach ((ScreenTileMap map, int offset) in screenMaps)
            PackScreenMap(upload.Slice(offset, map.Cells.Length), map);
        foreach ((PlacedPictures layer, int offset) in placedTables)
            PackPlacedPictures(upload, layer, offset, pictureWords);
        foreach ((PictureGrid grid, int offset) in gridTables)
            PackGrid(upload, grid, offset, pictureWords);
        upload[Bg2ImageWord] = (uint)imageOffset;
        upload[SkyImageWord] = (uint)skyImageOffset;
        foreach ((RoomBackgroundImagePlane? plane, int offset) in new[] { (image, imageOffset), (skyImage, skyImageOffset) })
        {
            if (plane is null)
                continue;
            upload[offset] = (uint)pictureWords[plane.Picture];
            for (int cell = 0; cell < plane.CellCount; cell++)
                upload[offset + 1 + cell] = plane.Cell(cell) is { } picture ? (uint)pictureWords[picture] : 0;
        }
        if (door is not null)
            upload[DoorWindowWord] = D3D11ConstantPacking.Halves(door.WindowOrigin.X, door.WindowOrigin.Y);
        upload[OverlayWord] = (uint)overlayOffset;
        PackTilePictures(upload, overlayPictures, overlayOffset, RoomWrittenOverlay.CellCount, pictureWords);
        upload[SkyWord] = (uint)skyOffset;
        if (sky is not null)
            for (int row = 0; row < SkyPictures.Rows; row++)
                upload[skyOffset + row] = sky.RowsShown[row] ? 1u : 0u;
        upload[Bg2OverlayWord] = (uint)bg2OverlayOffset;
        PackTilePictures(upload, bg2OverlayPictures, bg2OverlayOffset, RoomWrittenOverlay.CellCount, pictureWords);
        upload[FloorSheetsWord] = (uint)floorOffset;
        if (mode7 is not null)
        {
            ImmutableArray<SpriteSheetFrame> sheets = mode7.Tiles.Sheets;
            for (int sheet = 0; sheet < sheets.Length; sheet++)
                upload[floorOffset + sheet] = (uint)slots[new D3D11ColourSlot(sheets[sheet].Sheet, D3D11PictureColouring.Frame)];
        }
        upload[XrayRevealWord] = (uint)revealOffset;
        if (reveal is not null)
        {
            ReadOnlySpan<RevealedBlock?> blocks = reveal.Blocks;
            for (int block = 0; block < blocks.Length; block++)
            {
                // A block copied from beyond the room has no picture: zero, which draws nothing.
                if (blocks[block] is not { } shown)
                    continue;
                SpriteSheetFrame picture = reveal.Pictures.Picture(shown.Definition)
                    ?? throw new InvalidOperationException($"The X-ray reveal's block definition {shown.Definition} has no picture.");
                upload[revealOffset + block] = (uint)pictureWords[picture] | ShaderArrangement(shown.Arrangement) << ArrangementShift;
            }
        }
        upload[HudWord] = (uint)hudOffset;
        if (hud is not null)
            PackHud(upload, hud, hudOffset, pictureWords);
        upload[BossBodyWord] = (uint)bossBodyOffset;
        if (bossBody is not null)
            PackBossBody(upload, bossBody, bossBodyOffset, pictureWords);
        upload[Bg3Word] = (uint)bg3PicturesOffset;
        upload[MessagePictureWord] = message is null ? 0 : (uint)pictureWords[message];
        PackTilePictures(upload, bg3Pictures, bg3PicturesOffset, Layer3Plane.Rows * Layer3Plane.Columns, pictureWords);
        packed = frame;
        packedGeneration = images.Generation;
        return true;
    }

    /// <summary>
    /// Packs <paramref name="body"/>'s parts from <paramref name="offset"/>, as <see cref="BossBodyWord"/> describes: the
    /// count, each part's record, then each part's row masks.
    /// </summary>
    private static void PackBossBody(Span<uint> upload, ShownBossBody body, int offset, Dictionary<SpriteSheetFrame, int> pictureWords)
    {
        ImmutableArray<ShownBossBodyPart> parts = body.Parts;
        upload[offset] = (uint)parts.Length;
        int masks = offset + 1 + parts.Length * BossBodyPartWords;
        for (int index = 0; index < parts.Length; index++)
        {
            ShownBossBodyPart part = parts[index];
            int record = offset + 1 + index * BossBodyPartWords;
            upload[record] = (uint)pictureWords[part.Picture];
            upload[record + 1] = (uint)part.Top;
            upload[record + 2] = D3D11ConstantPacking.Halves(checked((ushort)part.Shows.Top), checked((ushort)part.Shows.Rows.Length));
            upload[record + 3] = (uint)masks;
            foreach (uint mask in part.Shows.Rows)
                upload[masks++] = mask;
        }
    }

    /// <summary>
    /// Packs <paramref name="hud"/>'s placed pictures from <paramref name="offset"/>, as <see cref="HudWord"/> describes: the
    /// count, then each picture's offset and its upper-left pixel on the screen, where the HUD's cells start at the top.
    /// </summary>
    private static void PackHud(Span<uint> upload, PicturedHud hud, int offset, Dictionary<SpriteSheetFrame, int> pictureWords)
    {
        ImmutableArray<PlacedHudPicture> pictures = hud.Pictures;
        upload[offset] = (uint)pictures.Length;
        for (int index = 0; index < pictures.Length; index++)
        {
            PlacedHudPicture placed = pictures[index];
            int record = offset + 1 + index * HudPictureWords;
            upload[record] = (uint)pictureWords[placed.Picture];
            upload[record + 1] = D3D11ConstantPacking.Halves(checked((ushort)(placed.Cell.X * GameplayHudDefinitions.CellPixels)),
                checked((ushort)(placed.Cell.Y * GameplayHudDefinitions.CellPixels)));
        }
    }

    /// <summary>
    /// Packs <paramref name="layer"/>'s placed pictures from <paramref name="offset"/>, as <see cref="placedTables"/>
    /// describes: the count, then each picture's offset and its top-left layer pixel.
    /// </summary>
    private static void PackPlacedPictures(Span<uint> upload, PlacedPictures layer, int offset, Dictionary<SpriteSheetFrame, int> pictureWords)
    {
        ImmutableArray<PlacedPicture> pictures = layer.Pictures;
        upload[offset] = (uint)pictures.Length;
        for (int index = 0; index < pictures.Length; index++)
        {
            PlacedPicture placed = pictures[index];
            int record = offset + 1 + index * HudPictureWords;
            upload[record] = (uint)pictureWords[placed.Picture];
            upload[record + 1] = D3D11ConstantPacking.Halves(placed.TopLeft.X, placed.TopLeft.Y);
        }
    }

    /// <summary>
    /// Packs <paramref name="grid"/> from <paramref name="offset"/>, as <see cref="gridTables"/> describes: its size, then
    /// each cell's picture with its mirroring, and its corner in the picture.
    /// </summary>
    private static void PackGrid(Span<uint> upload, PictureGrid grid, int offset, Dictionary<SpriteSheetFrame, int> pictureWords)
    {
        upload[offset] = D3D11ConstantPacking.Halves(grid.Size.X, grid.Size.Y);
        ImmutableArray<GridPicture?> cells = grid.Cells;
        for (int index = 0; index < cells.Length; index++)
        {
            if (cells[index] is not { } cell)
                continue;
            upload[offset + 1 + index] = (uint)pictureWords[cell.Picture]
                | ((cell.Mirroring & TileFlip.Horizontal) != 0 ? GridFlipHorizontally : 0)
                | ((cell.Mirroring & TileFlip.Vertical) != 0 ? GridFlipVertically : 0);
        }
    }

    /// <summary>The room level buffer word of <paramref name="layer"/>'s placed pictures in the last packing (#1467).</summary>
    /// <exception cref="InvalidOperationException">The last packing held no such layer.</exception>
    internal uint PlacedTable(PlacedPictures layer) => placedTables.TryGetValue(layer, out int offset) ? (uint)offset
        : throw new InvalidOperationException("A picture layer's pictures were not packed with its frame.");

    /// <summary>The room level buffer word of <paramref name="grid"/>'s cells in the last packing (#1467).</summary>
    /// <exception cref="InvalidOperationException">The last packing held no such grid.</exception>
    internal uint GridTable(PictureGrid grid) => gridTables.TryGetValue(grid, out int offset) ? (uint)offset
        : throw new InvalidOperationException("A picture grid's cells were not packed with its frame.");

    /// <summary>The tile pictures of an overlay, or none.</summary>
    private static ImmutableArray<BgTilePicture?> Tiles(ImmutableArray<BgTilePicture?>? pictures) =>
        pictures is { IsDefault: false } shown ? shown : [];

    /// <summary>
    /// Every picture <paramref name="frame"/> draws and how it is coloured, in packing order: the planes' and the reveal's
    /// block pictures, the background images, the message, the frontend tile sheets, the boss body's parts, the HUD's
    /// pictures, then the overlays' and the layer-3 plane's tile pictures. A picture may appear more than once.
    /// </summary>
    private static IEnumerable<(SpriteSheetFrame Picture, D3D11PictureColouring Colouring)> Pictures(RoomLevelLayers frame)
    {
        RoomBackgroundImagePlane? skyImage = frame.Sky?.Image;
        foreach (RoomBlockPictureTable? table in new[] { frame.Bg1?.Pictures, frame.Bg2?.Pictures, frame.Door?.SourceRoom.Pictures,
            frame.XrayReveal?.Pictures })
        {
            if (table is null)
                continue;
            for (int block = 0; block < table.Blocks; block++)
            {
                if (table.Picture(block) is { } picture)
                    yield return (picture, D3D11PictureColouring.Frame);
            }
        }
        foreach (RoomBackgroundImagePlane? plane in new[] { frame.Bg2Image, skyImage })
        {
            if (plane is null)
                continue;
            yield return (plane.Picture, D3D11PictureColouring.Frame);
            for (int cell = 0; cell < plane.CellCount; cell++)
            {
                if (plane.Cell(cell) is { } picture)
                    yield return (picture, D3D11PictureColouring.Frame);
            }
        }
        if (frame.MessagePicture is { } message)
            yield return (message, D3D11PictureColouring.MessageText);
        foreach (ScreenLayerPictures screen in frame.Screens?.Layers ?? [])
        {
            foreach (ScreenTileSheet sheet in screen.Sheets)
                yield return (sheet.Picture, D3D11PictureColouring.Frame);
        }
        foreach (ShownBossBodyPart part in frame.BossBody?.Parts ?? [])
            yield return (part.Picture, D3D11PictureColouring.Frame);
        foreach (PlacedHudPicture placed in frame.Hud?.Pictures ?? [])
            yield return (placed.Picture, D3D11PictureColouring.Frame);
        foreach (PlacedPictures layer in frame.Pictures?.Placed ?? [])
        {
            foreach (PlacedPicture placed in layer.Pictures)
                yield return (placed.Picture, D3D11PictureColouring.Frame);
        }
        foreach (PictureGrid grid in frame.Pictures?.Grids ?? [])
        {
            foreach (GridPicture? cell in grid.Cells)
            {
                if (cell is { } shown)
                    yield return (shown.Picture, D3D11PictureColouring.Frame);
            }
        }
        IEnumerable<BgTilePicture?>[] tilePictures =
        [
            Tiles(frame.Bg2Overlay?.Pictures),
            Tiles(frame.Overlay?.Pictures),
            frame.Bg3 is null ? [] : frame.Bg3.Pictures.Select(picture => (BgTilePicture?)picture),
        ];
        foreach (IEnumerable<BgTilePicture?> pictures in tilePictures)
        {
            foreach (BgTilePicture? tile in pictures)
            {
                if (tile is { } shown)
                    yield return (shown.Picture, D3D11PictureColouring.Frame);
            }
        }
    }

    /// <summary>
    /// Packs <paramref name="count"/> tile pictures from <paramref name="offset"/>: each its picture's offset, zero where the
    /// word has no picture, then its corner in the picture, x below y; nothing without pictures.
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
    /// Packs a frontend layer's cells (#1403), one word each, row by row: the cell's tile in the low half, then its colour
    /// variant, its priority and its horizontal and vertical mirroring, as Tiles.hlsl decodes a cell.
    /// </summary>
    private static void PackScreenMap(Span<uint> words, ScreenTileMap map)
    {
        const int variantShift = 16;
        const uint highPriority = 1u << 20, flipHorizontally = 1u << 21, flipVertically = 1u << 22;
        for (int index = 0; index < map.Cells.Length; index++)
        {
            BackgroundTile cell = map.Cells[index];
            words[index] = (uint)cell.Tile | (uint)cell.ColourVariant << variantShift | (cell.HighPriority ? highPriority : 0)
                | (cell.FlipHorizontally ? flipHorizontally : 0) | (cell.FlipVertically ? flipVertically : 0);
        }
    }

    /// <summary>The room level buffer word of <paramref name="pictures"/>' tile table in the last packing (#1392).</summary>
    /// <exception cref="InvalidOperationException">The last packing held no such layer.</exception>
    internal uint ScreenTable(ScreenLayerPictures pictures) => screenTables.TryGetValue(pictures, out int offset) ? (uint)offset
        : throw new InvalidOperationException("A frontend layer's tiles were not packed with its frame.");

    /// <summary>
    /// Packs a frontend layer's tile table (#1392): for each tile its cells name, the tile's picture, zero where no artwork
    /// shows it, the corner of its first colour variant, x below y, and the pixels from one variant's block to the next.
    /// </summary>
    private static void PackScreenTable(Span<uint> table, ScreenLayerPictures screen, Dictionary<SpriteSheetFrame, int> pictureWords)
    {
        for (int index = 0; index < ScreenLayerPictures.CharacterIndices; index++)
        {
            if (screen.Tiles[index] is not { } tile)
                continue;
            Position16<ImagePixels> corner = tile.Sheet.Corner(tile.SheetTile, 0);
            table[3 * index] = (uint)pictureWords[tile.Sheet.Picture];
            table[3 * index + 1] = D3D11ConstantPacking.Halves(corner.X, corner.Y);
            table[3 * index + 2] = (uint)tile.Sheet.RowStride;
        }
    }

    /// <summary>The images the last <see cref="Pack"/> numbered colour slots for, in slot order, with how each is coloured.</summary>
    internal IReadOnlyList<D3D11ColourSlot> ColourSlots => colourSlots;

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
