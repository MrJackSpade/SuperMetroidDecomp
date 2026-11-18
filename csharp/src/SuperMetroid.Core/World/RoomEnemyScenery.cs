using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's scenery its enemies act on (#2382): the Ceres turning floor whose elevator platform the elevator door
/// cycles, and the Tourian statue room whose statue the decorations follow down. Each is null in a room without it; the
/// enemies that use one take it at their placement.
/// </summary>
/// <param name="Floor">The room's turning floor, or null in a room that shows none.</param>
/// <param name="TourianStatueRoom">The Tourian statue room, or null in every other room.</param>
[RoomLifetime]
internal sealed record RoomEnemyScenery(RoomFloor? Floor, ITourianStatueRoom? TourianStatueRoom);
