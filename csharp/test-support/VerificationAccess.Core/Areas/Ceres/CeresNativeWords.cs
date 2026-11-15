using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Verification's native view of the Ceres actors' behaviour identities (#1415): the bank-$A6 routine words naming each
/// member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CeresNativeWords
{
    /// <summary>The routine word of the escape quake an ordinary door and the elevator room's wall keep, in variable A.</summary>
    internal const ushort EscapeQuakeWord = 0xf76b;

    /// <summary>The routine word of the escape quake the door of Ridley's room keeps, in variable A.</summary>
    internal const ushort RidleyRoomEscapeQuakeWord = 0xf770;

    /// <summary>The routine word of a getaway chamber wall's main behaviour, which only holds its place, in variable A.</summary>
    internal const ushort GetawayChamberWallWord = 0xf7a5;

    /// <summary>The routine word naming each <see cref="CeresElevatorDoorPhase"/>, stored in the elevator door's variable A.</summary>
    internal static readonly NativeWordIdentities<CeresElevatorDoorPhase> ElevatorDoorPhases = new(new Dictionary<CeresElevatorDoorPhase, ushort>
    {
        [CeresElevatorDoorPhase.Intact] = 0xf7bd,
        [CeresElevatorDoorPhase.Exploding] = 0xf7dc,
        [CeresElevatorDoorPhase.Destroyed] = 0xf850,
    });

    /// <summary>
    /// The routine word of the main behaviour <paramref name="slot"/>'s Ceres door or wall runs now (#1953): the cartridge
    /// stored it in the shared enemy's variable A, and each actor runs its own.
    /// </summary>
    /// <exception cref="InvalidDataException">The slot holds no Ceres door or wall.</exception>
    internal static ushort MainBehaviourWord(RoomEnemySlot slot) => slot.Entity switch
    {
        CeresDoorEntity or CeresElevatorRoomWallEntity => EscapeQuakeWord,
        CeresRidleyRoomDoorEntity => RidleyRoomEscapeQuakeWord,
        CeresGetawayChamberWallEntity => GetawayChamberWallWord,
        CeresElevatorDoorEntity => ElevatorDoorPhases.Word(slot.RequireSpeciesState<CeresElevatorDoorState>().Phase),
        var other => throw new InvalidDataException($"Enemy slot {slot.SlotIndex} holds {other?.GetType().Name ?? "no actor"}, not a Ceres door or wall."),
    };

    /// <summary>The routine word naming each <see cref="CeresSteamAction"/>, stored in the steam actor's variable A.</summary>
    internal static readonly NativeWordIdentities<CeresSteamAction> SteamActions = new(new Dictionary<CeresSteamAction, ushort>
    {
        // An ordinary vent's main AI returned at once (#1870).
        [CeresSteamAction.HoldsItsPlace] = 0xeff4,
        [CeresSteamAction.ApplyRotatingElevatorOffset] = 0xf019,
    });

    /// <summary>The steam action a vent's routine word names.</summary>
    internal static CeresSteamAction DecodeSteamAction(ushort word, string what) => SteamActions.Decode(word, what);

    /// <summary>The routine word of a vent's steam action.</summary>
    internal static ushort SteamActionWord(CeresSteamAction action) => SteamActions.Word(action);

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
