using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>The first, second, third, fourth and fifth of one skree metaree behavior create result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
/// <param name="Fifth">The fifth.</param>
internal readonly record struct SkreeMetareeBehaviorCreateResult(RoomEnemySystem First, RoomEnemySlot Second, Func<AnimationProgramCursor<SkreeMetareeProgram>> Third, Action<SkreeMetareeProgram> Fourth, Func<bool> Fifth);
