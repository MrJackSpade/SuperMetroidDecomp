using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Verification's native view of the Ceres actors' behaviour identities (#1415): the bank-$A6 routine words naming each
/// member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CeresNativeWords
{
    /// <summary>The routine word naming each <see cref="CeresDoorFunction"/>, stored in the door's variable A.</summary>
    internal static readonly NativeWordIdentities<CeresDoorFunction> DoorFunctions = new(new Dictionary<CeresDoorFunction, ushort>
    {
        [CeresDoorFunction.HandleEarthquakeDuringEscape] = 0xf76b,
        [CeresDoorFunction.HandleEarthquakeDuringEscapeInRidleysRoom] = 0xf770,
        [CeresDoorFunction.RidleyEscapeFloorWall] = 0xf7a5,
        [CeresDoorFunction.RotatingElevatorRoomDefault] = 0xf7bd,
        [CeresDoorFunction.RotatingElevatorRumble] = 0xf7dc,
        [CeresDoorFunction.ElevatorAnimation] = 0xf850,
    });

    /// <summary>The routine word naming each <see cref="CeresSteamFunction"/>, stored in the steam actor's variable A.</summary>
    internal static readonly NativeWordIdentities<CeresSteamFunction> SteamFunctions = new(new Dictionary<CeresSteamFunction, ushort>
    {
        [CeresSteamFunction.None] = 0xeff4,
        [CeresSteamFunction.ApplyRotatingElevatorOffset] = 0xf019,
    });
}
