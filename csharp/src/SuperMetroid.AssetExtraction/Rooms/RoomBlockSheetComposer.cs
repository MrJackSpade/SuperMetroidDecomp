using SuperMetroid.AssetExtraction.Video;
using System.Collections.Immutable;
using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours each tileset's room blocks (#1392, #1431, #1466): every block its rooms load, for each
/// value each appearance input that reaches the characters it reads can take, a collectible slot's for each collectible
/// shown there, coloured with the tileset's standard background palette and named by <see cref="RoomBlockPictureNames"/>
/// from the block's index and the inputs' values; with the inputs each block depends on, which the room reads to choose
/// among them.
/// </summary>
internal static class RoomBlockSheetComposer
{
    /// <summary>Pictures across one row of a sheet.</summary>
    private const int SheetColumns = 32;

    /// <summary>Composes every tileset's sheet.</summary>
    internal static RoomSheetComposition Compose(RoomBlockSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        IReadOnlyDictionary<TilesetDefinition, RoomBlockUploads.TilesetUploads> tilesets = RoomBlockUploads.ByTileset(inputs);
        IReadOnlyDictionary<RoomBlockSet, RoomBlockComposition> blocks = RoomBlockComposition.Of(inputs, tilesets);
        var sheets = new Dictionary<TilesetDefinition, ComposedSpriteSheet>();
        var appearances = new Dictionary<TilesetDefinition, RoomBlockAppearances>();
        var backgrounds = new Dictionary<string, ComposedRoomBackground>(StringComparer.Ordinal);
        foreach ((TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads) in tilesets)
        {
            ComposedBlocks composed = Compose(inputs, tileset, uploads, blocks[tileset.Blocks]);
            sheets.Add(tileset, composed.Sheet);
            appearances.Add(tileset, composed.Appearances);
            foreach (RoomBlockUploads.RoomBackgroundMap background in uploads.Backgrounds)
                backgrounds.Add(background.Name, ComposeBackground(inputs, tileset, background, composed.Cells[background.Name]));
        }
        var written = new Dictionary<TilesetDefinition, ComposedWrittenPictureSheet>();
        foreach ((TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads) in tilesets)
        {
            if (uploads.WordContent is { } content)
                written.Add(tileset, WrittenPictureSheetComposer.Compose(inputs, tileset, uploads, uploads.Writers, content));
        }
        IReadOnlyDictionary<string, ImmutableArray<BackgroundTile>> messages = MessageBoxSheetComposer.Messages(inputs);
        return new RoomSheetComposition(sheets, appearances, backgrounds, written,
            tilesets.ToDictionary(entry => entry.Key, entry => WrittenPictureSheetComposer.ComposeBg3(inputs, entry.Key, entry.Value)),
            messages, tilesets.Keys.ToDictionary(tileset => tileset, tileset => MessageBoxSheetComposer.Compose(inputs, tileset, messages)),
            tilesets.Where(entry => entry.Value.Ceres).ToDictionary(entry => entry.Key,
                entry => Mode7TileSheetComposer.Compose(inputs, entry.Key, RoomBlockUploads.Loaded(inputs, entry.Key))));
    }

    /// <summary>
    /// Composes a background's image: a library background's two pages, the first above the second, or the sky's rows, each
    /// tile over what its character shows as the room has loaded; with the cells whose pictures change with the room's
    /// appearance inputs.
    /// </summary>
    /// <exception cref="InvalidDataException">A tile reads a character nothing the room loads is bound over.</exception>
    private static ComposedRoomBackground ComposeBackground(RoomBlockSheetInputs inputs, TilesetDefinition tileset,
        RoomBlockUploads.RoomBackgroundMap background, IReadOnlyList<RoomBackgroundCell> cells)
    {
        IReadOnlyList<BgCharacterUpload> load = RoomBlockUploads.LoadUploads(inputs, tileset);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.GroupBy(placement => placement.Identity)
            .ToDictionary(group => group.Key, group => group.First().Bytes);
        RgbColor[] colours = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(RoomBackgroundImage.PagePixels, checked((ushort)(background.Words.Length / RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels))));
        ImageColourGroups groups = ImageColourGroups.None;
        for (int index = 0; index < background.Words.Length; index++)
        {
            var word = new SnesBgTilemapWord(background.Words[index]);
            int character = word.CharacterIndex;
            if (!background.Content.TryShowing(BgCharacterSet.Tiles, character, 1, out BgArtworkShown shown))
                throw new InvalidDataException($"Room background {background.Name} reads tile character {character}, which its room loads nothing over.");
            groups = groups.Union(canvas.PaintTile(index % RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels,
                index / RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels, word,
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
                Parts = [],
            },
        };
        return new ComposedRoomBackground(canvas.ToRoomSheet(colours, frames), background.Words, cells);
    }

    /// <summary>
    /// Composes one tileset's sheet, the inputs each of its blocks depends on, and the changing cells of each background its
    /// rooms load.
    /// </summary>
    /// <exception cref="InvalidDataException">An upload covers only part of a run of characters a block reads.</exception>
    private static ComposedBlocks Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        RoomBlockComposition blocks)
    {
        IReadOnlyList<BgCharacterUpload> load = RoomBlockUploads.LoadUploads(inputs, tileset);
        var loaded = new BgArtworkBindings();
        foreach (BgCharacterUpload placement in load)
            placement.BindTo(loaded);
        var options = new AppearanceOptions(uploads.AllOptions);
        var looks = new AppearanceOptions(uploads.ObjectPageLooks);
        var bytes = new Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>>();
        foreach (BgCharacterUpload placement in load.Concat(options.All.Select(option => option.Upload)).Concat(uploads.ObjectPageLoad ?? [])
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
        var tilesetInputs = new ImmutableArray<AppearanceInput>[blocks.Characters.Count];
        ImmutableArray<AppearanceInput>[]? spriteInputs = objectPage is null ? null : new ImmutableArray<AppearanceInput>[blocks.Characters.Count];
        for (int block = 0; block < blocks.Characters.Count; block++)
        {
            if (spriteInputs is not null)
                spriteInputs[block] = [];
            // A collectible slot's block shows only the collectible shown in the slot, pictured below.
            if (blocks.IsCollectibleSlotBlock(block))
            {
                int slot = CollectibleSlotBlocks.SlotOf(block, CollectiblePlms.GraphicsSlots)
                    ?? throw new InvalidOperationException($"Block {block} is a collectible slot's block in no slot.");
                tilesetInputs[block] = [new CollectibleSlotAppearance(slot)];
                continue;
            }
            BackgroundTile[] tiles = blocks.TilesOf(block).ToArray();
            tilesetInputs[block] = AddBlock(block, RoomBlockArtwork.Tileset, tiles, options, loaded);
            if (spriteInputs is not null)
                spriteInputs[block] = AddBlock(block, RoomBlockArtwork.Sprites, tiles, looks, objectPage);
        }
        foreach (RoomBlockUploads.CollectibleSlotCharacters slot in uploads.Collectibles)
        {
            // A collectible's blocks show its own graphics, written and bound together: the collectible shown in the slot
            // names both the tiles and their colours.
            ImmutableArray<AppearanceInput> slotInputs = [slot.Option.Input];
            string[] values = [slot.Option.Value];
            var slotOptions = new AppearanceOptions([slot.Option]);
            for (int block = 0; block < slot.Tiles.Length / RoomMetatileFormat.TilesPerBlock; block++)
            {
                BackgroundTile[] tiles = slot.Tiles[(block * RoomMetatileFormat.TilesPerBlock)..((block + 1) * RoomMetatileFormat.TilesPerBlock)];
                AddPicture(RoomBlockPictureNames.Of(slot.FirstBlock + block, RoomBlockArtwork.Tileset, slotInputs, values), slot.FirstBlock + block,
                    tiles, slotInputs, values, slotOptions, null);
            }
        }
        // A library background's image shows its room's load; each 16x16 cell whose characters an input reaches draws the
        // cell's picture for the inputs' values once one of them no longer shows the load.
        var cells = new Dictionary<string, IReadOnlyList<RoomBackgroundCell>>(StringComparer.Ordinal);
        foreach (RoomBlockUploads.RoomBackgroundMap background in uploads.Backgrounds)
        {
            var changing = new List<RoomBackgroundCell>();
            BackgroundTile[][] backgroundCells = [.. CellsOf(BackgroundTileWords.ToTiles(background.Words))];
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
                Parts = [],
            });
            index++;
        }
        return new ComposedBlocks(canvas.ToRoomSheet(colours, frames), new RoomBlockAppearances(tilesetInputs, spriteInputs), cells);

        // Adds a picture of block `block` over `artwork` for each assignment of values to the inputs `candidates` give the
        // runs of characters it reads, over what `over` shows where an input shows the load; returns those inputs. A block
        // one of whose runs shows nothing for an assignment is not pictured for it.
        ImmutableArray<AppearanceInput> AddBlock(int block, RoomBlockArtwork artwork, BackgroundTile[] tiles, AppearanceOptions candidates,
            BgArtworkBindings? over)
        {
            ImmutableArray<RoomBlockCharacterRun> runs = blocks.Characters.RunsOf(block);
            ImmutableArray<AppearanceInput> blockInputs = RoomAppearanceComposition.InputsOver(BgCharacterSet.Tiles, runs.Select(run => run.FirstCharacter), candidates);
            foreach (string[] values in RoomAppearanceComposition.Assignments(blockInputs, candidates, withLoaded: true))
                AddPicture(RoomBlockPictureNames.Of(block, artwork, blockInputs, values), block, tiles, blockInputs, values, candidates, over);
            return blockInputs;
        }

        // Adds the picture `name` of block `block`'s `tiles` while `blockInputs` take `values`, unless a run of characters the
        // block reads shows nothing then.
        void AddPicture(string name, int block, BackgroundTile[] tiles, ImmutableArray<AppearanceInput> blockInputs, string[] values,
            AppearanceOptions candidates, BgArtworkBindings? over)
        {
            ImmutableArray<RoomBlockCharacterRun> runs = blocks.Characters.RunsOf(block);
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
                var shown = new BgArtworkShown[RoomBackgroundImage.CellTiles];
                bool pictured = true;
                for (int tile = 0; tile < shown.Length && pictured; tile++)
                    pictured = RoomAppearanceComposition.TryShowing(new CharacterSpan(BgCharacterSet.Tiles, cellTiles[tile].Tile, 1), cellInputs, values, options, loaded, out shown[tile]);
                if (pictured)
                    pictures.TryAdd(AppearancePictureNames.Of(subject, cellInputs, values), new Picture([.. cellTiles], shown));
            }
        }
    }

    /// <summary>
    /// The 16x16 cells of 32-tile-wide <paramref name="tiles"/>, row by row, each its four tiles: top left, top right,
    /// bottom left, bottom right.
    /// </summary>
    private static IEnumerable<BackgroundTile[]> CellsOf(BackgroundTile[] tiles)
    {
        for (int cell = 0; cell < tiles.Length / RoomBackgroundImage.CellTiles; cell++)
        {
            // Sixteen cells to a pair of 32-tile rows.
            int first = cell / (RoomBackgroundImage.PageTiles / 2) * 2 * RoomBackgroundImage.PageTiles + cell % (RoomBackgroundImage.PageTiles / 2) * 2;
            yield return [tiles[first], tiles[first + 1], tiles[first + RoomBackgroundImage.PageTiles], tiles[first + RoomBackgroundImage.PageTiles + 1]];
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
}
