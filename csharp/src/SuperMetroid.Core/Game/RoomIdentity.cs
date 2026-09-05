namespace SuperMetroid.Core.Game;

/// <summary>
/// Logical room identity formed by the cartridge's mutually exclusive area and per-area
/// room bytes. This deliberately does not contain a bank-$8F room-header pointer: pointers
/// identify storage, while this value identifies the room as understood by game logic.
/// </summary>
public readonly record struct RoomIdentity
{
    public RoomIdentity(AreaId area, byte roomIndex)
    {
        // Enum casts can manufacture arbitrary bytes. Validate at construction so a room
        // identity can safely cross table and dispatcher boundaries without another guard.
        _ = AreaIds.ToIndex(area);
        Area = area;
        RoomIndex = roomIndex;
    }

    /// <summary>The room's validated retail world area.</summary>
    public AreaId Area { get; }

    /// <summary>The native room index, whose meaning is local to <see cref="Area"/>.</summary>
    public byte RoomIndex { get; }

    /// <summary>Formats the native logical pair used in diagnostics and debugger watches.</summary>
    public override string ToString() => $"${(byte)Area:X2}/${RoomIndex:X2}";
}

/// <summary>
/// Named logical room identities that are actually inspected by translated behavior.
/// Storage addresses remain in the separate ROM-pointer catalogs.
/// </summary>
public static class RoomIdentities
{
    /// <summary>Crateria's Landing Site, room pair <c>$00/$00</c>.</summary>
    public static readonly RoomIdentity LandingSite = new(AreaId.Crateria, 0x00);

    /// <summary>Crateria's first Space Pirate shaft, room pair <c>$00/$1C</c>.</summary>
    public static readonly RoomIdentity CrateriaSpacePirateShaft =
        new(AreaId.Crateria, 0x1c);

    /// <summary>
    /// The Brinstar room whose direct dust branch joins the same room-index cases reached
    /// through Tourian's landing-graphics handler.
    /// </summary>
    public static readonly RoomIdentity BrinstarDirectLandingDust =
        new(AreaId.Brinstar, 0x08);

    /// <summary>Blue Brinstar's Morph Ball room, room pair <c>$01/$0E</c>.</summary>
    public static readonly RoomIdentity MorphBallRoom = new(AreaId.Brinstar, 0x0e);

    /// <summary>Blue Brinstar's Construction Zone, room pair <c>$01/$0F</c>.</summary>
    public static readonly RoomIdentity ConstructionZone = new(AreaId.Brinstar, 0x0f);

    /// <summary>Blue Brinstar's double-missile room, room pair <c>$01/$1D</c>.</summary>
    public static readonly RoomIdentity BlueBrinstarDoubleMissile =
        new(AreaId.Brinstar, 0x1d);

    /// <summary>Ceres's initial elevator room, room pair <c>$06/$00</c>.</summary>
    public static readonly RoomIdentity CeresElevatorRoom = new(AreaId.Ceres, 0x00);

    /// <summary>
    /// Reproduces the shared room-index test at <c>$91:F0D1</c> without allowing an equal
    /// room byte from an unrelated area to masquerade as the same logical room.
    /// </summary>
    public static bool UsesTourianStyleLandingDust(RoomIdentity room) =>
        (room.Area is AreaId.Brinstar or AreaId.Tourian) &&
        (room.RoomIndex is >= 0x05 and < 0x09 or 0x0b);
}
