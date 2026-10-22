using System.Buffers.Binary;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
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
            .DistinctBy(upload => (upload.Identity, upload.FirstWord))];
        IReadOnlyDictionary<string, ImmutableArray<ushort>> messages = MessageBoxSheetComposer.Messages(inputs);
        return new RoomSheetComposition(tilesets.ToDictionary(entry => entry.Key, entry => Compose(inputs, entry.Key, entry.Value)), backgrounds,
            words, tilesets.ToDictionary(entry => entry.Key, entry => TileWordSheetComposer.ComposeBg3(inputs, entry.Key, entry.Value, bg3Uploads)),
            messages, tilesets.Keys.ToDictionary(tileset => tileset, tileset => MessageBoxSheetComposer.Compose(inputs, tileset, messages)));
    }

    /// <summary>
    /// Composes a background's image: a library background's two pages, the first above the second, or the sky's rows, each
    /// tile over what its character shows as the room has loaded.
    /// </summary>
    /// <exception cref="InvalidDataException">A tile reads a character nothing the room loads is bound over.</exception>
    [PpuLayoutBoundary("codec: the BG characters a background's tile words name, from the room tileset's character base")]
    private static ComposedRoomBackground ComposeBackground(RoomBlockSheetInputs inputs, TilesetDefinition tileset,
        RoomBlockUploads.TilesetUploads uploads, RoomBlockUploads.RoomBackgroundMap background)
    {
        IReadOnlyList<BgCharacterUpload> load = CartridgeRoomAssets.CharacterUploads(tileset, inputs.Characters.Cre.Transfer,
            inputs.Characters.Get(tileset.Characters).Transfer);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.Concat(uploads.Uploads).GroupBy(upload => upload.Identity)
            .ToDictionary(group => group.Key, group => group.First().Bytes);
        Bgr555[] colours = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).Transfer.Span);
        var canvas = new RoomTileCanvas(RoomBackgroundImage.PagePixels,
            background.Words.Length / RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels);
        SpritePaletteRows rows = SpritePaletteRows.None;
        for (int index = 0; index < background.Words.Length; index++)
        {
            var word = new SnesBgTilemapWord(background.Words[index]);
            int first = CharacterWord(word.Raw);
            if (!background.Content.TryShowing(first, RoomBlockPictureNames.CharacterWords, out BgArtworkShown shown))
                throw new InvalidDataException($"Room background {background.Name} reads a character at VRAM word ${first:X4} its room loads nothing over.");
            rows |= canvas.PaintTile(index % RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels,
                index / RoomBackgroundImage.PageTiles * RoomTileCanvas.CharacterPixels, word,
                bytes[shown.Artwork].Span.Slice(shown.WordOffset * sizeof(ushort), RoomTileCanvas.CharacterBytes), colours);
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [RoomBackgroundImage.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = new Extent16<ImagePixels>((ushort)canvas.Width, (ushort)canvas.Height),
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
        byte[] definitions = CartridgeRoomAssets.BlockDefinitions(inputs.Metatiles, tileset, uploads.Ceres);
        IReadOnlyList<BgCharacterUpload> load = CartridgeRoomAssets.CharacterUploads(tileset, inputs.Characters.Cre.Transfer,
            inputs.Characters.Get(tileset.Characters).Transfer);
        var loaded = new BgArtworkBindings(new SnesVram());
        foreach (BgCharacterUpload upload in load)
            upload.BindTo(loaded);
        var bytes = new Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>>();
        foreach (BgCharacterUpload upload in load.Concat(uploads.Uploads).Concat(uploads.Collectibles.Select(slot => slot.Upload)))
            bytes[upload.Identity] = upload.Bytes;
        Bgr555[] colours = RoomTileCanvas.StandardColours(inputs.Palettes.Get(tileset.Palette).Transfer.Span);

        var pictures = new Dictionary<string, Picture>(StringComparer.Ordinal);
        BgCharacterUpload[] all = [.. uploads.Uploads.Concat(uploads.Collectibles.Select(slot => slot.Upload))];
        int[] bounds = [.. all.SelectMany(upload => new[] { upload.FirstWord, upload.FirstWord + upload.WordCount }).Distinct().Order()];
        var tiles = new ushort[RoomBlockPictureNames.Tiles];
        for (int definition = 0; definition < definitions.Length / RoomLevelPlane.DefinitionBytes; definition++)
        {
            for (int tile = 0; tile < tiles.Length; tile++)
                tiles[tile] = BinaryPrimitives.ReadUInt16LittleEndian(definitions.AsSpan(definition * RoomLevelPlane.DefinitionBytes + tile * sizeof(ushort)));
            AddEach(tiles, all, withLoad: true);
        }
        foreach (RoomBlockUploads.CollectibleSlotUpload slot in uploads.Collectibles)
        {
            // A collectible's definitions show its own graphics, written and bound together.
            for (int block = 0; block < slot.Words.Length / RoomBlockPictureNames.Tiles; block++)
                AddEach(slot.Words[(block * RoomBlockPictureNames.Tiles)..((block + 1) * RoomBlockPictureNames.Tiles)], [slot.Upload], withLoad: false);
        }
        // A library background's image shows its room's load; each 16x16 cell whose characters the room rebinds after
        // loading draws the cell's picture for what they show then.
        BgCharacterUpload[] rebinding = [.. all.Where(upload => !uploads.LibraryArtwork.Contains(upload.Identity))];
        foreach (RoomBlockUploads.RoomBackgroundMap background in uploads.Backgrounds)
        {
            foreach (ushort[] cell in RoomBackgroundImage.Cells(background.Words))
            {
                if (cell.Any(word => rebinding.Any(upload => Covers(upload, CharacterWord(word)))))
                    AddEach(cell, all, withLoad: true);
            }
        }

        int rows = (pictures.Count + SheetColumns - 1) / SheetColumns;
        var canvas = new RoomTileCanvas(SheetColumns * RoomBlockSheet.BlockPixels, Math.Max(rows, 1) * RoomBlockSheet.BlockPixels);
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
                    new SnesBgTilemapWord(picture.Words[tile]), bytes[picture.Shown[tile].Artwork].Span.Slice(
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

        // Adds a picture of the block for each combination of what the upload regions its tiles read can show: the
        // room's load, when given, or each of `candidates` covering the region. Tiles in one region show the same
        // artwork, as one binding covers them all. A block one of whose characters shows nothing is not pictured.
        void AddEach(ushort[] words, IReadOnlyList<BgCharacterUpload> candidates, bool withLoad)
        {
            int[] firsts = [.. words.Select(word => CharacterWord(word))];
            int[] regions = [.. firsts.Select(first => Region(bounds, first))];
            IEnumerable<BgCharacterUpload?[]> combinations = [new BgCharacterUpload?[RoomBlockPictureNames.Tiles]];
            foreach (int region in regions.Distinct())
            {
                int first = firsts[Array.IndexOf(regions, region)];
                BgCharacterUpload?[] options = [.. (withLoad ? [null] : Array.Empty<BgCharacterUpload?>()),
                    .. candidates.Where(upload => Covers(upload, first))];
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
                    if (chosen[tile] is { } upload)
                        shown[tile] = new BgArtworkShown(upload.Identity, firsts[tile] - upload.FirstWord);
                    else
                        pictured = withLoad && loaded.TryShowing(firsts[tile], RoomBlockPictureNames.CharacterWords, out shown[tile]);
                }
                if (pictured)
                    pictures.TryAdd(RoomBlockPictureNames.Of(words, shown), new Picture([.. words], shown));
            }
        }
    }

    /// <summary>The first VRAM word of the character tile word <paramref name="word"/> names.</summary>
    [PpuLayoutBoundary("codec: the BG characters a block's tile words name, from the room tileset's character base")]
    private static int CharacterWord(ushort word) =>
        RoomGraphicsLayout.AreaCharacters.Offset + new SnesBgTilemapWord(word).CharacterIndex * RoomBlockPictureNames.CharacterWords;

    /// <summary>
    /// The upload region holding the character from <paramref name="first"/>: the run between two consecutive upload
    /// bounds it lies in, named by its first bound, or minus one outside every bound.
    /// </summary>
    private static int Region(int[] bounds, int first)
    {
        int index = Array.BinarySearch(bounds, first);
        return index >= 0 ? bounds[index] : ~index == 0 ? -1 : bounds[~index - 1];
    }

    /// <summary>Whether <paramref name="upload"/> covers the character from <paramref name="first"/>.</summary>
    /// <exception cref="InvalidDataException">The upload covers only part of the character.</exception>
    internal static bool Covers(BgCharacterUpload upload, int first, int characterWords = RoomBlockPictureNames.CharacterWords)
    {
        int end = first + characterWords, uploadEnd = upload.FirstWord + upload.WordCount;
        if (upload.FirstWord >= end || uploadEnd <= first)
            return false;
        return upload.FirstWord <= first && uploadEnd >= end
            ? true : throw new InvalidDataException($"{upload.Identity.Name} covers only part of the character at VRAM word ${first:X4}.");
    }

    /// <summary>One block to picture: its four tile words and what shows at each tile's character.</summary>
    /// <param name="Words">The tile words: top left, top right, bottom left, bottom right.</param>
    /// <param name="Shown">What shows at each tile's character.</param>
    private sealed record Picture(ushort[] Words, BgArtworkShown[] Shown);

}
