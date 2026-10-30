using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The blocks one block set's rooms load, as extraction pictures them (#1431): each block's four tiles, from the installed
/// stock compositions, and the runs of characters each block reads. A Zebes room loads the common room elements' blocks
/// before its tileset's, and a collectible slot's blocks read the slot's characters, which the collectible writes them to
/// read; a Ceres room loads its tileset's blocks alone. Runs are split wherever any binding a room of the set makes may
/// begin or end, so one artwork shows over each whole run in every room.
/// </summary>
internal sealed class RoomBlockComposition
{
    /// <summary>Keeps the tiles of every block and the characters each reads.</summary>
    private RoomBlockComposition(BackgroundTile[] tiles, bool collectibleSlots, RoomBlockCharacters characters)
    {
        this.tiles = tiles;
        CollectibleSlots = collectibleSlots;
        Characters = characters;
    }

    /// <summary>Every block's tiles, four per block: top left, top right, bottom left, bottom right.</summary>
    private readonly BackgroundTile[] tiles;

    /// <summary>Whether the set's rooms load the common room elements, whose collectible slot blocks a collectible writes.</summary>
    private bool CollectibleSlots { get; }

    /// <summary>The runs of characters each block reads.</summary>
    internal RoomBlockCharacters Characters { get; }

    /// <summary>The four tiles of block <paramref name="block"/> as the room loads them.</summary>
    internal ReadOnlySpan<BackgroundTile> TilesOf(int block) =>
        tiles.AsSpan(block * RoomMetatileFormat.TilesPerBlock, RoomMetatileFormat.TilesPerBlock);

    /// <summary>Whether block <paramref name="block"/> is a collectible slot's, which shows only the collectible bound over the slot.</summary>
    internal bool IsCollectibleSlotBlock(int block) => CollectibleSlots && CollectibleSlotBlocks.SlotOf(block, CollectiblePlms.GraphicsSlots) is not null;

    /// <summary>The blocks of each block set some tileset of <paramref name="tilesets"/> loads.</summary>
    /// <exception cref="InvalidDataException">One block set is loaded both inside and outside Ceres.</exception>
    internal static IReadOnlyDictionary<RoomBlockSet, RoomBlockComposition> Of(RoomBlockSheetInputs inputs,
        IReadOnlyDictionary<TilesetDefinition, RoomBlockUploads.TilesetUploads> tilesets)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(tilesets);
        var compositions = new Dictionary<RoomBlockSet, RoomBlockComposition>();
        foreach (IGrouping<RoomBlockSet, KeyValuePair<TilesetDefinition, RoomBlockUploads.TilesetUploads>> set in tilesets.GroupBy(entry => entry.Key.Blocks))
        {
            bool[] ceres = [.. set.Select(entry => entry.Value.Ceres).Distinct()];
            if (ceres.Length != 1)
                throw new InvalidDataException($"Block set {set.Key} is loaded both inside and outside Ceres.");
            int[] bounds = [.. set.SelectMany(entry => Bindings(inputs, entry.Key, entry.Value))
                .Where(placement => placement.Set == BgCharacterSet.Tiles)
                .SelectMany(placement => new[] { placement.FirstCharacter, placement.FirstCharacter + placement.CharacterCount })
                .Distinct().Order()];
            compositions.Add(set.Key, Of(inputs.Metatiles, set.Key, ceres[0], bounds));
        }
        return compositions;
    }

    /// <summary>
    /// The upload region holding the character from <paramref name="first"/>: the run between two consecutive upload
    /// bounds it lies in, named by its first bound, or minus one outside every bound.
    /// </summary>
    internal static int Region(int[] bounds, int first)
    {
        int index = Array.BinarySearch(bounds, first);
        return index >= 0 ? bounds[index] : ~index == 0 ? -1 : bounds[~index - 1];
    }

    /// <summary>
    /// The stock tiles of every block a room of <paramref name="set"/> loads, four per block, as the cartridge's block
    /// tables compose them before any collectible writes its slot's.
    /// </summary>
    /// <exception cref="InvalidDataException">The compositions are not whole blocks.</exception>
    internal static BackgroundTile[] StockTiles(RoomMetatileCatalog metatiles, RoomBlockSet set, bool ceres)
    {
        Ensure.NotNull(metatiles);
        // $82:E7D3 installs a Ceres tileset directly at the start of the block table: unlike Zebes rooms, area six does
        // not reserve its first blocks for the common room elements.
        BackgroundTile[] tiles = ceres ? metatiles.Get(set).BlockTiles.ToArray()
            : [.. metatiles.Cre.BlockTiles.ToArray(), .. metatiles.Get(set).BlockTiles.ToArray()];
        if (tiles.Length == 0 || tiles.Length % RoomMetatileFormat.TilesPerBlock != 0)
            throw new InvalidDataException($"Block set {set} composes {tiles.Length} tiles; blocks are four tiles each.");
        return tiles;
    }

    /// <summary>Every binding a room of <paramref name="tileset"/> makes over its characters: its load and everything after.</summary>
    private static IEnumerable<BgCharacterUpload> Bindings(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads) =>
        RoomBlockUploads.LoadUploads(inputs, tileset).Concat(uploads.Uploads).Concat(uploads.Collectibles.Select(slot => slot.Option.Upload))
            .Concat(uploads.ObjectPageLoad ?? []).Concat(uploads.ObjectPageLooks.Select(option => option.Upload));

    /// <summary>The blocks of <paramref name="set"/>, their runs split at <paramref name="bounds"/>.</summary>
    /// <exception cref="InvalidDataException">The compositions are not whole blocks.</exception>
    private static RoomBlockComposition Of(RoomMetatileCatalog metatiles, RoomBlockSet set, bool ceres, int[] bounds)
    {
        BackgroundTile[] tiles = StockTiles(metatiles, set, ceres);
        var blocks = new RoomBlockCharacterRun[tiles.Length / RoomMetatileFormat.TilesPerBlock][];
        for (int block = 0; block < blocks.Length; block++)
        {
            IEnumerable<int> characters = !ceres && CollectibleSlotBlocks.SlotOf(block, CollectiblePlms.GraphicsSlots) is not null
                ? CollectibleSlotBlocks.CharactersOf(block)
                : tiles.Skip(block * RoomMetatileFormat.TilesPerBlock).Take(RoomMetatileFormat.TilesPerBlock).Select(tile => tile.Tile);
            blocks[block] = [.. characters.Distinct().GroupBy(character => Region(bounds, character)).OrderBy(region => region.Key)
                .Select(region => new RoomBlockCharacterRun(region.Min(), region.Max() - region.Min() + 1))];
        }
        return new RoomBlockComposition(tiles, !ceres, new RoomBlockCharacters(set, blocks));
    }
}
