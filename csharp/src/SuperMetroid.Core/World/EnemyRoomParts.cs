using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// The parts of a room, besides its enemies, that the room's enemies act on (#2062): the room objects that show its
/// enemy-breakable blocks breaking, and its effect, whose liquid Ridley and the Chozo statue move and whose acid surface
/// Crocomire's smoke rises from. Both live as long as the room.
/// </summary>
/// <param name="BreakableTerrain">The room objects that show the room's enemy-breakable blocks breaking.</param>
/// <param name="Fx">The room's effect state.</param>
[RoomScopeBoundary("A construction input, built for one room's enemy system and never kept.")]
internal sealed record EnemyRoomParts(EnemyBreakableTerrainObjects BreakableTerrain, RoomFxOverlayState Fx);
