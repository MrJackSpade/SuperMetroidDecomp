using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Constructed room backgrounds for render tests (#1346): a blank room, or a seeded one over the screen.</summary>
internal static class RoomBackgroundsFixtures
{
    /// <summary>Blocks along each side of a fixture room.</summary>
    private const int SideBlocks = 32;

    /// <summary>Blocks a fixture room loads: every ten-bit visual index.</summary>
    private const int Definitions = 1024;

    /// <summary>Tiles a fixture paints into each block picture: top left, top right, bottom left, bottom right.</summary>
    private const int BlockTiles = 4;

    /// <summary>A room whose BG1 shows tile word zero over the whole unscrolled screen, no BG2 plane, and no effect page.</summary>
    internal static RoomBackgrounds Blank() => Uniform(new byte[SnesVram.ByteCount], new RgbColor[SnesCgram.ColorCount], default);

    /// <summary>An effect overlay showing nothing (#1403, #1514, #1706).</summary>
    internal static FxOverlayArtwork BlankFxOverlay() => new(null);

    /// <summary>A Ceres room's turning floor, with its floor animation frames, whose drawing a check does not look at (#1467).</summary>
    internal static RoomFloor CeresFloor() => new(AffineFloorConstants.BlankCeresRoom());

    /// <summary>A turning floor of tile zero everywhere, showing nothing.</summary>
    internal static AffineFloor BlankMode7Floor() => AffineFloorConstants.BlankCeresRoom().Floor;

    /// <summary>
    /// A room whose BG1 shows <paramref name="tile"/> in every cell over the whole unscrolled screen, as a VRAM BG1
    /// tilemap filled with that word did, except the listed eight-pixel <paramref name="cells"/>; no BG2 plane; and no
    /// effect page. Its blocks draw pictures of the characters at the start of <paramref name="image"/>, coloured from
    /// <paramref name="palette"/> (#1403).
    /// </summary>
    internal static RoomBackgrounds Uniform(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, SnesBgTilemapWord tile,
        params ReadOnlySpan<TilemapCell> cells)
    {
        // Each block's four cells, top left, top right, bottom left, bottom right, as its picture is painted from them.
        var blockCells = new ushort[SideBlocks * SideBlocks][];
        for (int block = 0; block < blockCells.Length; block++)
            blockCells[block] = [tile.Raw, tile.Raw, tile.Raw, tile.Raw];
        foreach (var (column, row, cell) in cells)
            blockCells[row / 2 * SideBlocks + column / 2][row % 2 * 2 + column % 2] = cell.Raw;
        var definitionIndices = new Dictionary<BlockCellWords, ushort>();
        var definitions = new BackgroundTile[Definitions * BlockTiles];
        var blocks = new RoomBlockAppearance[blockCells.Length];
        for (int block = 0; block < blocks.Length; block++)
        {
            ushort[] words = blockCells[block];
            var key = new BlockCellWords(words[0], words[1], words[2], words[3]);
            if (!definitionIndices.TryGetValue(key, out ushort index))
            {
                index = (ushort)definitionIndices.Count;
                definitionIndices.Add(key, index);
                for (int corner = 0; corner < words.Length; corner++)
                    definitions[index * BlockTiles + corner] = BackgroundTileWordBytes.Tile(words[corner]);
            }
            blocks[block] = RoomBlockAppearance.Of(index);
        }
        // BG2 shows the tilemap the fixture loaded at the gameplay BG2 tilemap, as a library background's image (#1403).
        return new(new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(SideBlocks, SideBlocks),
            blocks).Cells, RoomBlockFixtures.Pictures(definitions, image, palette)), null, BlankFxOverlay(),
            BackgroundImage: RoomBlockFixtures.LoadedBackground(image, palette, GameplayVramLayout.Bg2TilemapWord));
    }

    /// <summary>
    /// A seeded X-ray reveal over seeded block pictures coloured as <paramref name="scene"/>'s planes would be (#1392):
    /// random definitions in every arrangement.
    /// </summary>
    internal static SuperMetroid.Core.Samus.Abilities.XrayRevealPlane SeededReveal(Random random, RgbColor[] scene)
    {
        RoomBlockPictureTable pictures = SeededPictures(random, scene);
        RevealedBlockArrangement[] arrangements = Enum.GetValues<RevealedBlockArrangement>();
        var blocks = new RevealedBlock?[SuperMetroid.Core.Samus.Abilities.XrayRevealArea.BlockCount];
        for (int block = 0; block < blocks.Length; block++)
            blocks[block] = new RevealedBlock(random.Next(Definitions), arrangements[random.Next(arrangements.Length)]);
        return new SuperMetroid.Core.Samus.Abilities.XrayRevealPlane(default, blocks, pictures);
    }

    /// <summary>
    /// A seeded library background image over seeded colours as <see cref="SeededPictures"/> colours them, with about one
    /// cell in four drawn from a seeded block picture in its place (#1392).
    /// </summary>
    internal static RoomBackgroundImagePlane SeededBackground(Random random, RgbColor[] scene) =>
        SeededImage(random, scene, RoomBackgroundImageGeometry.Words / RoomBackgroundImageGeometry.PageTiles);

    /// <summary>
    /// A seeded boss body of <paramref name="rows"/> eight-pixel rows (#1466, #1514, #1515, #1540) whose screens of rows cover
    /// <paramref name="area"/> left to right and then downward: every four rows a seeded block picture tiled across them,
    /// one part for each place of each of its parts, at that part's depth (#1634).
    /// </summary>
    internal static ShownBossBody SeededBody(Random random, RgbColor[] scene, int rows, Extent16<BackgroundPixels> area)
    {
        RoomBlockPictureTable blocks = SeededPictures(random, scene);
        const int partRows = 4;
        var parts = new List<ShownBossBodyPart>();
        for (int top = 0; top < rows; top += partRows)
        {
            LayeredPicture picture = blocks.Picture(random.Next(Definitions))
                ?? throw new InvalidOperationException("A seeded block has no picture.");
            Tile(parts, picture, CellCorner(top, 0, area), BodyColumns * BodyCellPixels, Math.Min(partRows, rows - top) * BodyCellPixels);
        }
        return new ShownBossBody([.. parts]);
    }

    /// <summary>
    /// The boss body of <paramref name="area"/> pixels a fixture loaded as a tilemap at <paramref name="tilemapWord"/> in
    /// <paramref name="image"/>, one word for each eight-pixel cell in the hardware's screen order (#1403, #1466, #1540,
    /// #1601): for each distinct word a picture, a block of four copies of the word painted from the characters at
    /// <paramref name="characterWord"/>, tiled as parts over each run of cells showing the word.
    /// </summary>
    internal static ShownBossBody BodyFromTilemap(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int tilemapWord,
        Extent16<BackgroundPixels> area, int characterWord = 0)
    {
        var words = new ushort[area.X / BodyCellPixels * (area.Y / BodyCellPixels)];
        for (int cell = 0; cell < words.Length; cell++)
            words[cell] = BinaryPrimitives.ReadUInt16LittleEndian(image.Slice(SnesVramAddressingConstants.ByteOffsetOfWord(tilemapWord + cell), sizeof(ushort)));
        ushort[] distinct = [.. words.Distinct()];
        // Each distinct word painted as a block of four copies of itself; its top-left quarter pictures the word.
        var definitions = new BackgroundTile[distinct.Length * BlockTiles];
        for (int index = 0; index < distinct.Length; index++)
        for (int corner = 0; corner < BlockTiles; corner++)
            definitions[index * BlockTiles + corner] = BackgroundTileWordBytes.Tile(distinct[index]);
        SpriteSheetFrame[] painted = RoomBlockFixtures.WholePictures(definitions, image, palette, characterWord);
        var parts = new List<ShownBossBodyPart>();
        for (int cell = 0; cell < words.Length;)
        {
            // Each run of a row's cells showing one word shows that word's picture tiled across it; every quarter of the
            // picture shows the word.
            int first = cell, row = cell / BodyColumns;
            while (cell < words.Length && cell / BodyColumns == row && words[cell] == words[first])
                cell++;
            var picture = new LayeredPicture(painted[Array.IndexOf(distinct, words[first])], InFront: null);
            Tile(parts, picture, CellCorner(row, first % BodyColumns, area), (cell - first) * BodyCellPixels, BodyCellPixels);
        }
        return new ShownBossBody([.. parts]);
    }

    /// <summary>
    /// A boss body of <paramref name="area"/> pixels that shows tilemap word <paramref name="word"/> in every cell (#1403,
    /// #1466, #1540, #1601): a block of four copies of the word painted from the characters at
    /// <paramref name="characterWord"/> in <paramref name="image"/>, tiled as parts over the whole body.
    /// </summary>
    internal static ShownBossBody UniformBody(SnesBgTilemapWord word, Extent16<BackgroundPixels> area, ReadOnlySpan<byte> image,
        ReadOnlySpan<RgbColor> palette, int characterWord = 0)
    {
        var definition = new BackgroundTile[BlockTiles];
        definition.AsSpan().Fill(BackgroundTileWordBytes.Tile(word.Raw));
        var block = new LayeredPicture(RoomBlockFixtures.WholePictures(definition, image, palette, characterWord)[0], InFront: null);
        var parts = new List<ShownBossBodyPart>();
        Tile(parts, block, default, area.X, area.Y);
        return new ShownBossBody([.. parts]);
    }

    /// <summary>Cells across one screen of a fixture's body tilemap, the cells of a row in reading order.</summary>
    private const int BodyColumns = 32;

    /// <summary>Pixels across and down one cell of a fixture's body tilemap.</summary>
    private const int BodyCellPixels = 8;

    /// <summary>
    /// The body pixel the top-left corner of cell <paramref name="column"/> of tilemap row <paramref name="row"/> lies on: the
    /// tilemap's screens of rows lie left to right and then downward across <paramref name="area"/>.
    /// </summary>
    private static Position16<BackgroundPixels> CellCorner(int row, int column, Extent16<BackgroundPixels> area)
    {
        const int screenPixels = BodyColumns * BodyCellPixels;
        int screen = row / BodyColumns, screensAcross = area.X / screenPixels;
        return new Position16<BackgroundPixels>(screen % screensAcross * screenPixels + column * BodyCellPixels,
            screen / screensAcross * screenPixels + row % BodyColumns * BodyCellPixels);
    }

    /// <summary>
    /// Adds the parts that tile <paramref name="picture"/> over the <paramref name="width"/> by <paramref name="height"/>
    /// pixels from <paramref name="corner"/>, from the picture's top-left corner, each cut where the rectangle ends: at each
    /// place its part drawn behind the sprites, then its part drawn in front of them (#1634), which share no pixel. A picture
    /// showing nothing adds no part.
    /// </summary>
    private static void Tile(List<ShownBossBodyPart> parts, LayeredPicture picture, Position16<BackgroundPixels> corner, int width, int height)
    {
        if (picture.Size is not { } size)
            return;
        for (int y = 0; y < height; y += size.Y)
        for (int x = 0; x < width; x += size.X)
        {
            var at = new Position16<BackgroundPixels>(corner.X + x, corner.Y + y);
            var shown = new Extent16<BackgroundPixels>(Math.Min(size.X, width - x), Math.Min(size.Y, height - y));
            if (picture.Behind is { } behind)
                parts.Add(new ShownBossBodyPart(behind, at, shown, BossBodyDepth.BehindSprites));
            if (picture.InFront is { } inFront)
                parts.Add(new ShownBossBodyPart(inFront, at, shown, BossBodyDepth.InFrontOfSprites));
        }
    }

    /// <summary>
    /// A seeded run of words written over a background (#1392, #1403), drawn from seeded pictures as
    /// <see cref="SeededPictures"/> seeds them; a run position it leaves without a picture shows the background.
    /// </summary>
    internal static RoomOverlayPictures SeededPicturedOverlay(Random random, RgbColor[] scene) =>
        SeededOverlayRun(random, scene, random.Next(64, 256));

    /// <summary>
    /// <paramref name="count"/> seeded block pictures written over a background, as <see cref="SeededPicturedOverlay"/> seeds
    /// them, each at a seeded cell of the background's first two screens (#1514).
    /// </summary>
    private static RoomOverlayPictures SeededOverlayRun(Random random, RgbColor[] scene, int count)
    {
        RoomBlockPictureTable blocks = SeededPictures(random, scene);
        var pictures = System.Collections.Immutable.ImmutableArray.CreateBuilder<PlacedWrittenPicture>(count);
        for (int index = 0; index < count; index++)
        {
            pictures.Add(new PlacedWrittenPicture(blocks.Picture(random.Next(Definitions))
                ?? throw new InvalidOperationException("A seeded block has no picture."),
                new Position16<PlanePixels>(random.Next(64) * 8, random.Next(32) * 8)));
        }
        return new RoomOverlayPictures(pictures.MoveToImmutable());
    }

    /// <summary>
    /// An effect overlay atmosphere of a seeded two-bit picture (#1392, #1403, #1514, #1706) as wide as the overlay and as
    /// tall as its whole plane, its pixels keeping colour zero's row, even 16-colour rows coloured as <paramref name="scene"/>
    /// is and odd ones differently, with a seeded part behind and a seeded part in front (#1634).
    /// </summary>
    internal static FxOverlayArtwork SeededFxOverlay(Random random, RgbColor[] scene)
    {
        var size = new Extent16<ImagePixels>(FxOverlayPlaneConstants.Columns * 8, FxOverlayPlaneConstants.Rows * 8);
        SpriteSheet sheet = SeededSheet(random, scene, new Extent16<ImagePixels>(size.X, checked((ushort)(2 * size.Y))),
            new Dictionary<string, Position16<ImagePixels>> { ["overlay"] = default, ["overlay in front"] = new(0, size.Y) }, size,
            "seeded overlay", SpriteImageColours.TwoBppBackground);
        return new FxOverlayArtwork(new AtmosphereArtwork(new LayeredPicture(sheet.Frame("overlay"), sheet.Frame("overlay in front"))));
    }

    /// <summary>
    /// A seeded two-bit image of a message of <paramref name="rows"/> rows (#1392), its pixels keeping colour zero's row,
    /// some of them the two colours a message box installs.
    /// </summary>
    internal static SpriteSheetFrame SeededMessagePicture(Random random, RgbColor[] scene, int rows)
    {
        var size = new Extent16<ImagePixels>(256, (ushort)(rows * 8));
        return SeededSheet(random, scene, size, new Dictionary<string, Position16<ImagePixels>> { ["message"] = default }, size,
            "seeded message", SpriteImageColours.TwoBppBackground).Frame("message");
    }

    /// <summary>A seeded image of the scrolling sky's rows, with rebound cells as <see cref="SeededBackground"/> has (#1392).</summary>
    internal static RoomBackgroundImagePlane SeededSky(Random random, RgbColor[] scene) =>
        SeededImage(random, scene, SuperMetroid.AssetExtraction.Rooms.SkyTilemap.Rows, sky: true);

    /// <summary>
    /// A seeded background image of <paramref name="tileRows"/> 32-tile rows, about one cell in four rebound: in one column for
    /// a sky's rows, otherwise two pages side by side, as the import lays a library background out (#1617). Its picture's
    /// part behind and part in front are seeded apart (#1634).
    /// </summary>
    private static RoomBackgroundImagePlane SeededImage(Random random, RgbColor[] scene, int tileRows, bool sky = false)
    {
        RoomBlockPictureTable blocks = SeededPictures(random, scene);
        Extent16<ImagePixels> size = sky
            ? new(RoomBackgroundImageGeometryConstants.PagePixels, (ushort)(tileRows * 8))
            : new(RoomBackgroundImageGeometry.Pages * RoomBackgroundImageGeometryConstants.PagePixels, (ushort)(tileRows * 8 / RoomBackgroundImageGeometry.Pages));
        SpriteSheet sheet = SeededSheet(random, scene, new Extent16<ImagePixels>(size.X, checked((ushort)(2 * size.Y))),
            new Dictionary<string, Position16<ImagePixels>>
            {
                [RoomBackgroundImage.FrameName] = default,
                [PictureParts.InFront(RoomBackgroundImage.FrameName)] = new(0, size.Y),
            }, size, "seeded background");
        var image = new RoomBackgroundImage("seeded background", sheet, size, []);
        var cells = new LayeredPicture?[image.CellCount];
        for (int cell = 0; cell < cells.Length; cell++)
            cells[cell] = random.Next(4) == 0 ? blocks.Picture(random.Next(Definitions)) : null;
        return new RoomBackgroundImagePlane(image, cells, RoomBackgroundImagePlacement.Whole(image));
    }

    /// <summary>Block pictures on one seeded sheet.</summary>
    private const int SeededPictureCount = 16;

    /// <summary>
    /// A seeded sheet of block pictures, some pixels transparent and in every row, and a table showing one of them for each
    /// definition behind and, for about half the definitions, another in front (#1634). Even rows' standard colours are
    /// <paramref name="scene"/>'s, odd rows' differ, so a renderer shows the pictures' own colours for some rows and the
    /// scene's for others.
    /// </summary>
    private static RoomBlockPictureTable SeededPictures(Random random, RgbColor[] scene)
    {
        int side = RoomBlockSheet.BlockPixels, columns = 4;
        var size = new Extent16<ImagePixels>((ushort)(columns * side), (ushort)(SeededPictureCount / columns * side));
        var corners = new Dictionary<string, Position16<ImagePixels>>();
        for (int picture = 0; picture < SeededPictureCount; picture++)
            corners.Add($"{picture}", new Position16<ImagePixels>((ushort)(picture % columns * side), (ushort)(picture / columns * side)));
        SpriteSheet sheet = SeededSheet(random, scene, size, corners, new Extent16<ImagePixels>((ushort)side, (ushort)side), "seeded blocks");
        var shown = new LayeredPicture?[Definitions];
        for (int definition = 0; definition < shown.Length; definition++)
        {
            SpriteSheetFrame behind = sheet.Frame($"{random.Next(SeededPictureCount)}");
            shown[definition] = new LayeredPicture(behind, random.Next(2) == 0 ? sheet.Frame($"{random.Next(SeededPictureCount)}") : null);
        }
        return new RoomBlockPictureTable(shown);
    }

    /// <summary>
    /// A seeded background-coloured sheet of <paramref name="size"/>: some pixels transparent and in every row, even rows'
    /// standard colours <paramref name="scene"/>'s and odd rows' different, with a
    /// <paramref name="frameSize"/> frame at each of <paramref name="corners"/>.
    /// </summary>
    private static SpriteSheet SeededSheet(Random random, RgbColor[] scene, Extent16<ImagePixels> size,
        Dictionary<string, Position16<ImagePixels>> corners, Extent16<ImagePixels> frameSize, string name,
        SpriteImageColours kind = SpriteImageColours.Background)
    {
        bool twoBpp = kind == SpriteImageColours.TwoBppBackground;
        bool fullPalette = kind == SpriteImageColours.FullPalette;
        var standard = new ushort[ImageColourGroup.ColourCountOf(kind)];
        for (int colour = 0; colour < standard.Length; colour++)
            standard[colour] = colour / 16 % 2 == 0 ? scene[colour].ToWord() : (ushort)random.Next(32768);
        int pixels = size.X * size.Y;
        var image = new RgbaPixel[pixels];
        var indices = new byte[pixels];
        for (int pixel = 0; pixel < pixels; pixel++)
        {
            // A two-bit pixel keeps its row times four plus its colour, colour zero included and transparent.
            if (twoBpp)
            {
                int twoBppRow = random.Next(8), colour = random.Next(4);
                indices[pixel] = (byte)(twoBppRow * 4 + colour);
                if (colour == 0)
                    continue;
                image[pixel] = RgbColor.FromWord(standard[indices[pixel]]).ToRgbaPixel();
                continue;
            }
            if (random.Next(3) == 0)
                continue;
            // A full-palette pixel names any of the 256 colours.
            int row = random.Next(fullPalette ? 16 : 8);
            indices[pixel] = (byte)(row * 16 + random.Next(1, 16));
            image[pixel] = RgbColor.FromWord(standard[indices[pixel]]).ToRgbaPixel();
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>();
        foreach ((string frame, Position16<ImagePixels> corner) in corners)
        {
            ImageColourGroups groups = ImageColourGroups.None;
            for (int y = 0; y < frameSize.Y; y++)
            for (int x = 0; x < frameSize.X; x++)
            {
                byte index = indices[(corner.Y + y) * size.X + corner.X + x];
                if (index % (twoBpp ? 4 : 16) != 0 && index / 16 < 8)
                    groups = groups.With(ImageColourGroup.OfInk(index));
            }
            frames.Add(frame, new SpriteSheetFrameDocument { Corner = corner, Size = frameSize, Origin = default, ColourGroups = groups.ManifestMembers });
        }
        var document = new SpriteSheetDocument
        {
            Palette = kind,
            Colours = standard,
            // A room's seeded sheets are gameplay background images, whose groups the import pipeline names in the
            // cartridge's set order.
            BackgroundSets = [.. InstalledPictureColours.CartridgeOrderSets],
            // A seeded block sheet stands for a room's tileset, which names its scene's backdrop (#1544).
            Backdrop = 0,
            Frames = frames,
        };
        return SpriteSheetFileConstants.Load(document, name, size, image, indices);
    }

    /// <summary>
    /// A room of random blocks, with a random BG3 plane, each plane lying at the layer's origin. Optionally adds a door's
    /// source screen, lying near the foreground's view, and words written over BG1.
    /// </summary>
    /// <param name="random">The seeded source.</param>
    /// <param name="foregroundScroll">The foreground's view the room is shown through.</param>
    /// <param name="backgroundScroll">The background's view, or null for a seeded library background image in place of a BG2 plane.</param>
    /// <param name="door">Whether a door's source screen covers part of BG1.</param>
    /// <param name="overlay">Whether words are written over part of BG1.</param>
    /// <param name="pictureColours">
    /// The scene's 256 CGRAM colours the planes' seeded block pictures (#1392) keep as standard in their even rows, odd rows
    /// differing as under a palette effect; null for black.
    /// </param>
    internal static RoomBackgrounds Seeded(Random random, Position16<PlanePixels> foregroundScroll,
        Position16<PlanePixels>? backgroundScroll = null, bool door = false, bool overlay = false, RgbColor[]? pictureColours = null)
    {
        RoomBackgroundLayer Layer()
        {
            // Each draw is a seeded sixteen-bit value: its low ten bits name the block and the next two its mirroring.
            var blocks = new RoomBlockAppearance[SideBlocks * SideBlocks];
            for (int index = 0; index < blocks.Length; index++)
            {
                int draw = random.Next(ushort.MaxValue + 1);
                blocks[index] = RoomBlockAppearance.Of(draw & RoomBlockAppearance.LastBlock,
                    ((draw & (1 << 10)) != 0 ? BlockMirroring.Horizontal : BlockMirroring.None) |
                    ((draw & (1 << 11)) != 0 ? BlockMirroring.Vertical : BlockMirroring.None));
            }
            return new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(SideBlocks, SideBlocks), blocks).Cells,
                SeededPictures(random, pictureColours ?? new RgbColor[SnesCgram.ColorCount]));
        }
        RoomBackgroundLayer bg1 = Layer();
        RoomBackgroundLayer? bg2 = backgroundScroll is not null ? Layer() : null;
        RoomDoorBackground? source = null;
        if (door)
        {
            RoomBackgroundLayer sourceRoom = Layer();
            var sourceView = new Position16<PlanePixels>((ushort)random.Next(SideBlocks * RoomLevelPlane.BlockPixels / 2),
                (ushort)random.Next(SideBlocks * RoomLevelPlane.BlockPixels / 2));
            source = RoomDoorBackground.Begin(sourceRoom, sourceView,
                foregroundScroll + new Offset16<PlanePixels>((short)random.Next(-128, 128), 0), random.Next(2) == 0);
        }
        RgbColor[] scene = pictureColours ?? new RgbColor[SnesCgram.ColorCount];
        RoomOverlayPictures? written = overlay ? SeededOverlayRun(random, scene, random.Next(8, 24)) : null;
        return new RoomBackgrounds(bg1, bg2, SeededFxOverlay(random, scene), source, written,
            BackgroundImage: bg2 is null ? SeededBackground(random, scene) : null);
    }
}
