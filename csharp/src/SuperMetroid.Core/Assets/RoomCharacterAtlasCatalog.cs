using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Complete room-character artwork snapshot. Source addresses are stable identities
/// for shared cartridge streams, not instructions to read the ROM during room load.
/// </summary>
public sealed class RoomCharacterAtlasCatalog
{
    private readonly Dictionary<int, RoomCharacterAtlas> bySource;

    public RoomCharacterAtlasCatalog(RoomCharacterAtlas cre,
        IReadOnlyDictionary<int, RoomCharacterAtlas> bySource)
    {
        Cre = cre ?? throw new ArgumentNullException(nameof(cre));
        ArgumentNullException.ThrowIfNull(bySource);
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int source = RoomTilesetDefinitions.Get(graphicsSet).CharacterAddress;
            if (!bySource.TryGetValue(source, out RoomCharacterAtlas? atlas) || atlas is null)
                throw new InvalidDataException(
                    $"Room character catalog lacks graphics set ${graphicsSet:X2} source ${source:X6}.");
        }
        this.bySource = new Dictionary<int, RoomCharacterAtlas>(bySource);
    }

    public RoomCharacterAtlas Cre { get; }

    /// <summary>Resolves an already-compiled room sheet by its native source identity.</summary>
    public RoomCharacterAtlas Get(int sourceAddress)
    {
        if (!bySource.TryGetValue(sourceAddress, out RoomCharacterAtlas? atlas))
            throw new InvalidDataException($"No room character atlas for source ${sourceAddress:X6}.");
        return atlas;
    }
}
