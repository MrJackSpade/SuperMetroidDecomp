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
