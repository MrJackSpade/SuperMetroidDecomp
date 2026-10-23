using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rendering;
using System.Collections.Immutable;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
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

    /// <summary>Block definitions a fixture room loads: every ten-bit visual index.</summary>
    private const int Definitions = 1024;

    /// <summary>A room whose BG1 shows tile word zero over the whole unscrolled screen, no BG2 plane, and a zero BG3 plane.</summary>
    internal static RoomBackgrounds Blank() => Uniform(new byte[SnesPpuLayout.VramByteCount], new Bgr555[SnesPpuLayout.CgramColorCount], default);

    /// <summary>A BG3 plane of zero words, as a blank memory image shows it (#1403).</summary>
    internal static Layer3Cells BlankBg3() => Layer3CellsFixtures.Blank;

    /// <summary>Seeded Mode 7 tile pictures for a Ceres map whose drawing a check does not look at.</summary>
    internal static Mode7TileSheet Mode7Tiles() => SeededMode7Tiles(new Random(0), new Bgr555[SnesPpuLayout.CgramColorCount]);

    /// <summary>A Mode 7 floor of tile zero everywhere, showing nothing.</summary>
    internal static AffineFloor BlankMode7Floor() => new([.. new AffineFloorCell[AffineFloor.CellCount]], new Mode7Characters().Pictures());

    /// <summary>
    /// A room whose BG1 shows <paramref name="tile"/> in every cell over the whole unscrolled screen, as a VRAM BG1
    /// tilemap filled with that word did, except the listed eight-pixel <paramref name="cells"/>; no BG2 plane; and a zero
    /// BG3 plane. Its blocks draw pictures of the characters at the start of <paramref name="image"/>, coloured from
    /// <paramref name="palette"/> (#1403).
    /// </summary>
    internal static RoomBackgrounds Uniform(ReadOnlySpan<byte> image, ReadOnlySpan<Bgr555> palette, SnesBgTilemapWord tile,
        params ReadOnlySpan<TilemapCell> cells)
    {
        // Each block's four cells, top left, top right, bottom left, bottom right, as its definition stores them.
        var blockCells = new ushort[SideBlocks * SideBlocks][];
        for (int block = 0; block < blockCells.Length; block++)
            blockCells[block] = [tile.Raw, tile.Raw, tile.Raw, tile.Raw];
        foreach (var (column, row, cell) in cells)
            blockCells[row / 2 * SideBlocks + column / 2][row % 2 * 2 + column % 2] = cell.Raw;
        var definitionIndices = new Dictionary<BlockCellWords, ushort>();
        var definitions = new byte[Definitions * RoomLevelPlane.DefinitionBytes];
        var blocks = new ushort[blockCells.Length];
        for (int block = 0; block < blocks.Length; block++)
        {
            ushort[] words = blockCells[block];
            var key = new BlockCellWords(words[0], words[1], words[2], words[3]);
            if (!definitionIndices.TryGetValue(key, out ushort index))
            {
                index = (ushort)definitionIndices.Count;
                definitionIndices.Add(key, index);
                for (int corner = 0; corner < words.Length; corner++)
                    BinaryPrimitives.WriteUInt16LittleEndian(
                        definitions.AsSpan(index * RoomLevelPlane.DefinitionBytes + corner * 2), words[corner]);
            }
            blocks[block] = index;
        }
        return new(new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(SideBlocks, SideBlocks),
            blocks, [.. definitions]).Cells, default, RoomBlockFixtures.Pictures(definitions, image, palette)), null, BlankBg3());
    }

    /// <summary>
    /// A seeded X-ray reveal over seeded block pictures coloured as <paramref name="scene"/>'s planes would be (#1392):
    /// random definitions in every arrangement.
    /// </summary>
    internal static SuperMetroid.Core.Samus.Abilities.XrayRevealPlane SeededReveal(Random random, Bgr555[] scene)
    {
        RoomBlockPictureTable pictures = SeededPictures(random, scene);
        RevealedBlockArrangement[] arrangements = Enum.GetValues<RevealedBlockArrangement>();
        var blocks = new RevealedBlock[SuperMetroid.Core.Samus.Abilities.XrayTilemapLayout.MetatileColumns
            * SuperMetroid.Core.Samus.Abilities.XrayTilemapLayout.MetatileRows];
        for (int block = 0; block < blocks.Length; block++)
            blocks[block] = new RevealedBlock(random.Next(Definitions), arrangements[random.Next(arrangements.Length)]);
        return new SuperMetroid.Core.Samus.Abilities.XrayRevealPlane(blocks, pictures);
    }

    /// <summary>
    /// A seeded library background image over seeded colours as <see cref="SeededPictures"/> colours them, with about one
    /// cell in four drawn from a seeded block picture in its place (#1392).
    /// </summary>
    internal static RoomBackgroundImagePlane SeededBackground(Random random, Bgr555[] scene) =>
        SeededImage(random, scene, RoomBackgroundImage.Words / RoomBackgroundImage.PageTiles);

    /// <summary>
    /// Seeded pictures for <paramref name="words"/> boss body words (#1392): each a quarter of a seeded block picture, about
    /// one in eight drawing from its character instead.
    /// </summary>
    internal static System.Collections.Immutable.ImmutableArray<BgTilePicture?> SeededBodyPictures(Random random, Bgr555[] scene, int words)
    {
        RoomBlockPictureTable blocks = SeededPictures(random, scene);
        var pictures = new BgTilePicture?[words];
        for (int word = 0; word < words; word++)
        {
            pictures[word] = random.Next(8) == 0 ? null : new BgTilePicture(blocks.Picture(random.Next(Definitions))
                ?? throw new InvalidOperationException("A seeded block has no picture."),
                new Position16<ImagePixels>((ushort)(random.Next(2) * 8), (ushort)(random.Next(2) * 8)));
        }
        return [.. pictures];
    }

    /// <summary>
    /// A seeded overlay of words written over BG2, drawn from seeded pictures as <see cref="SeededBodyPictures"/> seeds them
    /// (#1392).
    /// </summary>
    internal static RoomTilemapOverlay SeededPicturedOverlay(Random random, Bgr555[] scene)
    {
        var words = new ushort[random.Next(64, 256)];
        for (int index = 0; index < words.Length; index++)
            words[index] = (ushort)random.Next(ushort.MaxValue + 1);
        RoomTilemapOverlay overlay = RoomTilemapOverlay.Empty.With(random.Next(RoomTilemapOverlay.WordCount - words.Length), words);
        return overlay.WithPictures(SeededBodyPictures(random, scene, RoomTilemapOverlay.WordCount));
    }

    /// <summary>
    /// A BG3 plane of seeded two-bit pictures (#1392, #1403): each cell a quarter of a seeded 16x16 picture whose pixels
    /// keep colour zero's row, even 16-colour rows coloured as <paramref name="scene"/> is and odd ones differently.
    /// </summary>
    internal static Layer3Cells SeededBg3Pictures(Random random, Bgr555[] scene)
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
    internal static SpriteSheetFrame SeededMessagePicture(Random random, Bgr555[] scene, int rows)
    {
        var size = new Extent16<ImagePixels>(256, (ushort)(rows * 8));
        return SeededSheet(random, scene, size, new Dictionary<string, Position16<ImagePixels>> { ["message"] = default }, size,
            "seeded message", SpriteImageColours.TwoBppBackground).Frame("message");
    }

    /// <summary>
    /// Seeded Mode 7 tile pictures (#1392): every tile's pixels seeded as <see cref="SeededPictures"/> seeds its sheets,
    /// even rows' standard colours <paramref name="scene"/>'s and odd rows' different.
    /// </summary>
    internal static Mode7TileSheet SeededMode7Tiles(Random random, Bgr555[] scene)
    {
        var size = new Extent16<ImagePixels>(Mode7TileSheet.TilesPerRow * Mode7TileSheet.TilePixels,
            Mode7TileSheet.Tiles / Mode7TileSheet.TilesPerRow * Mode7TileSheet.TilePixels);
        SpriteSheet sheet = SeededSheet(random, scene, size, new Dictionary<string, Position16<ImagePixels>> { [Mode7TileSheet.FrameName] = default },
            size, "seeded Mode 7 tiles", SpriteImageColours.FullPalette);
        return new Mode7TileSheet(default, sheet, [.. Enumerable.Repeat((BgArtworkShown?)default(BgArtworkShown), Mode7TileSheet.Tiles)]);
    }

    /// <summary>A seeded image of the scrolling sky's rows, with rebound cells as <see cref="SeededBackground"/> has (#1392).</summary>
    internal static RoomBackgroundImagePlane SeededSky(Random random, Bgr555[] scene) =>
        SeededImage(random, scene, SuperMetroid.Core.Rooms.Scrolling.SkyTilemap.Rows);

    /// <summary>A seeded background image <paramref name="tileRows"/> 32-tile rows tall, about one cell in four rebound.</summary>
    private static RoomBackgroundImagePlane SeededImage(Random random, Bgr555[] scene, int tileRows)
    {
        RoomBlockPictureTable blocks = SeededPictures(random, scene);
        var size = new Extent16<ImagePixels>(RoomBackgroundImage.PagePixels, (ushort)(tileRows * 8));
        SpriteSheet sheet = SeededSheet(random, scene, size, new Dictionary<string, Position16<ImagePixels>> { [RoomBackgroundImage.FrameName] = default },
            size, "seeded background");
        ushort[] words = [.. Enumerable.Range(0, tileRows * RoomBackgroundImage.PageTiles).Select(_ => (ushort)random.Next(65536))];
        var image = new RoomBackgroundImage("seeded background", sheet, [.. words]);
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
    private static RoomBlockPictureTable SeededPictures(Random random, Bgr555[] scene)
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
        return new RoomBlockPictureTable(new RoomBlockSheet(default(TilesetDefinition), sheet), shown);
    }

    /// <summary>
    /// A seeded background-coloured sheet of <paramref name="size"/>: some pixels transparent, of either tile priority and in
    /// every row, even rows' standard colours <paramref name="scene"/>'s and odd rows' different, with a
    /// <paramref name="frameSize"/> frame at each of <paramref name="corners"/>.
    /// </summary>
    private static SpriteSheet SeededSheet(Random random, Bgr555[] scene, Extent16<ImagePixels> size,
        Dictionary<string, Position16<ImagePixels>> corners, Extent16<ImagePixels> frameSize, string name,
        SpriteImageColours kind = SpriteImageColours.Background)
    {
        bool twoBpp = kind == SpriteImageColours.TwoBppBackground;
        bool fullPalette = kind == SpriteImageColours.FullPalette;
        var standard = new ushort[SpriteImage.ColourCountOf(kind)];
        for (int colour = 0; colour < standard.Length; colour++)
            standard[colour] = colour / 16 % 2 == 0 ? scene[colour].ToWord() : (ushort)random.Next(32768);
        int pixels = size.X * size.Y;
        var image = new Rgba32[pixels];
        var indices = new byte[pixels];
        var priorities = new byte[pixels];
        var partRows = new byte[pixels];
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
                partRows[pixel] = (byte)(indices[pixel] / 16 | SpriteImage.FixedPartRow);
                image[pixel] = Bgr555.FromWord(standard[indices[pixel]]).ToRgba32();
                continue;
            }
            if (random.Next(3) == 0)
                continue;
            // A full-palette pixel names any of the 256 colours and has no part row.
            int row = random.Next(fullPalette ? 16 : 8);
            indices[pixel] = (byte)(row * 16 + random.Next(1, 16));
            priorities[pixel] = fullPalette ? (byte)0 : (byte)random.Next(2);
            partRows[pixel] = fullPalette ? (byte)0 : (byte)(row | SpriteImage.FixedPartRow);
            image[pixel] = Bgr555.FromWord(standard[indices[pixel]]).ToRgba32();
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>();
        foreach ((string frame, Position16<ImagePixels> corner) in corners)
        {
            SpritePaletteRows rows = SpritePaletteRows.None;
            for (int y = 0; y < frameSize.Y; y++)
            for (int x = 0; x < frameSize.X; x++)
            {
                byte index = indices[(corner.Y + y) * size.X + corner.X + x];
                if (index % (twoBpp ? 4 : 16) != 0 && index / 16 < 8)
                    rows |= SpritePaletteRowSet.Rows[index / 16];
            }
            frames.Add(frame, new SpriteSheetFrameDocument { Corner = corner, Size = frameSize, Origin = default, Rows = rows, Parts = [] });
        }
        var document = new SpriteSheetDocument
        {
            Version = SpriteSheet.Version,
            Palette = kind,
            Colours = standard,
            Frames = frames,
            PartImages = [],
        };
        return SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(document), writable: false), name, size, image,
            indices, priorities, partRows);
    }

    /// <summary>
    /// A room of random blocks over random definitions, with a random BG3 plane. Each plane's offset places a random point of the room at the
    /// given scroll, so the screen it is scrolled to shows mostly room and some edge. Optionally adds a door's source
    /// screen and words written over BG1.
    /// </summary>
    /// <param name="random">The seeded source.</param>
    /// <param name="bg1Scroll">The BG1 scroll registers the room is shown through.</param>
    /// <param name="bg2Scroll">The BG2 scroll registers, or null for no BG2 plane.</param>
    /// <param name="door">Whether a door's source screen covers part of BG1.</param>
    /// <param name="overlay">Whether words are written over part of BG1.</param>
    /// <param name="pictureColours">
    /// The scene's 256 CGRAM colours the planes' seeded block pictures (#1392) keep as standard in their even rows, odd rows
    /// differing as under a palette effect; null for black.
    /// </param>
    internal static RoomBackgrounds Seeded(Random random, Position16<BgPlanePixels> bg1Scroll,
        Position16<BgPlanePixels>? bg2Scroll = null, bool door = false, bool overlay = false, Bgr555[]? pictureColours = null)
    {
        var bytes = new byte[Definitions * RoomLevelPlane.DefinitionBytes];
        random.NextBytes(bytes);
        ImmutableArray<byte> definitions = [.. bytes];
        RoomBackgroundLayer Layer(Position16<BgPlanePixels> scroll)
        {
            var blocks = new ushort[SideBlocks * SideBlocks];
            for (int index = 0; index < blocks.Length; index++)
                blocks[index] = (ushort)random.Next(ushort.MaxValue + 1);
            var inside = new Position16<BgPlanePixels>((ushort)random.Next(SideBlocks * RoomLevelPlane.BlockPixels / 2),
                (ushort)random.Next(SideBlocks * RoomLevelPlane.BlockPixels / 2));
            return new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(SideBlocks, SideBlocks), blocks, definitions).Cells,
                scroll - inside, SeededPictures(random, pictureColours ?? new Bgr555[SnesPpuLayout.CgramColorCount]));
        }
        RoomBackgroundLayer bg1 = Layer(bg1Scroll);
        RoomBackgroundLayer? bg2 = bg2Scroll is { } scroll2 ? Layer(scroll2) : null;
        RoomDoorBackground? source = door
            ? RoomDoorBackground.Begin(Layer(bg1Scroll), bg1Scroll + new Offset16<BgPlanePixels>((short)random.Next(-128, 128), 0))
            : null;
        RoomTilemapOverlay? written = null;
        if (overlay)
        {
            var words = new ushort[random.Next(8, 24)];
            for (int index = 0; index < words.Length; index++)
                words[index] = (ushort)random.Next(ushort.MaxValue + 1);
            written = RoomTilemapOverlay.Empty.With(random.Next(RoomTilemapOverlay.WordCount - words.Length), words);
        }
        return new RoomBackgrounds(bg1, bg2, SeededBg3Pictures(random, pictureColours ?? new Bgr555[SnesPpuLayout.CgramColorCount]), source, written);
    }
}
