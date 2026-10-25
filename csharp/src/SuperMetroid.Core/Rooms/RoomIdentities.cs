using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Named logical room identities that are actually inspected by translated behavior.
/// Storage addresses remain in the separate ROM-pointer catalogs.
/// </summary>
public static class RoomIdentities
{

    /// <summary>Crateria's first Space Pirate shaft, room pair <c>$00/$1C</c>.</summary>
    public static readonly RoomIdentity CrateriaSpacePirateShaft =
        new(AreaId.Crateria, 28);

    /// <summary>Crateria's Landing Site, where Samus's ship sets down.</summary>
    public static readonly RoomIdentity CrateriaLandingSite =
        new(AreaId.Crateria, 0);

    /// <summary>Crateria's West Ocean, the entrance to the Wrecked Ship.</summary>
    public static readonly RoomIdentity CrateriaWestOcean =
        new(AreaId.Crateria, 5);

    /// <summary>Crateria's Kihunter room.</summary>
    public static readonly RoomIdentity CrateriaKihunterRoom =
        new(AreaId.Crateria, 7);

    /// <summary>Crateria's East Ocean.</summary>
    public static readonly RoomIdentity CrateriaEastOcean =
        new(AreaId.Crateria, 9);

    /// <summary>Crateria's Forgotten Highway Kago room.</summary>
    public static readonly RoomIdentity CrateriaForgottenHighwayKagoRoom =
        new(AreaId.Crateria, 10);

    /// <summary>Crateria's Crab Maze.</summary>
    public static readonly RoomIdentity CrateriaCrabMaze =
        new(AreaId.Crateria, 11);

    /// <summary>Crateria's Forgotten Highway elbow.</summary>
    public static readonly RoomIdentity CrateriaForgottenHighwayElbow =
        new(AreaId.Crateria, 12);

    /// <summary>Crateria's Moat.</summary>
    public static readonly RoomIdentity CrateriaMoat =
        new(AreaId.Crateria, 14);

    /// <summary>
    /// The Brinstar room whose direct dust branch joins the same room-index cases reached
    /// through Tourian's landing-graphics handler.
    /// </summary>
    public static readonly RoomIdentity BrinstarDirectLandingDust =
        new(AreaId.Brinstar, 8);

    /// <summary>
    /// Reproduces the shared room-index test at <c>$91:F0D1</c> without allowing an equal
    /// room byte from an unrelated area to masquerade as the same logical room.
    /// </summary>
    public static bool UsesTourianStyleLandingDust(RoomIdentity room) =>
        (room.Area is AreaId.Brinstar or AreaId.Tourian) &&
        (room.RoomIndex is >= 5 and < 9 or 11);
}
