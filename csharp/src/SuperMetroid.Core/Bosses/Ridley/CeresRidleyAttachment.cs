using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley as he attaches himself to the room as he initializes (#2231): his body and the encounter it holds.</summary>
/// <param name="Body">His body's place.</param>
/// <param name="Ridley">The encounter his body holds.</param>
internal sealed partial record CeresRidleyAttachment(RoomEnemyPlace Body, CeresRidleyState Ridley);
