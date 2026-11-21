using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>Installed tank and room-specific X-ray presentation, independent of room object state.</summary>
public sealed partial class InstalledXrayOverlays
{
    /// <summary>The installed block the X-ray reveal shows for each tank kind.</summary>
    /// <remarks>
    /// The game reveals the four tanks as four particular blocks of the tileset. These are authored
    /// visual choices and reveals.json may replace them, so no universal formula can replace these installed
    /// blocks. Every other uncollected item is revealed as its kind's own picture (#1538).
    /// </remarks>
    private readonly Dictionary<InWorldCollectibleKind, RoomBlockAppearance> tankBlocks;
    /// <summary>Installed room-specific X-ray block lists keyed by overlay.</summary>
    /// <remarks>
    /// Of all the game's room states only Bomb Torizo's selects a special X-ray overlay. It reveals the three
    /// blocks where the animals break out of the wall, and no fourth.
    /// Each overlay reveals the blocks its installation lists, wherever they lie; loading rejects a missing or
    /// duplicate overlay.
    /// </remarks>
    internal readonly Dictionary<XrayRoomOverlayDefinition, XrayRoomOverlayVisual[]> rooms;

    /// <summary>Copies tank blocks and ordered special-room blocks, requiring every overlay source without changing item collection or room object state.</summary>
    /// <param name="tankBlocks">The block the X-ray reveals for each tank kind, keyed by the kind: one for each of <see cref="TankKinds"/> and no other.</param>
    /// <param name="rooms">Nonempty ordered block lists keyed by overlay; each list is copied.</param>
    /// <exception cref="ArgumentNullException">An input sequence or overlay is null.</exception>
    /// <exception cref="InvalidDataException">The tank blocks name other kinds than the tanks or miss one, or the room lists, duplicate overlays or required-overlay coverage are invalid.</exception>
    public InstalledXrayOverlays(IReadOnlyDictionary<InWorldCollectibleKind, RoomBlockAppearance> tankBlocks,
        IEnumerable<XrayRoomOverlayBlocks> rooms)
        : this(tankBlocks, rooms, requiredOverlays: XrayRoomOverlayDefinitions.All)
    {
    }

    /// <summary>Copies validated tank and room overlays and requires each of <paramref name="requiredOverlays"/>.</summary>
    internal InstalledXrayOverlays(IReadOnlyDictionary<InWorldCollectibleKind, RoomBlockAppearance> tankBlocks,
        IEnumerable<XrayRoomOverlayBlocks> rooms,
        IEnumerable<XrayRoomOverlayDefinition> requiredOverlays)
    {
        Ensure.NotNull(tankBlocks);
        Ensure.NotNull(rooms);
        if (tankBlocks.Count != TankKinds.Count || TankKinds.Any(tank => !tankBlocks.ContainsKey(tank)))
            throw new InvalidDataException(
                $"X-ray tank visuals name {string.Join(", ", tankBlocks.Keys)} instead of exactly the tank kinds {string.Join(", ", TankKinds)}.");
        this.tankBlocks = tankBlocks.ToDictionary();
        this.rooms = new Dictionary<XrayRoomOverlayDefinition, XrayRoomOverlayVisual[]>();
        foreach ((XrayRoomOverlayDefinition overlay, IReadOnlyList<XrayRoomOverlayVisual> visuals) in rooms)
        {
            Ensure.NotNull(overlay);
            if (visuals is null || visuals.Count == 0 ||
                !this.rooms.TryAdd(overlay, visuals.ToArray()))
                throw new InvalidDataException($"Invalid X-ray room overlay {overlay.Name}.");
        }
        foreach (XrayRoomOverlayDefinition overlay in Ensure.NotNull(requiredOverlays))
            if (!this.rooms.ContainsKey(overlay))
                throw new InvalidDataException($"Missing required X-ray room overlay {overlay.Name}.");
    }

    /// <summary>The tank kinds, whose installed reveals this holds.</summary>
    public static IReadOnlyCollection<InWorldCollectibleKind> TankKinds { get; } =
        [InWorldCollectibleKind.EnergyTank, InWorldCollectibleKind.MissileTank, InWorldCollectibleKind.SuperMissileTank, InWorldCollectibleKind.PowerBombTank];

    /// <summary>Selects the installed reveal block of tank kind <paramref name="tank"/>, independently of whether it has been collected.</summary>
    /// <returns>The selected appearance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The kind is not a tank.</exception>
    public RoomBlockAppearance TankBlock(InWorldCollectibleKind tank) =>
        tankBlocks.TryGetValue(tank, out RoomBlockAppearance block)
            ? block
            : throw new ArgumentOutOfRangeException(nameof(tank), tank, "The X-ray reveals only a tank by an installed block.");

    /// <summary>Returns a room overlay's installed records in draw order.</summary>
    /// <param name="overlay">The active room state's overlay; a state without one reveals no room blocks.</param>
    /// <returns>The shared copied record list; coordinates are room-block positions with the appearance shown there.</returns>
    /// <exception cref="InvalidDataException">No installed list exists for the overlay.</exception>
    public IReadOnlyList<XrayRoomOverlayVisual> RoomBlocks(XrayRoomOverlayDefinition overlay) =>
        rooms.TryGetValue(Ensure.NotNull(overlay), out XrayRoomOverlayVisual[]? visuals) ? visuals :
        throw new InvalidDataException($"Missing installed X-ray room overlay {overlay.Name}.");
}
