using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>The first, second and third of one puyo instruction program definitions create result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct PuyoInstructionProgramDefinitionsCreateResult(RoomEnemySystem First, RoomEnemySlot Second, PuyoEnemyState Third);
