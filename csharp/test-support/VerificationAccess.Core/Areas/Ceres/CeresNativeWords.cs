using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Verification's native view of the Ceres actors' behaviour identities (#1415): the bank-$A6 routine words naming each
/// member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CeresNativeWords
{
    /// <summary>The routine word naming each <see cref="CeresDoorAction"/>, stored in the door's variable A.</summary>
    internal static readonly NativeWordIdentities<CeresDoorAction> DoorActions = new(new Dictionary<CeresDoorAction, ushort>
    {
        [CeresDoorAction.HandleEarthquakeDuringEscape] = 0xf76b,
        [CeresDoorAction.HandleEarthquakeDuringEscapeInRidleysRoom] = 0xf770,
        [CeresDoorAction.RidleyEscapeFloorWall] = 0xf7a5,
        [CeresDoorAction.RotatingElevatorRoomDefault] = 0xf7bd,
        [CeresDoorAction.RotatingElevatorRumble] = 0xf7dc,
        [CeresDoorAction.ElevatorAnimation] = 0xf850,
    });

    /// <summary>The routine word naming each <see cref="CeresSteamAction"/>, stored in the steam actor's variable A.</summary>
    internal static readonly NativeWordIdentities<CeresSteamAction> SteamActions = new(new Dictionary<CeresSteamAction, ushort>
    {
        [CeresSteamAction.ApplyRotatingElevatorOffset] = 0xf019,
    });

    /// <summary>The routine word of an ordinary vent's main AI, which returned at once: the vent has no action (#1870).</summary>
    internal const ushort NoSteamActionWord = 0xeff4;

    /// <summary>The steam action a vent's routine word names, or null for the routine that returned at once.</summary>
    internal static CeresSteamAction? DecodeSteamAction(ushort word, string what) =>
        word == NoSteamActionWord ? null : SteamActions.Decode(word, what);

    /// <summary>The routine word of a vent's steam action, or of the routine that returned at once when it has none.</summary>
    internal static ushort SteamActionWord(CeresSteamAction? action) =>
        action is { } named ? SteamActions.Word(named) : NoSteamActionWord;

    /// <summary>The bank-$A6 floor update list naming each <see cref="CeresFloorAnimationFrame"/>.</summary>
    internal static readonly NativeWordIdentities<CeresFloorAnimationFrame> FloorAnimationFrames = new(new Dictionary<CeresFloorAnimationFrame, ushort>
    {
        [CeresFloorAnimationFrame.ElevatorLight] = 0xf904,
        [CeresFloorAnimationFrame.ElevatorDark] = 0xf90e,
        [CeresFloorAnimationFrame.BabyVertical] = 0xace2,
        [CeresFloorAnimationFrame.BabyRound] = 0xacf5,
        [CeresFloorAnimationFrame.BabyHorizontal] = 0xad08,
        [CeresFloorAnimationFrame.WingsRaised] = 0xad49,
        [CeresFloorAnimationFrame.WingsLowered] = 0xad80,
    });
}
