using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Complete room-character artwork snapshot. Source addresses are stable identities
/// for shared cartridge streams, not instructions to read the ROM during room load.
/// </summary>
public sealed class RoomCharacterAtlasCatalog
{
    private readonly Dictionary<int, RoomCharacterAtlas> bySource;

    /// <summary>Creates a complete installed character-art catalog and verifies that every compiled room graphics set can resolve its selected source.</summary>
    /// <param name="cre">Common room elements sheet uploaded alongside each graphics-set-specific sheet.</param>
    /// <param name="bySource">Atlases keyed by immutable native source address; the mapping is copied, while atlas instances are shared.</param>
    /// <exception cref="ArgumentNullException">The CRE atlas or source mapping is null.</exception>
    /// <exception cref="InvalidDataException">A source selected by a compiled room graphics set is absent or maps to null.</exception>
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

    /// <summary>Common room elements character sheet shared by every room graphics set.</summary>
    public RoomCharacterAtlas Cre { get; }

    /// <summary>SHA-256 of every selected character transfer, including CRE and shared sheets.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromTransfers(
        nameof(RoomCharacterAtlasCatalog), this.bySource, atlas => atlas.Transfer, Cre.Transfer);

    /// <summary>Resolves an already-compiled room sheet by its native source identity.</summary>
    public RoomCharacterAtlas Get(int sourceAddress)
    {
        if (!bySource.TryGetValue(sourceAddress, out RoomCharacterAtlas? atlas))
            throw new InvalidDataException($"No room character atlas for source ${sourceAddress:X6}.");
        return atlas;
    }
}
