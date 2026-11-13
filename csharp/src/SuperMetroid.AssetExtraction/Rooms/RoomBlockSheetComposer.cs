using SuperMetroid.AssetExtraction.Video;
using System.Collections.Immutable;
using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours each tileset's room blocks (#1392, #1431, #1466, #1513): every block its rooms show, as the
/// import's inventory of them finds (#1636, <see cref="RoomBlockInventory"/>), and no other block its rooms load, over
/// each artwork its rooms show their blocks over and no other (the tileset's own, or the sprites' in Ceres Ridley's arena,
/// #1648), once as that artwork leaves the characters it reads and again for each value each appearance input that shows
/// other artwork over them can take (an animation's frame, Kraid's backdrop, an encounter's look), and each frame of each
/// collectible the tileset's rooms place that shows its own picture, pictured from the collectible's own graphics alone
/// (#1538). No block
/// is pictured over a collectible's graphics. Each picture is coloured with the tileset's standard background palette and
/// named by <see cref="RoomBlockPictureNames"/> from the block's index and the inputs' values, or from the collectible's
/// kind and frame; with the inputs each block depends on, which the room reads to choose among them. Each block, cell and
/// background picture is installed as its two parts (#1634): the pixels of its back tiles under its own name and those of its
/// front tiles under <see cref="PictureParts.InFront"/>'s, so the room's passes choose pictures rather than pixels.
/// </summary>
internal static class RoomBlockSheetComposer
{
    /// <summary>Pictures across one row of a sheet.</summary>
    private const int SheetColumns = 32;

    /// <summary>
    /// The palette index of the colour a tileset's rooms show where nothing is drawn: the background half's first, which
    /// the room's colour effects change as they change its tiles' colours (#1544).
    /// </summary>
    private const int RoomBackdropIndex = 0;

    /// <summary>Composes every tileset's sheet, picturing the blocks <paramref name="inventory"/> finds its rooms show and no other (#1636).</summary>
    internal static RoomSheetComposition Compose(RoomBlockSheetInputs inputs, RoomBlockInventory inventory)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(inventory);
        IReadOnlyDictionary<TilesetDefinition, RoomBlockUploads.TilesetUploads> tilesets = RoomBlockUploads.ByTileset(inputs);
        IReadOnlyDictionary<RoomBlockSet, RoomBlockComposition> blocks = RoomBlockComposition.Of(inputs, tilesets);
        // Each tileset's pictures compose on their own (#1530), and are gathered in tileset order, so what is written and
        // its order stay the same on every install. Every room picture of a tileset is pre-coloured in the shades its rooms'
        // colour effects show (#1512), except the pictures written over its rooms, which show their writers' own (#1544).
        KeyValuePair<TilesetDefinition, RoomBlockUploads.TilesetUploads>[] order = [.. tilesets];
        // Only the tilesets whose rooms show a turning floor picture one (#1649).
        HashSet<TilesetDefinition> floored = [.. RoomBlockUploads.FloorTilesets()];
        var composedTilesets = new TilesetPictures[order.Length];
        Parallel.For(0, order.Length, index =>
        {
            (TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads) = order[index];
            IReadOnlyList<SheetShade> shades = RoomSheetShades.Room(inputs, tileset);
            ComposedBlocks composed = Compose(inputs, tileset, uploads, blocks[tileset.Blocks], inventory.Shown(tileset));
            composedTilesets[index] = new TilesetPictures(
                ShadedSheets.With(composed.Sheet, shades),
                composed.Appearances,
                [.. uploads.Backgrounds.Select(background => KeyValuePair.Create(background.Name, RoomSheetShades.With(
                    ComposeBackground(inputs, tileset, background, composed.Cells[background.Name]), shades)))],
                uploads.WordContent is { } content
                    ? RoomSheetShades.With(WrittenPictureSheetComposer.Compose(inputs, tileset, uploads, uploads.Writers, content),
                        RoomSheetShades.Written(inputs, uploads.Writers))
                    : null,
                RoomSheetShades.With(WrittenPictureSheetComposer.ComposeOverlay(inputs, tileset, uploads), RoomSheetShades.Layer3(inputs, tileset)),
                MessageBoxSheetComposer.Compose(inputs, tileset),
                floored.Contains(tileset) ? ShadedSheets.With(RoomFloorSheetComposer.Compose(inputs, tileset), shades) : null);
        });
        var sheets = new Dictionary<TilesetDefinition, ComposedSpriteSheet>();
        var appearances = new Dictionary<TilesetDefinition, RoomBlockAppearances>();
        var backgrounds = new Dictionary<string, ComposedRoomBackground>(StringComparer.Ordinal);
        var written = new Dictionary<TilesetDefinition, ComposedWrittenPictureSheet>();
        var bg3 = new Dictionary<TilesetDefinition, ComposedWrittenPictureSheet>();
        var messageBoxes = new Dictionary<TilesetDefinition, ComposedSpriteSheet>();
        var floors = new Dictionary<TilesetDefinition, ComposedSpriteSheet>();
        for (int index = 0; index < order.Length; index++)
        {
            TilesetDefinition tileset = order[index].Key;
            TilesetPictures pictures = composedTilesets[index];
            sheets.Add(tileset, pictures.Blocks);
            appearances.Add(tileset, pictures.Appearances);
            foreach ((string name, ComposedRoomBackground background) in pictures.Backgrounds)
                backgrounds.Add(name, background);
            if (pictures.Written is { } wordWritten)
                written.Add(tileset, wordWritten);
            bg3.Add(tileset, pictures.Bg3);
            messageBoxes.Add(tileset, pictures.MessageBox);
            if (pictures.Floor is { } floor)
                floors.Add(tileset, floor);
        }
        return new RoomSheetComposition(sheets, appearances, backgrounds, written, bg3, messageBoxes, floors);
    }

    /// <summary>
    /// Composes a background's image: one period of what a library background shows, its one page where its two pages hold
    /// one tilemap (#1651) and otherwise its two pages, the first above the second, or the sky's rows, each tile over what
    /// its character shows as the room has loaded; with the cells whose pictures change with the room's appearance inputs.
    /// </summary>
    /// <exception cref="InvalidDataException">A tile reads a character nothing the room loads is bound over.</exception>
    private static ComposedRoomBackground ComposeBackground(RoomBlockSheetInputs inputs, TilesetDefinition tileset,
        RoomBlockUploads.RoomBackgroundMap background, IReadOnlyList<RoomBackgroundCell> cells)
    {
        IReadOnlyList<BgCharacterUpload> load = RoomBlockUploads.LoadUploads(inputs, tileset);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.GroupBy(placement => placement.Identity)
            .ToDictionary(group => group.Key, group => group.First().Bytes);
        RgbColor[] colours = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));
        var canvas = new RoomTileCanvas(RoomBackgroundImageGeometry.ImageSize(background.Words.Length, background.WidthTiles));
        ImageColourGroups groups = ImageColourGroups.None;
        for (int index = 0; index < background.Words.Length; index++)
        {
            var word = new SnesBgTilemapWord(background.Words[index]);
            int character = word.CharacterIndex;
            if (!background.Content.TryShowing(BgCharacterSet.Tiles, character, 1, out BgArtworkShown shown))
                throw new InvalidDataException($"Room background {background.Name} reads tile character {character}, which its room loads nothing over.");
            groups = groups.Union(canvas.PaintTile(index % background.WidthTiles * RoomTileCanvas.CharacterPixels,
                index / background.WidthTiles * RoomTileCanvas.CharacterPixels, word,
                bytes[shown.Artwork].Span.Slice(shown.WordOffset * sizeof(ushort), RoomTileCanvas.CharacterBytes), colours));
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [RoomBackgroundImage.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = canvas.Size,
                Origin = default,
                ColourGroups = groups.ManifestMembers,
            },
        };
        return new ComposedRoomBackground(canvas.ToSplitRoomSheet(colours, frames), canvas.Size, cells);
    }

    /// <summary>
    /// Composes one tileset's sheet, picturing only the blocks <paramref name="shownBlocks"/> names (#1636), the inputs each of
    /// those blocks depends on, and the changing cells of each background its rooms load.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// An upload covers only part of a run of characters a block reads, or a block shown is not among those the tileset's
    /// rooms load.
    /// </exception>
    private static ComposedBlocks Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        RoomBlockComposition blocks, IReadOnlySet<int> shownBlocks)
    {
        IReadOnlyList<BgCharacterUpload> load = RoomBlockUploads.LoadUploads(inputs, tileset);
        var loaded = new BgArtworkBindings();
        foreach (BgCharacterUpload placement in load)
            placement.BindTo(loaded);
        // A collectible is pictured only as its kind, from its own graphics, below: no block or background cell is pictured
        // over its graphics (#1513, #1538).
        var options = new AppearanceOptions(uploads.Options);
        var looks = new AppearanceOptions(uploads.ObjectPageLooks);
        var bytes = new Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>>();
        foreach (BgCharacterUpload placement in load.Concat(options.All.Select(option => option.Upload))
            .Concat(uploads.ObjectPageLoad ?? [])
            .Concat(uploads.ObjectPageLooks.Select(option => option.Upload)))
            bytes[placement.Identity] = placement.Bytes;
        // A room whose blocks show the sprites' artwork shows them over what its load places there, and over the looks its
        // encounters show there after (#1392, #1466).
        BgArtworkBindings? objectPage = null;
        if (uploads.ObjectPageLoad is { } page)
        {
            objectPage = new BgArtworkBindings();
            foreach (BgCharacterUpload placement in page)
                placement.BindTo(objectPage);
        }
        RgbColor[] colours = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));

        var pictures = new Dictionary<string, Picture>(StringComparer.Ordinal);
        // Only the blocks some room of the tileset shows are pictured, over each artwork its rooms show them over and no other
        // (#1648); every other block it loads has no picture and depends on nothing (#1636).
        ImmutableArray<AppearanceInput>[]? tilesetInputs = uploads.ShowsTilesetArtwork ? NoInputs(blocks.Characters.Count) : null;
        ImmutableArray<AppearanceInput>[]? spriteInputs = objectPage is null ? null : NoInputs(blocks.Characters.Count);
        foreach (int block in shownBlocks.Order())
        {
            if ((uint)block >= (uint)blocks.Characters.Count)
                throw new InvalidDataException(
                    $"A room of tileset {RoomBlockSheet.NameOf(tileset)} shows block {block}, past the {blocks.Characters.Count} blocks its rooms load.");
            BackgroundTile[] tiles = blocks.TilesOf(block).ToArray();
            if (tilesetInputs is not null)
                tilesetInputs[block] = AddBlock(block, RoomBlockArtwork.Tileset, blocks.Characters.RunsOf(block), tiles, options, loaded);
            if (spriteInputs is not null)
                spriteInputs[block] = AddBlock(block, RoomBlockArtwork.Sprites, blocks.Characters.RunsOf(block), tiles, looks, objectPage);
        }
        foreach (RoomObjectDynamicCollectibleGraphic collectible in uploads.Collectibles)
        {
            // A collectible's picture shows its own graphics alone, each frame its four characters in the colour variants its
            // palette selectors give them, whatever the room loaded or showed before it.
            BgArtworkIdentity graphics = RoomBgArtwork.Collectible(collectible.Kind);
            bytes[graphics] = collectible.Tiles;
            for (int frame = 0; frame < CollectiblePicture.Frames; frame++)
            {
                var tiles = new BackgroundTile[RoomMetatileFormat.TilesPerBlock];
                var shown = new BgArtworkShown[RoomMetatileFormat.TilesPerBlock];
                for (int tile = 0; tile < tiles.Length; tile++)
                {
                    int character = frame * RoomMetatileFormat.TilesPerBlock + tile;
                    tiles[tile] = new BackgroundTile(character, collectible.PaletteOffsets.Span[character], highPriority: false);
                    shown[tile] = new BgArtworkShown(graphics, character * BgCharacterSet.Tiles.CharacterBytes() / sizeof(ushort));
                }
                pictures.Add(RoomBlockPictureNames.OfCollectible(new CollectiblePicture(collectible.Kind, frame)), new Picture(tiles, shown));
            }
        }
        // A library background's image shows its room's load; each 16x16 cell whose characters an input reaches draws the
        // cell's picture for the inputs' values once one of them no longer shows the load.
        var cells = new Dictionary<string, IReadOnlyList<RoomBackgroundCell>>(StringComparer.Ordinal);
        foreach (RoomBlockUploads.RoomBackgroundMap background in uploads.Backgrounds)
        {
            var changing = new List<RoomBackgroundCell>();
            BackgroundTile[][] backgroundCells = [.. CellsOf(BackgroundTileWords.ToTiles(background.Words), background.WidthTiles)];
            for (int cell = 0; cell < backgroundCells.Length; cell++)
            {
                BackgroundTile[] cellTiles = backgroundCells[cell];
                ImmutableArray<AppearanceInput> cellInputs = RoomAppearanceComposition.InputsOver(BgCharacterSet.Tiles, cellTiles.Select(tile => tile.Tile), options);
                if (cellInputs.IsEmpty)
                    continue;
                string subject = CellSubject(cellTiles);
                changing.Add(new RoomBackgroundCell(cell, subject, cellInputs));
                AddCell(cellTiles, subject, cellInputs);
            }
            cells.Add(background.Name, changing);
        }

        int rows = (pictures.Count + SheetColumns - 1) / SheetColumns;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(SheetColumns * RoomBlockSheet.BlockPixels, checked((ushort)(Math.Max(rows, 1) * RoomBlockSheet.BlockPixels))));
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int index = 0;
        foreach ((string name, Picture picture) in pictures)
        {
            var corner = new Position16<ImagePixels>((ushort)(index % SheetColumns * RoomBlockSheet.BlockPixels),
                (ushort)(index / SheetColumns * RoomBlockSheet.BlockPixels));
            ImageColourGroups coloured = ImageColourGroups.None;
            for (int tile = 0; tile < RoomMetatileFormat.TilesPerBlock; tile++)
            {
                coloured = coloured.Union(canvas.PaintTile(corner.X + tile % 2 * RoomTileCanvas.CharacterPixels, corner.Y + tile / 2 * RoomTileCanvas.CharacterPixels,
                    picture.Tiles[tile], bytes[picture.Shown[tile].Artwork].Span.Slice(
                        picture.Shown[tile].WordOffset * sizeof(ushort), RoomTileCanvas.CharacterBytes), colours));
            }
            frames.Add(name, new SpriteSheetFrameDocument
            {
                Corner = corner,
                Size = new Extent16<ImagePixels>(RoomBlockSheet.BlockPixels, RoomBlockSheet.BlockPixels),
                Origin = default,
                ColourGroups = coloured.ManifestMembers,
            });
            index++;
        }
        ComposedSpriteSheet sheet = canvas.ToSplitRoomSheet(colours, frames);
        // The tileset's rooms show its first background colour where nothing is drawn (#1544).
        return new ComposedBlocks(sheet with { Manifest = sheet.Manifest with { Backdrop = RoomBackdropIndex } },
            new RoomBlockAppearances(tilesetInputs, spriteInputs, shownBlocks), cells);

        // Adds a picture of block `block` over `artwork` for each assignment of values to the inputs `candidates` give the
        // runs of characters `runs` it reads, over what `over` shows where an input shows the load; returns those inputs. A
        // block one of whose runs shows nothing for an assignment is not pictured for it.
        ImmutableArray<AppearanceInput> AddBlock(int block, RoomBlockArtwork artwork, ImmutableArray<RoomBlockCharacterRun> runs, BackgroundTile[] tiles,
            AppearanceOptions candidates, BgArtworkBindings? over)
        {
            ImmutableArray<AppearanceInput> blockInputs = RoomAppearanceComposition.InputsOver(BgCharacterSet.Tiles, runs.Select(run => run.FirstCharacter), candidates);
            foreach (string[] values in RoomAppearanceComposition.Assignments(blockInputs, candidates, withLoaded: true))
                AddPicture(RoomBlockPictureNames.Of(block, artwork, blockInputs, values), runs, tiles, blockInputs, values, candidates, over);
            return blockInputs;
        }

        // Adds the picture `name` of a block's `tiles`, which read `runs`, while `blockInputs` take `values`, unless a run
        // shows nothing then.
        void AddPicture(string name, ImmutableArray<RoomBlockCharacterRun> runs, BackgroundTile[] tiles, ImmutableArray<AppearanceInput> blockInputs,
            string[] values, AppearanceOptions candidates, BgArtworkBindings? over)
        {
            var shown = new BgArtworkShown[runs.Length];
            for (int run = 0; run < runs.Length; run++)
            {
                if (!RoomAppearanceComposition.TryShowing(new CharacterSpan(BgCharacterSet.Tiles, runs[run].FirstCharacter, runs[run].CharacterCount), blockInputs, values,
                    candidates, over, out shown[run]))
                    return;
            }
            pictures.TryAdd(name, new Picture(tiles, PerTile(tiles, runs, shown)));
        }

        // Adds a picture of a background cell for each assignment of values to `cellInputs` but the one that shows the load
        // throughout, where the image shows. A cell one of whose characters shows nothing for an assignment is not pictured
        // for it.
        void AddCell(BackgroundTile[] cellTiles, string subject, ImmutableArray<AppearanceInput> cellInputs)
        {
            foreach (string[] values in RoomAppearanceComposition.Assignments(cellInputs, options, withLoaded: true))
            {
                if (values.All(value => value == RoomAppearanceValues.Loaded))
                    continue;
                var shown = new BgArtworkShown[RoomBackgroundImageGeometry.CellTiles];
                bool pictured = true;
                for (int tile = 0; tile < shown.Length && pictured; tile++)
                    pictured = RoomAppearanceComposition.TryShowing(new CharacterSpan(BgCharacterSet.Tiles, cellTiles[tile].Tile, 1), cellInputs, values, options, loaded, out shown[tile]);
                if (pictured)
                    pictures.TryAdd(AppearancePictureNames.Of(subject, cellInputs, values), new Picture([.. cellTiles], shown));
            }
        }
    }

    /// <summary>The inputs of <paramref name="count"/> blocks none of which depends on any.</summary>
    private static ImmutableArray<AppearanceInput>[] NoInputs(int count)
    {
        var inputs = new ImmutableArray<AppearanceInput>[count];
        Array.Fill(inputs, ImmutableArray<AppearanceInput>.Empty);
        return inputs;
    }

    /// <summary>
    /// The 16x16 cells of 32-tile-wide <paramref name="tiles"/>, row by row, each its four tiles: top left, top right,
    /// bottom left, bottom right.
    /// </summary>
    private static IEnumerable<BackgroundTile[]> CellsOf(BackgroundTile[] tiles, int widthTiles)
    {
        for (int cell = 0; cell < tiles.Length / RoomBackgroundImageGeometry.CellTiles; cell++)
        {
            // Half as many cells across as tiles, to a pair of rows.
            int first = cell / (widthTiles / 2) * 2 * widthTiles + cell % (widthTiles / 2) * 2;
            yield return [tiles[first], tiles[first + 1], tiles[first + widthTiles], tiles[first + widthTiles + 1]];
        }
    }

    /// <summary>
    /// The name a cell's pictures begin with: each of its four tiles' character, colour variant, and an <c>h</c>, <c>v</c>
    /// and <c>p</c> for each of its mirrorings and its upper priority tier.
    /// </summary>
    private static string CellSubject(BackgroundTile[] cell) => string.Join(".", cell.Select(tile =>
        string.Create(CultureInfo.InvariantCulture,
            $"{tile.Tile:X3}c{tile.ColourVariant}{(tile.FlipHorizontally ? "h" : "")}{(tile.FlipVertically ? "v" : "")}{(tile.HighPriority ? "p" : "")}")));

    /// <summary>
    /// What shows at each of <paramref name="tiles"/>' characters when <paramref name="shown"/> shows over the runs
    /// <paramref name="runs"/> they lie in: the run's artwork, from that character's place in it.
    /// </summary>
    /// <exception cref="InvalidDataException">A tile reads a character outside every run.</exception>
    private static BgArtworkShown[] PerTile(BackgroundTile[] tiles, ImmutableArray<RoomBlockCharacterRun> runs, BgArtworkShown[] shown)
    {
        var perTile = new BgArtworkShown[tiles.Length];
        for (int tile = 0; tile < tiles.Length; tile++)
        {
            int character = tiles[tile].Tile;
            int run = 0;
            while (run < runs.Length && (character < runs[run].FirstCharacter || character >= runs[run].FirstCharacter + runs[run].CharacterCount))
                run++;
            if (run == runs.Length)
                throw new InvalidDataException($"A block tile reads character {character}, outside every run its block records.");
            perTile[tile] = shown[run] with
            {
                WordOffset = shown[run].WordOffset + (character - runs[run].FirstCharacter) * BgCharacterSet.Tiles.CharacterBytes() / sizeof(ushort),
            };
        }
        return perTile;
    }

    /// <summary>One block or cell to picture: its four tile words and what shows at each tile's character.</summary>
    /// <param name="Tiles">The tiles: top left, top right, bottom left, bottom right.</param>
    /// <param name="Shown">What shows at each tile's character.</param>
    private sealed record Picture(BackgroundTile[] Tiles, BgArtworkShown[] Shown);

    /// <summary>One tileset's composed sheet, the inputs each block depends on, and each background's changing cells.</summary>
    /// <param name="Sheet">The sheet.</param>
    /// <param name="Appearances">The inputs each block depends on.</param>
    /// <param name="Cells">Each background's changing cells, by its installed name.</param>
    private sealed record ComposedBlocks(ComposedSpriteSheet Sheet, RoomBlockAppearances Appearances, IReadOnlyDictionary<string, IReadOnlyList<RoomBackgroundCell>> Cells);

    /// <summary>One tileset's composed pictures.</summary>
    /// <param name="Blocks">Its block sheet.</param>
    /// <param name="Appearances">Its blocks' appearances.</param>
    /// <param name="Backgrounds">Its rooms' background images, by name.</param>
    /// <param name="Written">Its pictures written by words, or null when its rooms write none.</param>
    /// <param name="Bg3">Its layer-3 pictures.</param>
    /// <param name="MessageBox">Its message box sheet.</param>
    /// <param name="Floor">Its Ceres floor sheet, or null outside Ceres.</param>
    private sealed record TilesetPictures(ComposedSpriteSheet Blocks, RoomBlockAppearances Appearances,
        KeyValuePair<string, ComposedRoomBackground>[] Backgrounds, ComposedWrittenPictureSheet? Written, ComposedWrittenPictureSheet Bg3,
        ComposedSpriteSheet MessageBox, ComposedSpriteSheet? Floor);
}
