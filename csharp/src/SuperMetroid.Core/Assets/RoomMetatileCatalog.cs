using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>Complete selected visual block definitions; no level collision or BTS data.</summary>
public sealed class RoomMetatileCatalog
{
    private readonly Dictionary<int, RoomMetatileAtlas> bySource;

    public RoomMetatileCatalog(RoomMetatileAtlas cre,
        IReadOnlyDictionary<int, RoomMetatileAtlas> bySource)
    {
        Cre = cre ?? throw new ArgumentNullException(nameof(cre));
        ArgumentNullException.ThrowIfNull(bySource);
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int source = RoomTilesetDefinitions.Get(graphicsSet).BlockDefinitionsAddress;
            if (!bySource.TryGetValue(source, out RoomMetatileAtlas? atlas) || atlas is null)
                throw new InvalidDataException(
                    $"Room metatile catalog lacks graphics set ${graphicsSet:X2} source ${source:X6}.");
        }
        this.bySource = new Dictionary<int, RoomMetatileAtlas>(bySource);
    }

    public RoomMetatileAtlas Cre { get; }

    public RoomMetatileAtlas Get(int sourceAddress) =>
        bySource.TryGetValue(sourceAddress, out RoomMetatileAtlas? atlas)
            ? atlas
            : throw new InvalidDataException($"No room metatile atlas for source ${sourceAddress:X6}.");
}
