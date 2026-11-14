using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's physical foot components for its walk frames and the small-Kraid initial frame. The sprite records are
/// presentation; these offsets and the one shared hitbox govern collision.
/// </summary>
internal static class KraidFootCollisionDefinitions
{

    /// <summary>Number of authored extended foot frames in the walk sequence.</summary>
    internal const int FrameCount = 35;

    /// <summary>The walk frame each Kraid-foot frame shows; the small-Kraid initial frame is physically the first.</summary>
    internal static readonly Dictionary<EnemyFrameId, int> WalkFrames = new()
    {
        [EnemyFrameId.KraidFootNeutral] = 0,
        [EnemyFrameId.KraidFootWalkingForwardSecond] = 1,
        [EnemyFrameId.KraidFootWalkingForwardThird] = 2,
        [EnemyFrameId.KraidFootWalkingForwardFourth] = 3,
        [EnemyFrameId.KraidFootWalkingForwardFifth] = 4,
        [EnemyFrameId.KraidFootWalkingForwardSixth] = 5,
        [EnemyFrameId.KraidFootWalkingForwardSeventh] = 6,
        [EnemyFrameId.KraidFootWalkingForwardEighth] = 7,
        [EnemyFrameId.KraidFootWalkingForwardNinth] = 8,
        [EnemyFrameId.KraidFootWalkingForwardTenth] = 9,
        [EnemyFrameId.KraidFootWalkingForwardEleventh] = 10,
        [EnemyFrameId.KraidFootWalkingForwardTwelfth] = 11,
        [EnemyFrameId.KraidFootWalkingForwardThirteenth] = 12,
        [EnemyFrameId.KraidFootWalkingForwardFourteenth] = 13,
        [EnemyFrameId.KraidFootWalkingForwardFifteenth] = 14,
        [EnemyFrameId.KraidFootWalkingForwardSixteenth] = 15,
        [EnemyFrameId.KraidFootWalkingForwardSeventeenth] = 16,
        [EnemyFrameId.KraidFootWalkingForwardEighteenth] = 17,
        [EnemyFrameId.KraidFootWalkingForwardNineteenth] = 18,
        [EnemyFrameId.KraidFootWalkingForwardTwentieth] = 19,
        [EnemyFrameId.KraidFootWalkingBackwardThirteenth] = 20,
        [EnemyFrameId.KraidFootWalkingForwardTwentyFirst] = 21,
        [EnemyFrameId.KraidFootWalkingForwardTwentySecond] = 22,
        [EnemyFrameId.KraidFootWalkingForwardTwentyThird] = 23,
        [EnemyFrameId.KraidFootWalkingForwardTwentyFourth] = 24,
        [EnemyFrameId.KraidFootWalkingForwardTwentyFifth] = 25,
        [EnemyFrameId.KraidFootWalkingForwardTwentySixth] = 26,
        [EnemyFrameId.KraidFootWalkingForwardTwentySeventh] = 27,
        [EnemyFrameId.KraidFootWalkingForwardTwentyEighth] = 28,
        [EnemyFrameId.KraidFootWalkingForwardTwentyNinth] = 29,
        [EnemyFrameId.KraidFootWalkingForwardThirtieth] = 30,
        [EnemyFrameId.KraidFootWalkingForwardThirtyFirst] = 31,
        [EnemyFrameId.KraidFootWalkingBackwardSecond] = 32,
        [EnemyFrameId.KraidFootWalkingBackwardFirst] = 34,
        [EnemyFrameId.KraidFootStanding] = 0,
    };

    /// <summary>The single shared rectangle every foot component collides with.</summary>
    internal static ImmutableArray<EnemyCollisionHitbox> SharedHitboxes { get; } =
        [new(-6, -6, 6, 6, KraidReactions.BackgroundTouch, EnemyReaction.Ignore)];
}
