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
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The frame's room backgrounds packed for Tiles.hlsl at register t3 (#1346): a header of three plane records (layers 1
/// and 2 and a door's source room), the door window, the foreground overlay, effect overlay and the background overlay offsets; then each plane's
/// blocks (#1403), each plane's picture of each block and the pictures themselves with their sheets' colour
/// slots (#1392, #1633), the foreground and the background overlays' pictures (#1348, #1403), the effect overlay's placed picture (#1514),
/// a turning floor (#1349, #1403), a boss body's placed pictures (#1352, #1466, #1514) and the HUD's placed pictures (#1466).
/// The GPU resolves every room pixel from these and the image texture. Repacked only when the published
/// planes, their pictures, offsets, door or overlay change.
/// </summary>
internal sealed class D3D11RoomLevelBuffer : IDisposable
{
    /// <summary>
    /// Words per plane record: block offset, width, height, the layer pixel the plane's first block lies on, X and Y (zero
    /// for the room's own planes; where the door's source screen shows its room), and the offset of the table of each
    /// block's picture's two parts (#1634).
    /// </summary>
    private const int RecordWords = 6;

    /// <summary>Words of a packed picture: its colour slot, its place in the image texture and its width and height.</summary>
    private const int PictureHeaderWords = 3;
    /// <summary>Plane slots: layers 1 and 2 and a door's source room.</summary>
    private const int PlaneSlots = 3;
    /// <summary>Header words: the plane records, the door window origin, and the offsets of the foreground overlay,
    /// the effect overlay, the background overlay and the other tables.</summary>
    private const int HeaderWords = PlaneSlots * RecordWords + 11;
    /// <summary>Header word of the door window's origin on the destination's foreground.</summary>
    private const int DoorWindowWord = PlaneSlots * RecordWords;
    /// <summary>
    /// Header word of the pictures written over the foreground (#1403, #1514), or zero without any: their count, then
    /// <see cref="WrittenPictureWords"/> words each in the order written, the picture's two parts' offsets (#1634), the
    /// foreground pixel its corner lies on and the size it covers, each x below y.
    /// </summary>
    private const int OverlayWord = DoorWindowWord + 1;
    /// <summary>Words of a written picture's record: its parts, its corner and its size.</summary>
    private const int WrittenPictureWords = PartWords + 2;
    /// <summary>
    /// Header word of what the effect overlay shows (#1403, #1514, #1706), or zero where it shows nothing: its kind
    /// (<see cref="FxOverlayAtmosphere"/>, <see cref="FxOverlayLiquidWithSurface"/> or <see cref="FxOverlayLiquidBodyOnly"/>),
    /// then an atmosphere's picture or a liquid's surface strip, then a liquid's body, each as its two parts' offsets
    /// (#1634) and its width and height, x below y.
    /// </summary>
    private const int FxOverlayWord = OverlayWord + 1;
    /// <summary>Words of each picture in the effect overlay's record: its parts and its size.</summary>
    private const int FxOverlayPictureWords = PartWords + 1;
    /// <summary>Words of the effect overlay's record: its kind and two pictures.</summary>
    private const int FxOverlayWords = 1 + 2 * FxOverlayPictureWords;
    /// <summary>The effect overlay's kind for an atmosphere: its picture repeats across and down.</summary>
    private const uint FxOverlayAtmosphere = 0;
    /// <summary>The effect overlay's kind for a liquid whose surface strip shows, nothing above it and the body below it.</summary>
    private const uint FxOverlayLiquidWithSurface = 1;
    /// <summary>The effect overlay's kind for a liquid whose surface lies at or above the screen's top: the body on every line.</summary>
    private const uint FxOverlayLiquidBodyOnly = 2;
    /// <summary>Header word of the pictures written over the background, laid out as <see cref="OverlayWord"/>'s, or zero without any (#1348).</summary>
    private const int WrittenOverBackgroundWord = FxOverlayWord + 1;
    /// <summary>
    /// Header word of the perspective floor's pictures' colour slots, one word each in the order the floor image numbers
    /// its pictures, or zero without a floor (#1408, #1467).
    /// </summary>
    private const int FloorSheetsWord = WrittenOverBackgroundWord + 1;
    /// <summary>
    /// Header word of a boss body's parts (#1352, #1466, #1514, #1540), or zero without one: the count of parts, then five
    /// words for each part in draw order, the first beneath the others: its picture's offset, the body pixel the picture's
    /// top-left corner lies on, how much of the picture shows from that corner, each x below y, the depth it is drawn at
    /// (<see cref="DepthWord"/>, #1634), and the offset of the pixels it hides or zero (#1652). After the parts, each hiding
    /// part's hidden pixels: its cells across below its cells down, then each cell's hidden pixel columns, a byte a cell,
    /// four cells a word, the first cell in the lowest byte.
    /// </summary>
    private const int BossBodyWord = FloorSheetsWord + 1;
    /// <summary>Words of a boss body part's record.</summary>
    private const int BossBodyPartWords = 5;
    /// <summary>Cells of hidden pixel columns one word packs, a byte a cell.</summary>
    private const int HiddenCellsPerWord = 4;
    /// <summary>
    /// Header word of the X-ray reveal, or zero without one (#1392, #1514): the foreground pixel its first block lies on, x
    /// below y, then each block row by row, its picture's two parts' offsets (#1634), the first with the block's arrangement
    /// in the top four bits.
    /// </summary>
    private const int XrayRevealWord = BossBodyWord + 1;
    /// <summary>
    /// Header word of a library background's image (#1392), or zero without one: the image picture's two parts' offsets
    /// (#1634), the image's width and height and the area its rectangles lie on, each x below y (#1514), the count of its
    /// rectangles, three words for each (#1555: the area pixel its corner lies on, the image pixel that corner shows, and its
    /// size, each x below y), then each 16x16 cell's picture's two parts' offsets, row by row across the image, zero where
    /// the image shows.
    /// </summary>
    private const int LibraryBackgroundImageWord = XrayRevealWord + 1;
    /// <summary>
    /// Words of a picture's two parts wherever a table names a picture of a room's tiles (#1634): the offset of its part
    /// drawn behind, then of its part drawn in front, each zero where that part shows nothing.
    /// </summary>
    private const int PartWords = 2;
    /// <summary>Word of a background image's record holding its width and height, after its picture's parts.</summary>
    private const int ImageSizeWord = PartWords;
    /// <summary>Word of a background image's record holding the area its rectangles lie on.</summary>
    private const int ImageAreaWord = ImageSizeWord + 1;
    /// <summary>Word of a background image's record holding the count of its rectangles.</summary>
    private const int ImagePieceCountWord = ImageAreaWord + 1;
    /// <summary>Words of a background image's record before its rectangles.</summary>
    private const int ImageHeaderWords = ImagePieceCountWord + 1;
    /// <summary>The bit of a background image cell's first part word marking that the cell shows its own picture in place of the image's.</summary>
    private const uint CellShowsOwnPicture = 1u << 31;
    /// <summary>Words of a background image's rectangle: its corner on the area, the image pixel it shows there, and its size.</summary>
    private const int ImagePieceWords = 3;
    /// <summary>Header word of the scrolling sky's image, laid out as <see cref="LibraryBackgroundImageWord"/>'s, or zero without one (#1392).</summary>
    private const int SkyImageWord = LibraryBackgroundImageWord + 1;
    /// <summary>Header word of the image of the message box the frame shows, or zero without one (#1392).</summary>
    private const int MessagePictureWord = SkyImageWord + 1;
    /// <summary>
    /// Header word of the HUD's placed pictures (#1466), or zero without a HUD: the count of pictures, then two words each in
    /// drawing order, its picture's offset and its upper-left pixel on the screen, x below y.
    /// </summary>
    private const int HudWord = MessagePictureWord + 1;
    /// <summary>Words of a placed HUD picture's record.</summary>
    private const int HudPictureWords = 2;
    /// <summary>
    /// Words of a pictured plane's table before its pictures (#1467, #1555): its count of pictures; each picture follows in
    /// drawing order as two words, its picture's offset and its upper-left plane pixel, x below y, each a signed sixteen-bit
    /// half.
    /// </summary>
    private const int PlaneTableHeaderWords = 1;
    /// <summary>Words of a placed plane picture's record.</summary>
    private const int PlanePictureWords = 2;
    /// <summary>The bit an X-ray reveal block's arrangement starts at.</summary>
    private const int ArrangementShift = 28;
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
    /// <summary>The colour slot of each image and how it is coloured, in slot order, as the last <see cref="Pack"/> numbered them.</summary>
    private readonly List<D3D11ColourSlot> colourSlots = [];
    /// <summary>Where the last <see cref="Pack"/> put each pictured plane's table (#1467).</summary>
    private readonly Dictionary<PicturedPlane, int> planeTables = [];
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
            foreach (ShownPicture shown in Pictures(frame))
                collectedPictures.Add(shown.Picture);
            collected = frame;
        }
        foreach (SpriteSheetFrame picture in collectedPictures)
            needed.Add(picture);
    }

    /// <summary>
    /// Packs the frame's room planes against the image texture's places; false when they match the last packing and the
    /// texture has kept every picture where it was.
    /// </summary>
    /// <param name="frame">The planes, overlays, effect overlay, floor, boss body, X-ray reveal and HUD the frame shows.</param>
    /// <param name="images">The image texture, holding every picture <see cref="Collect"/> named.</param>
    internal bool Pack(RoomLevelLayers frame, D3D11ImageAtlas images)
    {
        RoomBackgroundLayer? foreground = frame.Foreground, background = frame.Background;
        RoomDoorBackground? door = frame.Door;
        RoomOverlayPictures? writtenOverForeground = frame.ForegroundText, writtenOverBackground = frame.BackgroundText;
        SkyPictures? sky = frame.Sky;
        ShownFxOverlay? fxOverlay = frame.FxOverlay;
        AffineFloor? turningFloor = frame.Floor;
        ShownBossBody? bossBody = frame.BossBody;
        XrayRevealPlane? reveal = frame.XrayReveal;
        RoomBackgroundImagePlane? image = frame.BackgroundImage, skyImage = sky?.Image;
        SpriteSheetFrame? message = frame.MessagePicture;
        PicturedHud? hud = frame.Hud;
        ImmutableArray<PicturedPlane> pictured = frame.Planes?.Planes ?? [];
        ImmutableArray<PictureGrid> grids = frame.Grids?.Grids ?? [];
        if (packed == frame && packedGeneration == images.Generation)
            return false;
        // No dispatch samples t3 without one of these, so such a scene keeps the last packed room unread. It colours no
        // picture either, so the last room's colour slots go: the scene's colours are packed for none of its images, and a
        // later frame showing that room again packs it afresh.
        if (foreground is null && background is null && sky is null && fxOverlay is null && turningFloor is null && reveal is null && image is null
            && message is null && hud is null && pictured.IsEmpty && grids.IsEmpty)
        {
            colourSlots.Clear();
            packed = null;
            return false;
        }
        RoomBackgroundLayer?[] planes = [foreground, background, door?.SourceRoom];
        int words = HeaderWords;
        planeTables.Clear();
        foreach (PicturedPlane plane in pictured)
        {
            if (planeTables.TryAdd(plane, words))
                words += PlaneTableHeaderWords + plane.Pictures.Length * PlanePictureWords;
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
            words += table.Blocks * PartWords;
        }
        int revealOffset = reveal is null ? 0 : words;
        if (reveal is not null)
            words += 1 + reveal.Blocks.Length * PartWords;
        int imageOffset = image is null ? 0 : words;
        if (image is not null)
            words += ImageHeaderWords + image.Pieces.Length * ImagePieceWords + image.CellCount * PartWords;
        int skyImageOffset = skyImage is null ? 0 : words;
        if (skyImage is not null)
            words += ImageHeaderWords + skyImage.Pieces.Length * ImagePieceWords + skyImage.CellCount * PartWords;
        int hudOffset = hud is null ? 0 : words;
        if (hud is not null)
            words += 1 + hud.Pictures.Length * HudPictureWords;
        int bossBodyOffset = bossBody is null ? 0 : words;
        if (bossBody is not null)
            words += 1 + bossBody.Parts.Length * BossBodyPartWords + bossBody.Parts.Sum(part => HiddenWords(part.Hidden));
        int backgroundWrittenOffset = ReserveWritten(writtenOverBackground);
        int foregroundWrittenOffset = ReserveWritten(writtenOverForeground);
        // Reserves the count and two words for each written picture; zero without any.
        int ReserveWritten(RoomOverlayPictures? written)
        {
            if (written is null)
                return 0;
            int offset = words;
            words += 1 + WrittenPictureWords * written.Pictures.Length;
            return offset;
        }
        int fxOverlayOffset = fxOverlay is null ? 0 : words;
        if (fxOverlay is not null)
            words += FxOverlayWords;
        int floorOffset = turningFloor is null ? 0 : words;
        if (turningFloor is not null)
            words += turningFloor.Pictures.Length;
        // Each picture's header once for each way it is coloured, and each image and how it is coloured once as a colour
        // slot (#1408): a picture two groups show, as a block of one tileset in both rooms of a door, shows each group's
        // shading (#1512).
        var pictureWords = new Dictionary<ShownPicture, int>(ShownPictureComparer.Instance);
        var slots = new Dictionary<D3D11ColourSlot, int>();
        var pictureSlots = new Dictionary<ShownPicture, int>(ShownPictureComparer.Instance);
        foreach (ShownPicture shown in Pictures(frame))
        {
            var slot = new D3D11ColourSlot(shown.Picture.Sheet, shown.Colouring, shown.Shading);
            slots.TryAdd(slot, slots.Count);
            if (pictureWords.TryAdd(shown, words))
            {
                pictureSlots[shown] = slots[slot];
                words += PictureHeaderWords;
            }
        }
        // The floor image draws from its pictures directly, so they hold colour slots but no place in the image texture.
        foreach (SpriteSheetFrame sheet in turningFloor?.Pictures ?? [])
            slots.TryAdd(new D3D11ColourSlot(sheet.Sheet, frame.FloorColouring, turningFloor!.Shading), slots.Count);
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
            // The door's source room lies on the destination so that its screen's window origin shows its source origin (#1555).
            Offset16<PlanePixels> origin = slot == PlaneSlots - 1 && door is not null ? door.WindowOrigin - door.SourceOrigin : Offset16<PlanePixels>.Zero;
            upload[record + 3] = unchecked((ushort)origin.X);
            upload[record + 4] = unchecked((ushort)origin.Y);
            upload[record + 5] = (uint)pictureTables[slot];
            RoomBlockPictureTable table = layer.Pictures ?? throw new InvalidOperationException("A room plane is drawn once its pictures are resolved.");
            for (int block = 0; block < table.Blocks; block++)
                PackParts(upload, pictureTables[slot] + block * PartWords, table.Picture(block), PlaneColouring(slot), pictureWords);
            // Each cell's picture definition, a block's or past the blocks a collectible's (#1538), with its horizontal and
            // vertical mirroring in bits sixteen and seventeen, as Tiles.hlsl reads a block.
            ImmutableArray<RoomBlockCell> cells = blocks.Cells;
            for (int block = 0; block < cells.Length; block++)
                upload[blockOffsets[slot] + block] = (uint)cells[block].Definition
                    | ((cells[block].Flips & BlockMirroring.Horizontal) != 0 ? 1u << 16 : 0)
                    | ((cells[block].Flips & BlockMirroring.Vertical) != 0 ? 1u << 17 : 0);
        }
        colourSlots.Clear();
        colourSlots.AddRange(slots.OrderBy(slot => slot.Value).Select(slot => slot.Key));
        foreach ((ShownPicture shown, int offset) in pictureWords)
        {
            upload[offset] = (uint)pictureSlots[shown];
            upload[offset + 1] = images.Place(shown.Picture);
            upload[offset + 2] = D3D11ConstantPacking.Halves(shown.Picture.Size.X, shown.Picture.Size.Y);
        }
        foreach ((PicturedPlane plane, int offset) in planeTables)
            PackPlane(upload, plane, offset, pictureWords);
        foreach ((PictureGrid grid, int offset) in gridTables)
            PackGrid(upload, grid, offset, pictureWords);
        upload[LibraryBackgroundImageWord] = (uint)imageOffset;
        upload[SkyImageWord] = (uint)skyImageOffset;
        foreach ((RoomBackgroundImagePlane? plane, int offset) in new[] { (image, imageOffset), (skyImage, skyImageOffset) })
        {
            if (plane is null)
                continue;
            PackParts(upload, offset, plane.Picture, D3D11PictureColouring.Room, pictureWords);
            upload[offset + ImageSizeWord] = D3D11ConstantPacking.Halves(plane.Size.X, plane.Size.Y);
            upload[offset + ImageAreaWord] = D3D11ConstantPacking.Halves(plane.Area.X, plane.Area.Y);
            upload[offset + ImagePieceCountWord] = (uint)plane.Pieces.Length;
            for (int index = 0; index < plane.Pieces.Length; index++)
            {
                RoomBackgroundImagePiece piece = plane.Pieces[index];
                int record = offset + ImageHeaderWords + index * ImagePieceWords;
                upload[record] = D3D11ConstantPacking.Halves(piece.Corner.X, piece.Corner.Y);
                upload[record + 1] = D3D11ConstantPacking.Halves(piece.Source.X, piece.Source.Y);
                upload[record + 2] = D3D11ConstantPacking.Halves(piece.Size.X, piece.Size.Y);
            }
            int cells = offset + ImageHeaderWords + plane.Pieces.Length * ImagePieceWords;
            for (int cell = 0; cell < plane.CellCount; cell++)
            {
                if (plane.Cell(cell) is not { } shown)
                    continue;
                PackParts(upload, cells + cell * PartWords, shown, D3D11PictureColouring.Room, pictureWords);
                upload[cells + cell * PartWords] |= CellShowsOwnPicture;
            }
        }
        if (door is not null)
            upload[DoorWindowWord] = D3D11ConstantPacking.Halves(door.WindowOrigin.X, door.WindowOrigin.Y);
        upload[OverlayWord] = (uint)foregroundWrittenOffset;
        PackWritten(upload, writtenOverForeground, foregroundWrittenOffset, pictureWords);
        upload[WrittenOverBackgroundWord] = (uint)backgroundWrittenOffset;
        PackWritten(upload, writtenOverBackground, backgroundWrittenOffset, pictureWords);
        upload[FloorSheetsWord] = (uint)floorOffset;
        if (turningFloor is not null)
        {
            ImmutableArray<SpriteSheetFrame> sheets = turningFloor.Pictures;
            for (int sheet = 0; sheet < sheets.Length; sheet++)
                upload[floorOffset + sheet] = (uint)slots[new D3D11ColourSlot(sheets[sheet].Sheet, frame.FloorColouring, turningFloor.Shading)];
        }
        upload[XrayRevealWord] = (uint)revealOffset;
        if (reveal is not null)
        {
            upload[revealOffset] = D3D11ConstantPacking.Halves(reveal.Origin.X, reveal.Origin.Y);
            ReadOnlySpan<RevealedBlock?> blocks = reveal.Blocks;
            for (int block = 0; block < blocks.Length; block++)
            {
                // A block copied from beyond the room has no picture: zero, which draws nothing.
                if (blocks[block] is not { } shown)
                    continue;
                LayeredPicture picture = reveal.Pictures.Picture(shown.Definition)
                    ?? throw new InvalidOperationException($"The X-ray reveal's block definition {shown.Definition} has no picture.");
                int record = revealOffset + 1 + block * PartWords;
                PackParts(upload, record, picture, D3D11PictureColouring.Room, pictureWords);
                upload[record] |= ShaderArrangement(shown.Arrangement) << ArrangementShift;
            }
        }
        upload[HudWord] = (uint)hudOffset;
        if (hud is not null)
            PackHud(upload, hud, hudOffset, pictureWords, frame.HudColouring);
        upload[BossBodyWord] = (uint)bossBodyOffset;
        if (bossBody is not null)
            PackBossBody(upload, bossBody, bossBodyOffset, pictureWords);
        upload[FxOverlayWord] = (uint)fxOverlayOffset;
        if (fxOverlay is { } shownFx)
            PackFxOverlay(upload, fxOverlayOffset, shownFx, pictureWords);
        upload[MessagePictureWord] = message is null ? 0 : (uint)pictureWords[new(message, D3D11PictureColouring.MessageBox)];
        packed = frame;
        packedGeneration = images.Generation;
        return true;
    }

    /// <summary>Packs what the effect overlay shows at <paramref name="offset"/>, as <see cref="FxOverlayWord"/> describes.</summary>
    private static void PackFxOverlay(Span<uint> upload, int offset, ShownFxOverlay shown, Dictionary<ShownPicture, int> pictureWords)
    {
        switch (shown.Artwork)
        {
            case AtmosphereArtwork atmosphere:
                upload[offset] = FxOverlayAtmosphere;
                PackFxOverlayPicture(upload, offset + 1, atmosphere.Picture, atmosphere.Size, pictureWords);
                break;
            case LiquidArtwork liquid:
                upload[offset] = shown.ShowsSurface ? FxOverlayLiquidWithSurface : FxOverlayLiquidBodyOnly;
                PackFxOverlayPicture(upload, offset + 1, liquid.Surface, liquid.SurfaceSize, pictureWords);
                PackFxOverlayPicture(upload, offset + 1 + FxOverlayPictureWords, liquid.Body, liquid.BodySize, pictureWords);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shown), shown.Artwork, "Undefined room effect artwork.");
        }
    }

    /// <summary>Packs one of the effect overlay's pictures at <paramref name="at"/>: its parts, then its size.</summary>
    private static void PackFxOverlayPicture(Span<uint> upload, int at, LayeredPicture picture, Extent16<ImagePixels> size,
        Dictionary<ShownPicture, int> pictureWords)
    {
        PackParts(upload, at, picture, D3D11PictureColouring.FxOverlay, pictureWords);
        upload[at + PartWords] = D3D11ConstantPacking.Halves(size.X, size.Y);
    }

    /// <summary>
    /// Packs the offsets of <paramref name="picture"/>'s two parts at <paramref name="at"/>, as <see cref="PartWords"/>
    /// describes: zero for a part that shows nothing, and both zero for no picture.
    /// </summary>
    private static void PackParts(Span<uint> upload, int at, LayeredPicture? picture, D3D11PictureColouring colouring,
        Dictionary<ShownPicture, int> pictureWords)
    {
        upload[at] = picture?.Behind is { } behind ? (uint)pictureWords[new(behind, colouring)] : 0;
        upload[at + 1] = picture?.InFront is { } inFront ? (uint)pictureWords[new(inFront, colouring)] : 0;
    }

    /// <summary>
    /// Packs <paramref name="body"/>'s parts from <paramref name="offset"/>, as <see cref="BossBodyWord"/> describes: the
    /// count, then each part's record in draw order.
    /// </summary>
    private static void PackBossBody(Span<uint> upload, ShownBossBody body, int offset, Dictionary<ShownPicture, int> pictureWords)
    {
        ImmutableArray<ShownBossBodyPart> parts = body.Parts;
        upload[offset] = (uint)parts.Length;
        int hiddenOffset = offset + 1 + parts.Length * BossBodyPartWords;
        for (int index = 0; index < parts.Length; index++)
        {
            ShownBossBodyPart part = parts[index];
            int record = offset + 1 + index * BossBodyPartWords;
            upload[record] = (uint)pictureWords[new(part.Picture, D3D11PictureColouring.BossBody)];
            upload[record + 1] = D3D11ConstantPacking.Halves(part.Corner.X, part.Corner.Y);
            upload[record + 2] = D3D11ConstantPacking.Halves(part.Size.X, part.Size.Y);
            upload[record + 3] = DepthWord(part.Depth);
            upload[record + 4] = 0;
            if (part.Hidden is { } hidden)
            {
                upload[record + 4] = (uint)hiddenOffset;
                PackHidden(upload, hidden, hiddenOffset);
                hiddenOffset += HiddenWords(hidden);
            }
        }
    }

    /// <summary>Words <paramref name="hidden"/>'s pixels pack into (#1652): its size, then its cells four a word; none without any.</summary>
    private static int HiddenWords(HiddenPictureColumns? hidden) =>
        hidden is null ? 0 : 1 + ((hidden.Cells.Length + HiddenCellsPerWord - 1) / HiddenCellsPerWord);

    /// <summary>Packs <paramref name="hidden"/> from <paramref name="offset"/>, as <see cref="BossBodyWord"/> describes.</summary>
    private static void PackHidden(Span<uint> upload, HiddenPictureColumns hidden, int offset)
    {
        upload[offset] = D3D11ConstantPacking.Halves(checked((ushort)hidden.CellsAcross), checked((ushort)hidden.CellsDown));
        for (int word = 0; word < HiddenWords(hidden) - 1; word++)
        {
            uint packed = 0;
            for (int cell = 0; cell < HiddenCellsPerWord && (word * HiddenCellsPerWord) + cell < hidden.Cells.Length; cell++)
                packed |= (uint)hidden.Cells[(word * HiddenCellsPerWord) + cell] << (8 * cell);
            upload[offset + 1 + word] = packed;
        }
    }

    /// <summary>
    /// The word a boss body part's depth packs as (#1634), which Tiles.hlsl's BossBodyBehindSprites and
    /// BossBodyInFrontOfSprites name.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="depth"/> is not a defined depth.</exception>
    internal static uint DepthWord(BossBodyDepth depth) => depth switch
    {
        BossBodyDepth.BehindSprites => 0,
        BossBodyDepth.InFrontOfSprites => 1,
        _ => throw new InvalidOperationException($"Undefined boss body depth {depth}."),
    };

    /// <summary>
    /// Packs <paramref name="hud"/>'s placed pictures from <paramref name="offset"/>, as <see cref="HudWord"/> describes: the
    /// count, then each picture's offset and its upper-left pixel on the screen.
    /// </summary>
    private static void PackHud(Span<uint> upload, PicturedHud hud, int offset, Dictionary<ShownPicture, int> pictureWords,
        D3D11PictureColouring colouring)
    {
        ImmutableArray<PlacedHudPicture> pictures = hud.Pictures;
        upload[offset] = (uint)pictures.Length;
        for (int index = 0; index < pictures.Length; index++)
        {
            PlacedHudPicture placed = pictures[index];
            int record = offset + 1 + index * HudPictureWords;
            upload[record] = (uint)pictureWords[new(placed.Picture, colouring, placed.Shading)];
            upload[record + 1] = D3D11ConstantPacking.Halves(placed.Corner.X, placed.Corner.Y);
        }
    }

    /// <summary>
    /// Packs <paramref name="plane"/>'s table from <paramref name="offset"/>, as <see cref="PlaneTableHeaderWords"/>
    /// describes: the count of its pictures, then each picture's offset and its upper-left plane pixel.
    /// </summary>
    private static void PackPlane(Span<uint> upload, PicturedPlane plane, int offset, Dictionary<ShownPicture, int> pictureWords)
    {
        ImmutableArray<PlacedPlanePicture> pictures = plane.Pictures;
        upload[offset] = (uint)pictures.Length;
        for (int index = 0; index < pictures.Length; index++)
        {
            PlacedPlanePicture placed = pictures[index];
            int record = offset + PlaneTableHeaderWords + index * PlanePictureWords;
            upload[record] = (uint)pictureWords[new(placed.Picture, D3D11PictureColouring.OwnShading, placed.Shading)];
            upload[record + 1] = D3D11ConstantPacking.SignedHalves(placed.Corner.X, placed.Corner.Y);
        }
    }

    /// <summary>
    /// Packs <paramref name="grid"/> from <paramref name="offset"/>, as <see cref="gridTables"/> describes: its size, then
    /// each cell's picture with its mirroring.
    /// </summary>
    private static void PackGrid(Span<uint> upload, PictureGrid grid, int offset, Dictionary<ShownPicture, int> pictureWords)
    {
        upload[offset] = D3D11ConstantPacking.Halves(grid.Size.X, grid.Size.Y);
        ImmutableArray<GridPicture?> cells = grid.Cells;
        for (int index = 0; index < cells.Length; index++)
        {
            if (cells[index] is not { } cell)
                continue;
            upload[offset + 1 + index] = (uint)pictureWords[new(cell.Picture, D3D11PictureColouring.OwnShading, cell.Shading)]
                | ((cell.Mirroring & PictureFlip.Horizontal) != 0 ? GridFlipHorizontally : 0)
                | ((cell.Mirroring & PictureFlip.Vertical) != 0 ? GridFlipVertically : 0);
        }
    }

    /// <summary>The room level buffer word of <paramref name="grid"/>'s cells in the last packing (#1467).</summary>
    /// <exception cref="InvalidOperationException">The last packing held no such grid.</exception>
    internal uint GridTable(PictureGrid grid) => gridTables.TryGetValue(grid, out int offset) ? (uint)offset
        : throw new InvalidOperationException("A picture grid's cells were not packed with its frame.");

    /// <summary>The room level buffer word of <paramref name="plane"/>'s table in the last packing (#1467).</summary>
    /// <exception cref="InvalidOperationException">The last packing held no such plane.</exception>
    internal uint PlaneTable(PicturedPlane plane) => planeTables.TryGetValue(plane, out int offset) ? (uint)offset
        : throw new InvalidOperationException("A pictured plane was not packed with its frame.");

    /// <summary>
    /// Packs <paramref name="written"/>'s pictures from <paramref name="offset"/>, as <see cref="OverlayWord"/> describes:
    /// the count, then each picture's parts, the layer pixel its corner lies on and its size; nothing without pictures.
    /// </summary>
    private static void PackWritten(Span<uint> upload, RoomOverlayPictures? written, int offset, Dictionary<ShownPicture, int> pictureWords)
    {
        if (written is null)
            return;
        ImmutableArray<PlacedWrittenPicture> pictures = written.Pictures;
        upload[offset] = (uint)pictures.Length;
        for (int index = 0; index < pictures.Length; index++)
        {
            PlacedWrittenPicture placed = pictures[index];
            int record = offset + 1 + index * WrittenPictureWords;
            PackParts(upload, record, placed.Picture, D3D11PictureColouring.WrittenOverRoom, pictureWords);
            upload[record + PartWords] = D3D11ConstantPacking.Halves(placed.Corner.X, placed.Corner.Y);
            upload[record + PartWords + 1] = D3D11ConstantPacking.Halves(placed.Size.X, placed.Size.Y);
        }
    }

    /// <summary>
    /// Every picture <paramref name="frame"/> draws and how it is coloured, in packing order: the planes' and the reveal's
    /// block pictures, the background images, the message, the boss body's pictures, the HUD's pictures, the pictured planes'
    /// and the picture grids' pictures, then the written pictures and the effect overlay's pictures. A picture may appear more
    /// than once, and under more than one colouring (#1512).
    /// </summary>
    private static IEnumerable<ShownPicture> Pictures(RoomLevelLayers frame)
    {
        RoomBackgroundImagePlane? skyImage = frame.Sky?.Image;
        RoomBlockPictureTable?[] tables = [frame.Foreground?.Pictures, frame.Background?.Pictures, frame.Door?.SourceRoom.Pictures];
        for (int slot = 0; slot < tables.Length; slot++)
        {
            if (tables[slot] is not { } table)
                continue;
            for (int block = 0; block < table.Blocks; block++)
            {
                foreach (SpriteSheetFrame part in table.Picture(block)?.Parts ?? [])
                    yield return new(part, PlaneColouring(slot));
            }
        }
        if (frame.XrayReveal?.Pictures is { } reveal)
        {
            for (int block = 0; block < reveal.Blocks; block++)
            {
                foreach (SpriteSheetFrame part in reveal.Picture(block)?.Parts ?? [])
                    yield return new(part, D3D11PictureColouring.Room);
            }
        }
        foreach (RoomBackgroundImagePlane? plane in new[] { frame.BackgroundImage, skyImage })
        {
            if (plane is null)
                continue;
            foreach (SpriteSheetFrame part in plane.Picture.Parts)
                yield return new(part, D3D11PictureColouring.Room);
            for (int cell = 0; cell < plane.CellCount; cell++)
            {
                foreach (SpriteSheetFrame part in plane.Cell(cell)?.Parts ?? [])
                    yield return new(part, D3D11PictureColouring.Room);
            }
        }
        if (frame.MessagePicture is { } message)
            yield return new(message, D3D11PictureColouring.MessageBox);
        foreach (ShownBossBodyPart part in frame.BossBody?.Parts ?? [])
            yield return new(part.Picture, D3D11PictureColouring.BossBody);
        foreach (PlacedHudPicture placed in frame.Hud?.Pictures ?? [])
            yield return new(placed.Picture, frame.HudColouring, placed.Shading);
        foreach (PicturedPlane plane in frame.Planes?.Planes ?? [])
        {
            foreach (PlacedPlanePicture placed in plane.Pictures)
                yield return new(placed.Picture, D3D11PictureColouring.OwnShading, placed.Shading);
        }
        foreach (PictureGrid grid in frame.Grids?.Grids ?? [])
        {
            foreach (GridPicture? cell in grid.Cells)
            {
                if (cell is { } shown)
                    yield return new(shown.Picture, D3D11PictureColouring.OwnShading, shown.Shading);
            }
        }
        foreach (PlacedWrittenPicture placed in (frame.BackgroundText?.Pictures ?? []).Concat(frame.ForegroundText?.Pictures ?? []))
        {
            foreach (SpriteSheetFrame part in placed.Picture.Parts)
                yield return new(part, D3D11PictureColouring.WrittenOverRoom);
        }
        IEnumerable<LayeredPicture> fxPictures = frame.FxOverlay?.Artwork switch
        {
            null => [],
            AtmosphereArtwork atmosphere => [atmosphere.Picture],
            LiquidArtwork liquid => [liquid.Surface, liquid.Body],
            { } other => throw new ArgumentOutOfRangeException(nameof(frame), other, "Undefined room effect artwork."),
        };
        foreach (SpriteSheetFrame part in fxPictures.SelectMany(picture => picture.Parts))
            yield return new(part, D3D11PictureColouring.FxOverlay);
    }

    /// <summary>How plane slot <paramref name="slot"/>'s block pictures are coloured: layers 1 and 2 the room's, the third the room a door leaves (#1512).</summary>
    private static D3D11PictureColouring PlaneColouring(int slot) =>
        slot == PlaneSlots - 1 ? D3D11PictureColouring.DoorSourceRoom : D3D11PictureColouring.Room;

    /// <summary>A picture the frame draws and how it is coloured there (#1512).</summary>
    /// <param name="Picture">The picture.</param>
    /// <param name="Colouring">How it is coloured while no shading names its colours.</param>
    /// <param name="Shading">The shades and colour effects it shows, or null for <paramref name="Colouring"/>'s.</param>
    private readonly record struct ShownPicture(SpriteSheetFrame Picture, D3D11PictureColouring Colouring, PictureShading? Shading = null);

    /// <summary>Tells shown pictures apart by the picture's identity, its colouring and its shading.</summary>
    private sealed class ShownPictureComparer : IEqualityComparer<ShownPicture>
    {
        /// <summary>The one comparer.</summary>
        internal static ShownPictureComparer Instance { get; } = new();

        /// <inheritdoc />
        public bool Equals(ShownPicture x, ShownPicture y) => ReferenceEquals(x.Picture, y.Picture) && x.Colouring == y.Colouring && Equals(x.Shading, y.Shading);

        /// <inheritdoc />
        public int GetHashCode(ShownPicture obj) => HashCode.Combine(ReferenceEqualityComparer.Instance.GetHashCode(obj.Picture), obj.Colouring, obj.Shading);
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
