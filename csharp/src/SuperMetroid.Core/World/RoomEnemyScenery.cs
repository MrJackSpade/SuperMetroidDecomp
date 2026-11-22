using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's scenery its enemies act on (#2382, #2426): the Ceres turning floor whose elevator platform the elevator door
/// cycles, the elevator shaft's turn the shaft's steam vents are drawn turned with, and the Tourian statue room whose statue
/// the decorations follow down. Each is null in a room without it; the room's enemy system builds the owners that need one
/// only when the room has it, with it required, so a room without it cannot place their enemies.
/// </summary>
/// <param name="Floor">The room's turning floor, or null in a room that shows none.</param>
/// <param name="ShaftTurn">The elevator shaft's turn, or null in a room that is no turning shaft.</param>
/// <param name="TourianStatueRoom">The Tourian statue room, or null in every other room.</param>
/// <param name="Warnings">
/// The warnings the room's escapes write, or null in a room whose tileset has no warning picture sheet: only a room with it
/// places Ceres Ridley, whose getaway writes Ceres's warning, and Mother Brain, whose death types Zebes's (#2554).
/// </param>
[RoomLifetime]
internal sealed partial record RoomEnemyScenery(RoomFloor? Floor, CeresElevatorShaftRoomMainState? ShaftTurn, TourianStatueRoom? TourianStatueRoom,
    RoomWarnings? Warnings);
