using SuperMetroid.AssetExtraction.Video;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours each tileset's room blocks (#1392, #1431): every block its rooms load, a collectible
/// slot's over each collectible bound there, over each artwork that can show at the runs of characters it reads once the
/// room has loaded, coloured with the tileset's standard background palette and named by
/// <see cref="RoomBlockPictureNames"/> from the block's index. The room resolves the same names from the runs each block
/// reads and what its bindings show over them.
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
        var backgrounds = new Dictionary<string, ComposedRoomBackground>(StringComparer.Ordinal);
        foreach ((TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads) in tilesets)
        {
            foreach (RoomBlockUploads.RoomBackgroundMap background in uploads.Backgrounds)
                backgrounds.Add(background.Name, ComposeBackground(inputs, tileset, uploads, background));
        }
        var words = new Dictionary<TilesetDefinition, ComposedTileWordSheet>();
        foreach ((TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads) in tilesets)
        {
            if (uploads.WordContent is { } content)
                words.Add(tileset, TileWordSheetComposer.Compose(inputs, tileset, uploads, uploads.Writers, content));
        }
        // BG3's characters outlive the room that binds them: an effect's frames and Kraid's restoration stay bound in the
        // rooms after, until something binds over them again, so every room's BG3 words are pictured over all of them.
        BgCharacterUpload[] bg3Uploads = [.. tilesets.Values.SelectMany(uploads => uploads.Bg3Uploads)
            .DistinctBy(placement => new BoundArtwork(placement.Identity, placement.Set, placement.FirstCharacter))];
        IReadOnlyDictionary<string, ImmutableArray<BackgroundTile>> messages = MessageBoxSheetComposer.Messages(inputs);
        IReadOnlyDictionary<RoomBlockSet, RoomBlockComposition> blocks = RoomBlockComposition.Of(inputs, tilesets);
        return new RoomSheetComposition(tilesets.ToDictionary(entry => entry.Key, entry => Compose(inputs, entry.Key, entry.Value, blocks[entry.Key.Blocks])),
            blocks.ToDictionary(entry => entry.Key, entry => entry.Value.Characters), backgrounds,
            words, tilesets.ToDictionary(entry => entry.Key, entry => TileWordSheetComposer.ComposeBg3(inputs, entry.Key, entry.Value, bg3Uploads)),
            messages, tilesets.Keys.ToDictionary(tileset => tileset, tileset => MessageBoxSheetComposer.Compose(inputs, tileset, messages)),
            tilesets.Where(entry => entry.Value.Ceres).ToDictionary(entry => entry.Key,
                entry => Mode7TileSheetComposer.Compose(inputs, entry.Key, RoomBlockUploads.Loaded(inputs, entry.Key))),
            Animations(tilesets));
    }

    /// <summary>
    /// The characters each tileset's rooms' animations paint (#1406): its own rooms' runs of the BG1 and BG2 characters and
    /// the OBJ character page, and every room's runs of BG3's, whose words are pictured over every room's effects.
    /// </summary>
    private static Dictionary<TilesetDefinition, RoomAnimatedCharacters> Animations(
        IReadOnlyDictionary<TilesetDefinition, RoomBlockUploads.TilesetUploads> tilesets)
    {
        AnimatedCharacters bg3 = Sorted(tilesets.Values.SelectMany(uploads => uploads.Bg3Runs).Distinct());
        return tilesets.ToDictionary(entry => entry.Key, entry => new RoomAnimatedCharacters(Sorted(entry.Value.BlockRuns),
            Sorted(entry.Value.ObjectPageRuns), bg3));

        // The runs in a stable order: by animation, then by first character.
        static AnimatedCharacters Sorted(IEnumerable<AnimatedCharacterRun> runs) => new(runs
            .OrderBy(run => run.Animation.Name, StringComparer.Ordinal).ThenBy(run => run.FirstCharacter).ThenBy(run => run.CharacterCount));
    }

    /// <summary>
    /// Composes a background's image: a library background's two pages, the first above the second, or the sky's rows, each
    /// tile over what its character shows as the room has loaded.
    /// </summary>
    /// <exception cref="InvalidDataException">A tile reads a character nothing the room loads is bound over.</exception>
    private static ComposedRoomBackground ComposeBackground(RoomBlockSheetInputs inputs, TilesetDefinition tileset,
        RoomBlockUploads.TilesetUploads uploads, RoomBlockUploads.RoomBackgroundMap background)
    {
        IReadOnlyList<BgCharacterUpload> load = RoomBlockUploads.LoadUploads(inputs, tileset);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.Concat(uploads.Uploads).GroupBy(placement => placement.Identity)
            .ToDictionary(group => group.Key, group => group.First().Bytes);
        RgbColor[] colours = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).PackedColours.Span);
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(RoomBackgroundImage.PagePixels, checked((ushort)(background.Words.Length / RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels))));
        SpritePaletteRows rows = SpritePaletteRows.None;
        for (int index = 0; index < background.Words.Length; index++)
        {
            var word = new SnesBgTilemapWord(background.Words[index]);
            int character = word.CharacterIndex;
            if (!background.Content.TryShowing(BgCharacterSet.Tiles, character, 1, out BgArtworkShown shown))
                throw new InvalidDataException($"Room background {background.Name} reads tile character {character}, which its room loads nothing over.");
            rows |= canvas.PaintTile(index % RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels,
                index / RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels, word,
                bytes[shown.Artwork].Span.Slice(shown.WordOffset * sizeof(ushort), RoomTileCanvas.CharacterBytes), colours);
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [RoomBackgroundImage.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = canvas.Size,
                Origin = default,
                Rows = rows,
                Parts = [],
            },
        };
        return new ComposedRoomBackground(canvas.ToRoomSheet(colours, frames), background.Words);
    }

    /// <summary>Composes one tileset's sheet.</summary>
    /// <exception cref="InvalidDataException">An upload covers only part of a run of characters a block reads.</exception>
    private static ComposedSpriteSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        RoomBlockComposition blocks)
    {
        IReadOnlyList<BgCharacterUpload> load = RoomBlockUploads.LoadUploads(inputs, tileset);
        var loaded = new BgArtworkBindings();
        foreach (BgCharacterUpload placement in load)
            placement.BindTo(loaded);
        var bytes = new Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>>();
        foreach (BgCharacterUpload placement in load.Concat(uploads.Uploads).Concat(uploads.Collectibles.Select(slot => slot.Characters))
            .Concat(uploads.ObjectPages.SelectMany(page => page)))
            bytes[placement.Identity] = placement.Bytes;
        // A room whose blocks read the OBJ character table shows them over what its load uploads there, and over what its
        // encounters upload there after (#1392).
        BgArtworkBindings[] objectPages = [.. uploads.ObjectPages.Select(page =>
        {
            var bound = new BgArtworkBindings();
            foreach (BgCharacterUpload placement in page)
                placement.BindTo(bound);
            return bound;
        })];
        RgbColor[] colours = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).PackedColours.Span);

        var pictures = new Dictionary<string, Picture>(StringComparer.Ordinal);
        BgCharacterUpload[] all = [.. uploads.Uploads.Concat(uploads.Collectibles.Select(slot => slot.Characters))
            .Where(placement => placement.Set == BgCharacterSet.Tiles)];
        int[] bounds = [.. all.SelectMany(placement => new[] { placement.FirstCharacter, placement.FirstCharacter + placement.CharacterCount }).Distinct().Order()];
        for (int block = 0; block < blocks.Characters.Count; block++)
        {
            // A collectible slot's block shows only the collectible bound over the slot, pictured below.
            if (blocks.IsCollectibleSlotBlock(block))
                continue;
            BackgroundTile[] tiles = blocks.TilesOf(block).ToArray();
            AddBlock(block, tiles, all, withLoad: true);
            foreach (BgArtworkBindings objectPage in objectPages)
                AddBlockOverObjectPage(block, tiles, objectPage);
        }
        foreach (RoomBlockUploads.CollectibleSlotCharacters slot in uploads.Collectibles)
        {
            // A collectible's blocks show its own graphics, written and bound together: the collectible bound over the slot
            // names both the characters and their colours.
            for (int block = 0; block < slot.Tiles.Length / RoomMetatileFormat.TilesPerBlock; block++)
                AddBlock(slot.FirstBlock + block, slot.Tiles[(block * RoomMetatileFormat.TilesPerBlock)..((block + 1) * RoomMetatileFormat.TilesPerBlock)],
                    [slot.Characters], withLoad: false);
        }
        // A library background's image shows its room's load; each 16x16 cell whose characters the room rebinds after
        // loading draws the cell's picture for what they show then.
        foreach (RoomBlockUploads.RoomBackgroundMap background in uploads.Backgrounds)
        {
            foreach (BackgroundTile[] cell in RoomBackgroundImage.Cells(BackgroundTileWords.ToTiles(background.Words)))
            {
                if (cell.Any(tile => all.Any(placement => placement.Covers(BgCharacterSet.Tiles, tile.Tile))))
                    AddCell(cell, all);
            }
        }

        int rows = (pictures.Count + SheetColumns - 1) / SheetColumns;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(SheetColumns * RoomBlockSheet.BlockPixels, checked((ushort)(Math.Max(rows, 1) * RoomBlockSheet.BlockPixels))));
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int index = 0;
        foreach ((string name, Picture picture) in pictures)
        {
            var corner = new Position16<ImagePixels>((ushort)(index % SheetColumns * RoomBlockSheet.BlockPixels),
                (ushort)(index / SheetColumns * RoomBlockSheet.BlockPixels));
            SpritePaletteRows coloured = SpritePaletteRows.None;
            for (int tile = 0; tile < RoomMetatileFormat.TilesPerBlock; tile++)
            {
                coloured |= canvas.PaintTile(corner.X + tile % 2 * RoomTileCanvas.CharacterPixels, corner.Y + tile / 2 * RoomTileCanvas.CharacterPixels,
                    picture.Tiles[tile], bytes[picture.Shown[tile].Artwork].Span.Slice(
                        picture.Shown[tile].WordOffset * sizeof(ushort), RoomTileCanvas.CharacterBytes), colours);
            }
            frames.Add(name, new SpriteSheetFrameDocument
            {
                Corner = corner,
                Size = new Extent16<ImagePixels>(RoomBlockSheet.BlockPixels, RoomBlockSheet.BlockPixels),
                Origin = default,
                Rows = coloured,
                Parts = [],
            });
            index++;
        }
        return canvas.ToRoomSheet(colours, frames);

        // Adds a picture of block `block` over the OBJ character table `objects` binds, when every run it reads is bound.
        void AddBlockOverObjectPage(int block, BackgroundTile[] tiles, BgArtworkBindings objects)
        {
            ImmutableArray<RoomBlockCharacterRun> runs = blocks.Characters.RunsOf(block);
            var shown = new BgArtworkShown[runs.Length];
            for (int run = 0; run < runs.Length; run++)
            {
                if (!objects.TryShowing(BgCharacterSet.Tiles, runs[run].FirstCharacter, runs[run].CharacterCount, out shown[run]))
                    return;
            }
            pictures.TryAdd(RoomBlockPictureNames.Of(block, shown), new Picture(tiles, PerTile(tiles, runs, shown)));
        }

        // Adds a picture of block `block` for each combination of what can show over the runs of characters it reads: the
        // room's load, when given, or each of `candidates` covering the run. A block one of whose runs shows nothing is not
        // pictured.
        void AddBlock(int block, BackgroundTile[] tiles, IReadOnlyList<BgCharacterUpload> candidates, bool withLoad)
        {
            ImmutableArray<RoomBlockCharacterRun> runs = blocks.Characters.RunsOf(block);
            IEnumerable<BgCharacterUpload?[]> combinations = [new BgCharacterUpload?[runs.Length]];
            for (int run = 0; run < runs.Length; run++)
            {
                int place = run, first = runs[run].FirstCharacter;
                BgCharacterUpload?[] options = [.. (withLoad ? [null] : Array.Empty<BgCharacterUpload?>()),
                    .. candidates.Where(placement => placement.Covers(BgCharacterSet.Tiles, first))];
                combinations = [.. combinations.SelectMany(earlier => options.Select(option =>
                {
                    BgCharacterUpload?[] chosen = [.. earlier];
                    chosen[place] = option;
                    return chosen;
                }))];
            }
            foreach (BgCharacterUpload?[] chosen in combinations)
            {
                var shown = new BgArtworkShown[runs.Length];
                bool pictured = true;
                for (int run = 0; run < runs.Length && pictured; run++)
                {
                    if (chosen[run] is { } placement)
                        shown[run] = placement.ShownAt(runs[run].FirstCharacter);
                    else
                        pictured = withLoad && loaded.TryShowing(BgCharacterSet.Tiles, runs[run].FirstCharacter, runs[run].CharacterCount, out shown[run]);
                }
                if (pictured)
                    pictures.TryAdd(RoomBlockPictureNames.Of(block, shown), new Picture(tiles, PerTile(tiles, runs, shown)));
            }
        }

        // Adds a picture of a library background cell for each combination of what the upload regions its tiles read can
        // show: the room's load, or each of `candidates` covering the region. Tiles in one region show the same artwork, as
        // one binding covers them all. A cell one of whose characters shows nothing is not pictured.
        void AddCell(BackgroundTile[] words, IReadOnlyList<BgCharacterUpload> candidates)
        {
            int[] firsts = [.. words.Select(tile => tile.Tile)];
            int[] regions = [.. firsts.Select(first => RoomBlockComposition.Region(bounds, first))];
            IEnumerable<BgCharacterUpload?[]> combinations = [new BgCharacterUpload?[RoomBackgroundImage.CellTiles]];
            foreach (int region in regions.Distinct())
            {
                int first = firsts[Array.IndexOf(regions, region)];
                BgCharacterUpload?[] options = [null, .. candidates.Where(placement => placement.Covers(BgCharacterSet.Tiles, first))];
                combinations = [.. combinations.SelectMany(earlier => options.Select(option =>
                {
                    BgCharacterUpload?[] chosen = [.. earlier];
                    for (int tile = 0; tile < chosen.Length; tile++)
                    {
                        if (regions[tile] == region)
                            chosen[tile] = option;
                    }
                    return chosen;
                }))];
            }
            foreach (BgCharacterUpload?[] chosen in combinations)
            {
                var shown = new BgArtworkShown[RoomBackgroundImage.CellTiles];
                bool pictured = true;
                for (int tile = 0; tile < shown.Length && pictured; tile++)
                {
                    if (chosen[tile] is { } placement)
                        shown[tile] = placement.ShownAt(firsts[tile]);
                    else
                        pictured = loaded.TryShowing(BgCharacterSet.Tiles, firsts[tile], 1, out shown[tile]);
                }
                if (pictured)
                    pictures.TryAdd(RoomBackgroundImage.CellPictureName(words, shown), new Picture([.. words], shown));
            }
        }
    }

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

    /// <summary>Artwork bound over background characters: the artwork, the set and the character it starts at.</summary>
    /// <param name="Identity">The artwork.</param>
    /// <param name="Set">The character set it is bound over.</param>
    /// <param name="FirstCharacter">The character it starts at.</param>
    private readonly record struct BoundArtwork(BgArtworkIdentity Identity, BgCharacterSet Set, int FirstCharacter);
}
