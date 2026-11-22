using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The door of Ceres Ridley's room and the animation holding its look, as it initialized.</summary>
/// <param name="Place">The door's place.</param>
/// <param name="Door">The door's animation, which holds its look.</param>
[RoomLifetime]
internal sealed partial record CeresRidleyRoomDoor(RoomEnemyPlace Place, CeresDoorAnimationState Door);
