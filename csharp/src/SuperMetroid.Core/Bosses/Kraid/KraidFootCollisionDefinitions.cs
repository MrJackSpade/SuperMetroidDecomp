using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's physical foot components for its walk frames and the small-Kraid initial frame. The pictures are
/// presentation; these offsets and the one shared hitbox govern collision.
/// </summary>
internal static class KraidFootCollisionDefinitions
{
    /// <summary>
    /// The offsets of each foot frame's two components from the foot, in collision order (#1867): the first slides back
    /// and forth along the floor through the walk, and the second lifts and comes down again. The neutral, standing and
    /// last backward-walk frames share the resting placement.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, Offset16<RoomPixels>[]> ComponentOffsets = new()
    {
        [EnemyFrameId.KraidFootNeutral] = [new(8, 40), new(0, 0)],
        [EnemyFrameId.KraidFootWalkingForwardSecond] = [new(6, 39), new(-2, -1)],
        [EnemyFrameId.KraidFootWalkingForwardThird] = [new(4, 38), new(-4, -2)],
        [EnemyFrameId.KraidFootWalkingForwardFourth] = [new(2, 37), new(-6, -3)],
        [EnemyFrameId.KraidFootWalkingForwardFifth] = [new(0, 36), new(-8, -4)],
        [EnemyFrameId.KraidFootWalkingForwardSixth] = [new(-2, 35), new(5, -15)],
        [EnemyFrameId.KraidFootWalkingForwardSeventh] = [new(-4, 34), new(3, -16)],
        [EnemyFrameId.KraidFootWalkingForwardEighth] = [new(-6, 33), new(1, -17)],
        [EnemyFrameId.KraidFootWalkingForwardNinth] = [new(-8, 32), new(-1, -18)],
        [EnemyFrameId.KraidFootWalkingForwardTenth] = [new(-10, 31), new(-3, -19)],
        [EnemyFrameId.KraidFootWalkingForwardEleventh] = [new(-12, 29), new(12, -26)],
        [EnemyFrameId.KraidFootWalkingForwardTwelfth] = [new(-14, 28), new(26, -23)],
        [EnemyFrameId.KraidFootWalkingForwardThirteenth] = [new(-16, 30), new(24, -21)],
        [EnemyFrameId.KraidFootWalkingForwardFourteenth] = [new(-18, 32), new(22, -19)],
        [EnemyFrameId.KraidFootWalkingForwardFifteenth] = [new(-20, 34), new(20, -17)],
        [EnemyFrameId.KraidFootWalkingForwardSixteenth] = [new(-22, 36), new(18, -15)],
        [EnemyFrameId.KraidFootWalkingForwardSeventeenth] = [new(-24, 38), new(16, -13)],
        [EnemyFrameId.KraidFootWalkingForwardEighteenth] = [new(-26, 40), new(14, -11)],
        [EnemyFrameId.KraidFootWalkingForwardNineteenth] = [new(-24, 40), new(16, -10)],
        [EnemyFrameId.KraidFootWalkingForwardTwentieth] = [new(-22, 40), new(18, -10)],
        [EnemyFrameId.KraidFootWalkingBackwardThirteenth] = [new(-20, 40), new(20, -10)],
        [EnemyFrameId.KraidFootWalkingForwardTwentyFirst] = [new(-18, 40), new(6, -15)],
        [EnemyFrameId.KraidFootWalkingForwardTwentySecond] = [new(-16, 40), new(8, -15)],
        [EnemyFrameId.KraidFootWalkingForwardTwentyThird] = [new(-14, 40), new(-7, -10)],
        [EnemyFrameId.KraidFootWalkingForwardTwentyFourth] = [new(-12, 40), new(-5, -10)],
        [EnemyFrameId.KraidFootWalkingForwardTwentyFifth] = [new(-10, 40), new(-3, -10)],
        [EnemyFrameId.KraidFootWalkingForwardTwentySixth] = [new(-8, 40), new(-1, -10)],
        [EnemyFrameId.KraidFootWalkingForwardTwentySeventh] = [new(-6, 40), new(1, -10)],
        [EnemyFrameId.KraidFootWalkingForwardTwentyEighth] = [new(-4, 40), new(3, -10)],
        [EnemyFrameId.KraidFootWalkingForwardTwentyNinth] = [new(-2, 40), new(5, -10)],
        [EnemyFrameId.KraidFootWalkingForwardThirtieth] = [new(0, 40), new(7, -10)],
        [EnemyFrameId.KraidFootWalkingForwardThirtyFirst] = [new(2, 40), new(-6, 0)],
        [EnemyFrameId.KraidFootWalkingBackwardSecond] = [new(4, 40), new(-4, 0)],
        [EnemyFrameId.KraidFootWalkingBackwardFirst] = [new(8, 40), new(0, 0)],
        [EnemyFrameId.KraidFootStanding] = [new(8, 40), new(0, 0)],
    };

    /// <summary>The single shared rectangle every foot component collides with.</summary>
    internal static ImmutableArray<EnemyCollisionHitbox> SharedHitboxes { get; } =
        [new(-6, -6, 6, 6, KraidReactions.BackgroundTouch, EnemyReaction.Ignore)];
}
