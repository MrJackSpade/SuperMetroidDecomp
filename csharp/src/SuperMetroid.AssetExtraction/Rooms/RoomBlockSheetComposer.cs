using System.Buffers.Binary;
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

    /// <summary>Colours in one palette row.</summary>
    private const int RowColours = 16;

    /// <summary>Pixels along a character's side.</summary>
    private const int CharacterPixels = 8;

    /// <summary>Composes every tileset's sheet.</summary>
    internal static IReadOnlyDictionary<TilesetDefinition, ComposedSpriteSheet> Compose(RoomBlockSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        return RoomBlockUploads.ByTileset(inputs).ToDictionary(entry => entry.Key, entry => Compose(inputs, entry.Key, entry.Value));
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
        Bgr555[] colours = StandardColours(inputs.Palettes.Get(tileset.Palette).Transfer.Span);

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

        int rows = (pictures.Count + SheetColumns - 1) / SheetColumns;
        var canvas = new Canvas(SheetColumns * RoomBlockSheet.BlockPixels, Math.Max(rows, 1) * RoomBlockSheet.BlockPixels);
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int index = 0;
        foreach ((string name, Picture picture) in pictures)
        {
            var corner = new Position16<ImagePixels>((ushort)(index % SheetColumns * RoomBlockSheet.BlockPixels),
                (ushort)(index / SheetColumns * RoomBlockSheet.BlockPixels));
            SpritePaletteRows coloured = canvas.Paint(corner, picture, bytes, colours);
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
        return new ComposedSpriteSheet(new((ushort)canvas.Width, (ushort)canvas.Height), canvas.Pixels, canvas.PaletteIndices,
            canvas.Priorities, canvas.PartRows, new SpriteSheetDocument
            {
                Version = SpriteSheet.Version,
                Palette = SpriteImageColours.Background,
                Colours = [.. colours.Select(colour => colour.ToWord())],
                Frames = frames,
                PartImages = [],
            });

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
    private static bool Covers(BgCharacterUpload upload, int first)
    {
        int end = first + RoomBlockPictureNames.CharacterWords, uploadEnd = upload.FirstWord + upload.WordCount;
        if (upload.FirstWord >= end || uploadEnd <= first)
            return false;
        return upload.FirstWord <= first && uploadEnd >= end
            ? true : throw new InvalidDataException($"{upload.Identity.Name} covers only part of the character at VRAM word ${first:X4}.");
    }

    /// <summary>The tileset palette's 128 background colours, the first half of CGRAM a room loads.</summary>
    private static Bgr555[] StandardColours(ReadOnlySpan<byte> palette)
    {
        var colours = new Bgr555[SpriteImage.SpriteColourCount];
        for (int colour = 0; colour < colours.Length; colour++)
            colours[colour] = Bgr555.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(palette[(colour * 2)..]));
        return colours;
    }

    /// <summary>One block to picture: its four tile words and what shows at each tile's character.</summary>
    /// <param name="Words">The tile words: top left, top right, bottom left, bottom right.</param>
    /// <param name="Shown">What shows at each tile's character.</param>
    private sealed record Picture(ushort[] Words, BgArtworkShown[] Shown);

    /// <summary>The sheet's pixel planes, painted one picture at a time.</summary>
    /// <param name="width">The sheet's width.</param>
    /// <param name="height">The sheet's height.</param>
    private sealed class Canvas(int width, int height)
    {
        /// <summary>The sheet's width.</summary>
        internal int Width { get; } = width;

        /// <summary>The sheet's height.</summary>
        internal int Height { get; } = height;

        /// <summary>Each pixel's colour.</summary>
        internal Rgba32[] Pixels { get; } = new Rgba32[width * height];

        /// <summary>Each pixel's palette row times sixteen plus its colour; zero where transparent.</summary>
        internal byte[] PaletteIndices { get; } = new byte[width * height];

        /// <summary>Each pixel's tile priority.</summary>
        internal byte[] Priorities { get; } = new byte[width * height];

        /// <summary>Each pixel's row, which no draw overrides.</summary>
        internal byte[] PartRows { get; } = new byte[width * height];

        /// <summary>Paints a block at <paramref name="corner"/> and returns the rows it was coloured from.</summary>
        internal SpritePaletteRows Paint(Position16<ImagePixels> corner, Picture picture,
            IReadOnlyDictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes, Bgr555[] colours)
        {
            SpritePaletteRows rows = SpritePaletteRows.None;
            for (int tile = 0; tile < RoomBlockPictureNames.Tiles; tile++)
            {
                var word = new SnesBgTilemapWord(picture.Words[tile]);
                ReadOnlySpan<byte> character = bytes[picture.Shown[tile].Artwork].Span.Slice(
                    picture.Shown[tile].WordOffset * sizeof(ushort), RoomBlockPictureNames.CharacterWords * sizeof(ushort));
                int left = corner.X + tile % 2 * CharacterPixels, top = corner.Y + tile / 2 * CharacterPixels;
                for (int y = 0; y < CharacterPixels; y++)
                {
                    int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
                    for (int x = 0; x < CharacterPixels; x++)
                    {
                        int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                        int colour = (character[row * 2] >> bit & 1) | (character[row * 2 + 1] >> bit & 1) << 1
                            | (character[16 + row * 2] >> bit & 1) << 2 | (character[17 + row * 2] >> bit & 1) << 3;
                        if (colour == 0)
                            continue;
                        int pixel = (top + y) * Width + left + x;
                        int index = word.PaletteIndex * RowColours + colour;
                        PaletteIndices[pixel] = (byte)index;
                        Pixels[pixel] = colours[index].ToRgba32();
                        Priorities[pixel] = word.HasPriority ? (byte)1 : (byte)0;
                        PartRows[pixel] = (byte)(word.PaletteIndex | SpriteImage.FixedPartRow);
                        rows |= SpritePaletteRowSet.Rows[word.PaletteIndex];
                    }
                }
            }
            return rows;
        }
    }
}
