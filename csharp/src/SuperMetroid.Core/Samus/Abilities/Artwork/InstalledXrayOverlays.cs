using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>Installed tank and room-specific X-ray presentation, independent of room object state.</summary>
public sealed class InstalledXrayOverlays
{
    /// <summary>The installed block the X-ray reveal shows for each tank kind, in tank order.</summary>
    /// <remarks>
    /// Issue #1038: the game's data reveals the four tanks as four particular blocks of the tileset. These are authored
    /// visual choices and reveals.json may replace them, so no universal formula can replace this bounded installed
    /// array. Every other uncollected item is revealed as its kind's own picture (#1538).
    /// </remarks>
    internal readonly RoomBlockAppearance[] tankBlocks;
    /// <summary>Installed room-specific X-ray block lists keyed by overlay.</summary>
    /// <remarks>
    /// Issue #1039: among the Japanese and North American v1.0 release's 323 pinned room states, only
    /// Bomb Torizo selects a special X-ray overlay. It reveals three
    /// blocks, block 82 at column fifteen, rows ten through twelve, followed
    /// by zero coordinates. The loop stops there;
    /// what follows is not a fourth block. The independent installation
    /// verifier compared all three stock blocks with the game.
    /// reveals.json may change coordinates and blocks while retaining the
    /// overlay and block count, so the apparent Y sequence is not an
    /// algorithm for all valid installations. Retain the bounded lists
    /// and reject missing or duplicate overlays.
    /// </remarks>
    internal readonly Dictionary<XrayRoomOverlayDefinition, XrayRoomOverlayVisual[]> rooms;

    /// <summary>Copies tank blocks and ordered special-room blocks, requiring all compiled overlay sources without changing item collection or room object state.</summary>
    /// <param name="tankBlocks">Exactly four appearances, one for each tank kind in tank order: Energy, Missile, Super Missile, Power Bomb.</param>
    /// <param name="rooms">Nonempty ordered block lists keyed by overlay; each list is copied.</param>
    /// <exception cref="ArgumentNullException">An input sequence or overlay is null.</exception>
    /// <exception cref="InvalidDataException">Tank values/count, room lists, duplicate overlays, or required-overlay coverage are invalid.</exception>
    public InstalledXrayOverlays(IEnumerable<RoomBlockAppearance> tankBlocks,
        IEnumerable<XrayRoomOverlayBlocks> rooms)
        : this(tankBlocks, rooms, requiredOverlays: XrayRoomOverlayDefinitions.All)
    {
    }

    /// <summary>Copies validated tank and room overlays and requires each of <paramref name="requiredOverlays"/>.</summary>
    internal InstalledXrayOverlays(IEnumerable<RoomBlockAppearance> tankBlocks,
        IEnumerable<XrayRoomOverlayBlocks> rooms,
        IEnumerable<XrayRoomOverlayDefinition> requiredOverlays)
    {
        Ensure.NotNull(tankBlocks);
        Ensure.NotNull(rooms);
        this.tankBlocks = tankBlocks.ToArray();
        if (this.tankBlocks.Length != TankCount)
            throw new InvalidDataException($"X-ray tank visuals require {TankCount} appearances.");
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
    public static int TankCount => RoomObjectCollectibleDrawDefinitions.TankKinds.Length;

    /// <summary>Selects the installed reveal block of tank kind <paramref name="tank"/>, independently of whether it has been collected.</summary>
    /// <returns>The selected appearance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The kind is not a tank.</exception>
    public RoomBlockAppearance TankBlock(InWorldCollectibleKind tank) =>
        Array.IndexOf(RoomObjectCollectibleDrawDefinitions.TankKinds, tank) is var index and >= 0
            ? tankBlocks[index]
            : throw new ArgumentOutOfRangeException(nameof(tank), tank, "The X-ray reveals only a tank by an installed block.");

    /// <summary>Returns a room overlay's installed records in draw order.</summary>
    /// <param name="overlay">The active room state's overlay; a state without one is handled by the caller.</param>
    /// <returns>The shared copied record list; coordinates are room-block positions with the appearance shown there.</returns>
    /// <exception cref="InvalidDataException">No installed list exists for the overlay.</exception>
    public IReadOnlyList<XrayRoomOverlayVisual> RoomBlocks(XrayRoomOverlayDefinition overlay) =>
        rooms.TryGetValue(Ensure.NotNull(overlay), out XrayRoomOverlayVisual[]? visuals) ? visuals :
        throw new InvalidDataException($"Missing installed X-ray room overlay {overlay.Name}.");
}
