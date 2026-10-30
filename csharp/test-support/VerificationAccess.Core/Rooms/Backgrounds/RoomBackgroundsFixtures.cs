using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

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

    /// <summary>A room whose BG1 shows tile word zero over the whole unscrolled screen, no BG2 plane, and a zero BG3 plane.</summary>
    internal static RoomBackgrounds Blank() => Uniform(new byte[SnesVram.ByteCount], new RgbColor[SnesCgram.ColorCount], default);

    /// <summary>A BG3 plane of zero words, as a blank memory image shows it (#1403).</summary>
    internal static Layer3Cells BlankBg3() => Layer3CellsFixtures.Blank;

    /// <summary>A Ceres room's turning floor, with its floor animation frames, whose drawing a check does not look at (#1467).</summary>
    internal static RoomFloor CeresFloor() => new(AffineFloorConstants.BlankCeresRoom());

    /// <summary>A turning floor of tile zero everywhere, showing nothing.</summary>
    internal static AffineFloor BlankMode7Floor() => AffineFloorConstants.BlankCeresRoom().Floor;

    /// <summary>
    /// A room whose BG1 shows <paramref name="tile"/> in every cell over the whole unscrolled screen, as a VRAM BG1
    /// tilemap filled with that word did, except the listed eight-pixel <paramref name="cells"/>; no BG2 plane; and a zero
    /// BG3 plane. Its blocks draw pictures of the characters at the start of <paramref name="image"/>, coloured from
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
            blocks).Cells, default, RoomBlockFixtures.Pictures(definitions, image, palette)), null, BlankBg3(),
            Bg2Image: RoomBlockFixtures.LoadedBackground(image, palette, GameplayVramLayout.Bg2TilemapWord));
    }

    /// <summary>
    /// A seeded X-ray reveal over seeded block pictures coloured as <paramref name="scene"/>'s planes would be (#1392):
    /// random definitions in every arrangement.
    /// </summary>
    internal static SuperMetroid.Core.Samus.Abilities.XrayRevealPlane SeededReveal(Random random, RgbColor[] scene)
    {
        RoomBlockPictureTable pictures = SeededPictures(random, scene);
        RevealedBlockArrangement[] arrangements = Enum.GetValues<RevealedBlockArrangement>();
        var blocks = new RevealedBlock?[SuperMetroid.Core.Samus.Abilities.XrayRevealLayout.Cells];
        for (int block = 0; block < blocks.Length; block++)
            blocks[block] = new RevealedBlock(random.Next(Definitions), arrangements[random.Next(arrangements.Length)]);
        return new SuperMetroid.Core.Samus.Abilities.XrayRevealPlane(blocks, pictures);
    }

    /// <summary>
    /// A seeded library background image over seeded colours as <see cref="SeededPictures"/> colours them, with about one
    /// cell in four drawn from a seeded block picture in its place (#1392).
    /// </summary>
    internal static RoomBackgroundImagePlane SeededBackground(Random random, RgbColor[] scene) =>
        SeededImage(random, scene, RoomBackgroundImage.Words / RoomBackgroundImage.PageTiles);

    /// <summary>Seeded pictures for <paramref name="words"/> tile words (#1392): each a quarter of a seeded block picture.</summary>
    internal static System.Collections.Immutable.ImmutableArray<BgTilePicture?> SeededBodyPictures(Random random, RgbColor[] scene, int words)
    {
        RoomBlockPictureTable blocks = SeededPictures(random, scene);
        var pictures = new BgTilePicture?[words];
        for (int word = 0; word < words; word++)
        {
            pictures[word] = new BgTilePicture(blocks.Picture(random.Next(Definitions))
                ?? throw new InvalidOperationException("A seeded block has no picture."),
                new Position16<ImagePixels>((ushort)(random.Next(2) * 8), (ushort)(random.Next(2) * 8)));
        }
        return [.. pictures];
    }

    /// <summary>
    /// A seeded boss body of <paramref name="rows"/> rows (#1466): every four rows one part, showing a seeded block
    /// picture repeated across them.
    /// </summary>
    internal static ShownBossBody SeededBody(Random random, RgbColor[] scene, int rows)
    {
        RoomBlockPictureTable blocks = SeededPictures(random, scene);
        const int partRows = 4;
        var parts = new List<ShownBossBodyPart>();
        for (int top = 0; top < rows; top += partRows)
        {
            parts.Add(new ShownBossBodyPart(blocks.Picture(random.Next(Definitions))
                ?? throw new InvalidOperationException("A seeded block has no picture."), top, BossBodyFootprint.OfRows(top, Math.Min(partRows, rows - top))));
        }
        return new ShownBossBody(rows, [.. parts]);
    }

    /// <summary>
    /// The boss body a fixture loaded as a <paramref name="size"/> tilemap at <paramref name="tilemapWord"/> in
    /// <paramref name="image"/>, in the hardware's screen order (#1403, #1466): one part for each distinct word, its
    /// picture a block of four copies of the word painted from the characters at <paramref name="characterWord"/>,
    /// repeated over the cells showing the word.
    /// </summary>
    internal static ShownBossBody BodyFromTilemap(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int tilemapWord,
        Extent16<TilemapCells> size, int characterWord = 0)
    {
        var words = new ushort[size.X * size.Y];
        for (int cell = 0; cell < words.Length; cell++)
            words[cell] = BinaryPrimitives.ReadUInt16LittleEndian(image.Slice(SnesVramAddressingConstants.ByteOffsetOfWord(tilemapWord + cell), sizeof(ushort)));
        ushort[] distinct = [.. words.Distinct()];
        // Each distinct word painted as a block of four copies of itself; its top-left quarter pictures the word.
        var definitions = new BackgroundTile[distinct.Length * BlockTiles];
        for (int index = 0; index < distinct.Length; index++)
        for (int corner = 0; corner < BlockTiles; corner++)
            definitions[index * BlockTiles + corner] = BackgroundTileWordBytes.Tile(distinct[index]);
        RoomBlockPictureTable painted = RoomBlockFixtures.Pictures(definitions, image, palette, characterWord);
        var parts = new List<ShownBossBodyPart>();
        for (int index = 0; index < distinct.Length; index++)
        {
            var masks = new uint[words.Length / BossBodyDefinitions.ColumnCount];
            for (int cell = 0; cell < words.Length; cell++)
            {
                if (words[cell] == distinct[index])
                    masks[cell / BossBodyDefinitions.ColumnCount] |= 1u << (cell % BossBodyDefinitions.ColumnCount);
            }
            parts.Add(new ShownBossBodyPart(painted.Picture(index) ?? throw new InvalidOperationException("A painted word has no picture."), 0,
                new BossBodyFootprint(0, [.. masks])));
        }
        return new ShownBossBody(words.Length / BossBodyDefinitions.ColumnCount, [.. parts]);
    }

    /// <summary>
    /// A boss body of <paramref name="size"/> that shows tilemap word <paramref name="word"/> in every cell (#1403, #1466):
    /// one part, a block of four copies of the word painted from the characters at <paramref name="characterWord"/> in
    /// <paramref name="image"/>, repeated over every cell.
    /// </summary>
    internal static ShownBossBody UniformBody(SnesBgTilemapWord word, Extent16<TilemapCells> size, ReadOnlySpan<byte> image,
        ReadOnlySpan<RgbColor> palette, int characterWord = 0)
    {
        var definition = new BackgroundTile[BlockTiles];
        definition.AsSpan().Fill(BackgroundTileWordBytes.Tile(word.Raw));
        SpriteSheetFrame block = RoomBlockFixtures.Pictures(definition, image, palette, characterWord).Picture(0)
            ?? throw new InvalidOperationException("The painted block has no picture.");
        int rows = size.X * size.Y / BossBodyDefinitions.ColumnCount;
        return new ShownBossBody(rows, [new ShownBossBodyPart(block, 0, BossBodyFootprint.OfRows(0, rows))]);
    }

    /// <summary>
    /// A seeded run of words written over a background (#1392, #1403), drawn from seeded pictures as
    /// <see cref="SeededBodyPictures"/> seeds them; a run position it leaves without a picture shows the background.
    /// </summary>
    internal static RoomOverlayPictures SeededPicturedOverlay(Random random, RgbColor[] scene) =>
        SeededOverlayRun(random, scene, random.Next(64, 256));

    /// <summary>A seeded run of <paramref name="count"/> words written over a background, as <see cref="SeededPicturedOverlay"/> seeds them.</summary>
    private static RoomOverlayPictures SeededOverlayRun(Random random, RgbColor[] scene, int count)
    {
        int first = random.Next(RoomWrittenOverlay.CellCount - count);
        System.Collections.Immutable.ImmutableArray<BgTilePicture?> written = SeededBodyPictures(random, scene, count);
        var pictures = new BgTilePicture?[RoomWrittenOverlay.CellCount];
        for (int index = 0; index < count; index++)
            pictures[first + index] = random.Next(8) == 0 ? null : written[index];
        return new RoomOverlayPictures([.. pictures]);
    }

    /// <summary>
    /// A BG3 plane of seeded two-bit pictures (#1392, #1403): each cell a quarter of a seeded 16x16 picture whose pixels
    /// keep colour zero's row, even 16-colour rows coloured as <paramref name="scene"/> is and odd ones differently.
    /// </summary>
    internal static Layer3Cells SeededBg3Pictures(Random random, RgbColor[] scene)
    {
        int side = RoomBlockSheet.BlockPixels, columns = 4;
        var size = new Extent16<ImagePixels>((ushort)(columns * side), (ushort)(SeededPictureCount / columns * side));
        var corners = new Dictionary<string, Position16<ImagePixels>>();
        for (int picture = 0; picture < SeededPictureCount; picture++)
            corners.Add($"{picture}", new Position16<ImagePixels>((ushort)(picture % columns * side), (ushort)(picture / columns * side)));
        SpriteSheet sheet = SeededSheet(random, scene, size, corners, new Extent16<ImagePixels>((ushort)side, (ushort)side), "seeded BG3",
            SpriteImageColours.TwoBppBackground);
        var pictures = new BgTilePicture?[Layer3Plane.Columns * Layer3Plane.Rows];
        for (int word = 0; word < pictures.Length; word++)
        {
            pictures[word] = new BgTilePicture(sheet.Frame($"{random.Next(SeededPictureCount)}"),
                new Position16<ImagePixels>((ushort)(random.Next(2) * 8), (ushort)(random.Next(2) * 8)));
        }
        return new Layer3Cells([.. pictures]);
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
        SeededImage(random, scene, SuperMetroid.AssetExtraction.Rooms.SkyTilemap.Rows);

    /// <summary>A seeded background image <paramref name="tileRows"/> 32-tile rows tall, about one cell in four rebound.</summary>
    private static RoomBackgroundImagePlane SeededImage(Random random, RgbColor[] scene, int tileRows)
    {
        RoomBlockPictureTable blocks = SeededPictures(random, scene);
        var size = new Extent16<ImagePixels>(RoomBackgroundImage.PagePixels, (ushort)(tileRows * 8));
        SpriteSheet sheet = SeededSheet(random, scene, size, new Dictionary<string, Position16<ImagePixels>> { [RoomBackgroundImage.FrameName] = default },
            size, "seeded background");
        var image = new RoomBackgroundImage("seeded background", sheet, []);
        var cells = new SpriteSheetFrame?[image.CellCount];
        for (int cell = 0; cell < cells.Length; cell++)
            cells[cell] = random.Next(4) == 0 ? blocks.Picture(random.Next(Definitions)) : null;
        return new RoomBackgroundImagePlane(image, cells);
    }

    /// <summary>Block pictures on one seeded sheet.</summary>
    private const int SeededPictureCount = 16;

    /// <summary>
    /// A seeded sheet of block pictures, some pixels transparent, of either tile priority and in every row, and a table
    /// showing one of them for each definition. Even rows' standard colours are <paramref name="scene"/>'s, odd rows'
    /// differ, so a renderer shows the pictures' own colours for some rows and the scene's for others.
    /// </summary>
    private static RoomBlockPictureTable SeededPictures(Random random, RgbColor[] scene)
    {
        int side = RoomBlockSheet.BlockPixels, columns = 4;
        var size = new Extent16<ImagePixels>((ushort)(columns * side), (ushort)(SeededPictureCount / columns * side));
        var corners = new Dictionary<string, Position16<ImagePixels>>();
        for (int picture = 0; picture < SeededPictureCount; picture++)
            corners.Add($"{picture}", new Position16<ImagePixels>((ushort)(picture % columns * side), (ushort)(picture / columns * side)));
        SpriteSheet sheet = SeededSheet(random, scene, size, corners, new Extent16<ImagePixels>((ushort)side, (ushort)side), "seeded blocks");
        var shown = new SpriteSheetFrame?[Definitions];
        for (int definition = 0; definition < shown.Length; definition++)
            shown[definition] = sheet.Frame($"{random.Next(SeededPictureCount)}");
        return new RoomBlockPictureTable(new RoomBlockSheet(default(TilesetDefinition), sheet, RoomBlockFixtures.Appearances(Definitions)), shown);
    }

    /// <summary>
    /// A seeded background-coloured sheet of <paramref name="size"/>: some pixels transparent, of either tile priority and in
    /// every row, even rows' standard colours <paramref name="scene"/>'s and odd rows' different, with a
    /// <paramref name="frameSize"/> frame at each of <paramref name="corners"/>.
    /// </summary>
    private static SpriteSheet SeededSheet(Random random, RgbColor[] scene, Extent16<ImagePixels> size,
        Dictionary<string, Position16<ImagePixels>> corners, Extent16<ImagePixels> frameSize, string name,
        SpriteImageColours kind = SpriteImageColours.Background)
    {
        bool twoBpp = kind == SpriteImageColours.TwoBppBackground;
        bool fullPalette = kind == SpriteImageColours.FullPalette;
        var standard = new ushort[SpriteImage.ColourCountOf(kind)];
        for (int colour = 0; colour < standard.Length; colour++)
            standard[colour] = colour / 16 % 2 == 0 ? scene[colour].ToWord() : (ushort)random.Next(32768);
        int pixels = size.X * size.Y;
        var image = new RgbaPixel[pixels];
        var indices = new byte[pixels];
        var priorities = new byte[pixels];
        var partGroups = new byte[pixels];
        for (int pixel = 0; pixel < pixels; pixel++)
        {
            // A two-bit pixel keeps its row times four plus its colour, colour zero included and transparent.
            if (twoBpp)
            {
                int twoBppRow = random.Next(8), colour = random.Next(4);
                indices[pixel] = (byte)(twoBppRow * 4 + colour);
                priorities[pixel] = (byte)random.Next(2);
                if (colour == 0)
                    continue;
                partGroups[pixel] = (byte)PartColourGroup.Keeping(ImageColourGroup.OfInk(indices[pixel])).Code;
                image[pixel] = RgbColor.FromWord(standard[indices[pixel]]).ToRgbaPixel();
                continue;
            }
            if (random.Next(3) == 0)
                continue;
            // A full-palette pixel names any of the 256 colours and has no part group.
            int row = random.Next(fullPalette ? 16 : 8);
            indices[pixel] = (byte)(row * 16 + random.Next(1, 16));
            priorities[pixel] = fullPalette ? (byte)0 : (byte)random.Next(2);
            partGroups[pixel] = fullPalette ? (byte)0 : (byte)PartColourGroup.Keeping(ImageColourGroup.Numbered(row)).Code;
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
            frames.Add(frame, new SpriteSheetFrameDocument { Corner = corner, Size = frameSize, Origin = default, ColourGroups = groups.ManifestMembers, Parts = [] });
        }
        var document = new SpriteSheetDocument
        {
            Version = SpriteSheet.Version,
            Palette = kind,
            Colours = standard,
            // A room's seeded sheets are gameplay background images, whose groups the import pipeline names in the
            // cartridge's set order.
            BackgroundSets = [.. SuperMetroid.AssetExtraction.Video.BackgroundColourSetImage.ImageGroups],
            Frames = frames,
            PartImages = [],
        };
        return SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(document), writable: false), name, size, image,
            indices, priorities, partGroups);
    }

    /// <summary>
    /// A room of random blocks, with a random BG3 plane. Each plane's offset places a random point of the room at the
    /// given scroll, so the screen it is scrolled to shows mostly room and some edge. Optionally adds a door's source
    /// screen and words written over BG1.
    /// </summary>
    /// <param name="random">The seeded source.</param>
    /// <param name="bg1Scroll">The BG1 scroll registers the room is shown through.</param>
    /// <param name="bg2Scroll">The BG2 scroll registers, or null for a seeded library background image in place of a BG2 plane.</param>
    /// <param name="door">Whether a door's source screen covers part of BG1.</param>
    /// <param name="overlay">Whether words are written over part of BG1.</param>
    /// <param name="pictureColours">
    /// The scene's 256 CGRAM colours the planes' seeded block pictures (#1392) keep as standard in their even rows, odd rows
    /// differing as under a palette effect; null for black.
    /// </param>
    internal static RoomBackgrounds Seeded(Random random, Position16<TilePlanePixels> bg1Scroll,
        Position16<TilePlanePixels>? bg2Scroll = null, bool door = false, bool overlay = false, RgbColor[]? pictureColours = null)
    {
        RoomBackgroundLayer Layer(Position16<TilePlanePixels> scroll)
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
            var inside = new Position16<TilePlanePixels>((ushort)random.Next(SideBlocks * RoomLevelPlane.BlockPixels / 2),
                (ushort)random.Next(SideBlocks * RoomLevelPlane.BlockPixels / 2));
            return new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(SideBlocks, SideBlocks), blocks).Cells,
                scroll - inside, SeededPictures(random, pictureColours ?? new RgbColor[SnesCgram.ColorCount]));
        }
        RoomBackgroundLayer bg1 = Layer(bg1Scroll);
        RoomBackgroundLayer? bg2 = bg2Scroll is { } scroll2 ? Layer(scroll2) : null;
        RoomDoorBackground? source = door
            ? RoomDoorBackground.Begin(Layer(bg1Scroll), bg1Scroll + new Offset16<TilePlanePixels>((short)random.Next(-128, 128), 0))
            : null;
        RgbColor[] scene = pictureColours ?? new RgbColor[SnesCgram.ColorCount];
        RoomOverlayPictures? written = overlay ? SeededOverlayRun(random, scene, random.Next(8, 24)) : null;
        return new RoomBackgrounds(bg1, bg2, SeededBg3Pictures(random, scene), source, written,
            Bg2Image: bg2 is null ? SeededBackground(random, scene) : null);
    }
}
