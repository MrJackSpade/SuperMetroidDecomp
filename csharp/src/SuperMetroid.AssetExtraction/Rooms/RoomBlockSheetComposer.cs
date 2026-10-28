using SuperMetroid.AssetExtraction.Video;
using System.Buffers.Binary;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours each tileset's room blocks (#1392): every block definition its rooms load, and each one a
/// collectible writes, over each artwork that can show at its characters once the room has loaded, coloured with the
/// tileset's standard background palette and named by <see cref="RoomBlockPictureNames"/>. The room resolves the same
/// names from what its bindings show.
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
        return new RoomSheetComposition(tilesets.ToDictionary(entry => entry.Key, entry => Compose(inputs, entry.Key, entry.Value)), backgrounds,
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
        return new ComposedRoomBackground(canvas.ToSheet(colours, frames), background.Words);
    }

    /// <summary>Composes one tileset's sheet.</summary>
    /// <exception cref="InvalidDataException">An upload covers only part of a character a block reads.</exception>
    private static ComposedSpriteSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads)
    {
        BackgroundTile[] definitions = LoadedRoomAssets.BlockDefinitions(inputs.Metatiles, tileset, uploads.Ceres);
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
        var tiles = new BackgroundTile[RoomBlockPictureNames.Tiles];
        for (int definition = 0; definition < definitions.Length / RoomLevelPlane.DefinitionTiles; definition++)
        {
            for (int tile = 0; tile < tiles.Length; tile++)
                tiles[tile] = definitions[definition * RoomLevelPlane.DefinitionTiles + tile];
            AddEach(tiles, all, withLoad: true);
            foreach (BgArtworkBindings objectPage in objectPages)
                AddOverObjectPage(tiles, objectPage);
        }
        foreach (RoomBlockUploads.CollectibleSlotCharacters slot in uploads.Collectibles)
        {
            // A collectible's definitions show its own graphics, written and bound together.
            for (int block = 0; block < slot.Tiles.Length / RoomBlockPictureNames.Tiles; block++)
                AddEach(slot.Tiles[(block * RoomBlockPictureNames.Tiles)..((block + 1) * RoomBlockPictureNames.Tiles)], [slot.Characters], withLoad: false);
        }
        // A library background's image shows its room's load; each 16x16 cell whose characters the room rebinds after
        // loading draws the cell's picture for what they show then.
        foreach (RoomBlockUploads.RoomBackgroundMap background in uploads.Backgrounds)
        {
            foreach (BackgroundTile[] cell in RoomBackgroundImage.Cells(BackgroundTileWords.ToTiles(background.Words)))
            {
                if (cell.Any(tile => all.Any(placement => placement.Covers(BgCharacterSet.Tiles, tile.Tile))))
                    AddEach(cell, all, withLoad: true);
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
            for (int tile = 0; tile < RoomBlockPictureNames.Tiles; tile++)
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
        return canvas.ToSheet(colours, frames);

        // Adds a picture of the block over the OBJ character table `objects` binds, when every tile's character is bound.
        void AddOverObjectPage(BackgroundTile[] words, BgArtworkBindings objects)
        {
            var shown = new BgArtworkShown[RoomBlockPictureNames.Tiles];
            for (int tile = 0; tile < shown.Length; tile++)
            {
                if (!objects.TryShowing(BgCharacterSet.Tiles, words[tile].Tile, 1, out shown[tile]))
                    return;
            }
            pictures.TryAdd(RoomBlockPictureNames.Of(words, shown), new Picture([.. words], shown));
        }

        // Adds a picture of the block for each combination of what the upload regions its tiles read can show: the
        // room's load, when given, or each of `candidates` covering the region. Tiles in one region show the same
        // artwork, as one binding covers them all. A block one of whose characters shows nothing is not pictured.
        void AddEach(BackgroundTile[] words, IReadOnlyList<BgCharacterUpload> candidates, bool withLoad)
        {
            int[] firsts = [.. words.Select(tile => tile.Tile)];
            int[] regions = [.. firsts.Select(first => Region(bounds, first))];
            IEnumerable<BgCharacterUpload?[]> combinations = [new BgCharacterUpload?[RoomBlockPictureNames.Tiles]];
            foreach (int region in regions.Distinct())
            {
                int first = firsts[Array.IndexOf(regions, region)];
                BgCharacterUpload?[] options = [.. (withLoad ? [null] : Array.Empty<BgCharacterUpload?>()),
                    .. candidates.Where(placement => placement.Covers(BgCharacterSet.Tiles, first))];
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
                var shown = new BgArtworkShown[RoomBlockPictureNames.Tiles];
                bool pictured = true;
                for (int tile = 0; tile < shown.Length && pictured; tile++)
                {
                    if (chosen[tile] is { } placement)
                        shown[tile] = placement.ShownAt(firsts[tile]);
                    else
                        pictured = withLoad && loaded.TryShowing(BgCharacterSet.Tiles, firsts[tile], 1, out shown[tile]);
                }
                if (pictured)
                    pictures.TryAdd(RoomBlockPictureNames.Of(words, shown), new Picture([.. words], shown));
            }
        }
    }

    /// <summary>
    /// The upload region holding the character from <paramref name="first"/>: the run between two consecutive upload
    /// bounds it lies in, named by its first bound, or minus one outside every bound.
    /// </summary>
    private static int Region(int[] bounds, int first)
    {
        int index = Array.BinarySearch(bounds, first);
        return index >= 0 ? bounds[index] : ~index == 0 ? -1 : bounds[~index - 1];
    }

    /// <summary>One block to picture: its four tile words and what shows at each tile's character.</summary>
    /// <param name="Tiles">The tiles: top left, top right, bottom left, bottom right.</param>
    /// <param name="Shown">What shows at each tile's character.</param>
    private sealed record Picture(BackgroundTile[] Tiles, BgArtworkShown[] Shown);

    /// <summary>Artwork bound over background characters: the artwork, the set and the character it starts at.</summary>
    /// <param name="Identity">The artwork.</param>
    /// <param name="Set">The character set it is bound over.</param>
    /// <param name="FirstCharacter">The character it starts at.</param>
    private readonly record struct BoundArtwork(BgArtworkIdentity Identity, BgCharacterSet Set, int FirstCharacter);
}
