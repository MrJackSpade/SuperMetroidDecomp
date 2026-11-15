using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>The first, second and third of one maridia large snail instruction definitions create result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct MaridiaLargeSnailInstructionDefinitionsCreateResult(RoomEnemySystem First, RoomEnemySlot Second, MaridiaLargeSnailEnemyState Third);
