using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The primary half a linked Zebetite half joins as it is spawned (#2554): the half holds its place as its linked half, the
/// enemy in that place in the room, whatever occupies it later.
/// </summary>
/// <param name="Place">The primary half's place.</param>
internal sealed record ZebetitePrimary(RoomEnemyPlace Place);
