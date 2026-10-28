using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed room block compositions; no collision or block behaviour.</summary>
public sealed class RoomMetatileCatalog
{
    /// <summary>Copied lookup by block table; the RoomMetatileAtlas instances are shared and immutable.</summary>
    internal readonly Dictionary<RoomBlockSet, RoomMetatileAtlas> byRoomBlockSet;

    /// <summary>Copies the lookup and requires an entry for every retail graphics set's block table.</summary>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidDataException">A graphics set's block table is missing or null.</exception>
    public RoomMetatileCatalog(RoomMetatileAtlas cre,
        IReadOnlyDictionary<RoomBlockSet, RoomMetatileAtlas> atlases)
    {
        Cre = Ensure.NotNull(cre);
        Ensure.NotNull(atlases);
        for (int graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            RoomBlockSet required = RoomTilesetDefinitions.Get(graphicsSet).Blocks;
            if (!atlases.TryGetValue(required, out RoomMetatileAtlas? value) || value is null)
                throw new InvalidDataException($"RoomMetatileCatalog lacks graphics set ${graphicsSet:X2}'s {required}.");
        }
        byRoomBlockSet = new Dictionary<RoomBlockSet, RoomMetatileAtlas>(atlases);
    }

    /// <summary>The common room elements, shared by every graphics set.</summary>
    public RoomMetatileAtlas Cre { get; }

    /// <summary>The installed RoomMetatileAtlas for <paramref name="set"/>.</summary>
    /// <exception cref="InvalidDataException">None is installed for <paramref name="set"/>.</exception>
    public RoomMetatileAtlas Get(RoomBlockSet set) =>
        byRoomBlockSet.TryGetValue(set, out RoomMetatileAtlas? value)
            ? value
            : throw new InvalidDataException($"No RoomMetatileAtlas is installed for {set}.");
}
