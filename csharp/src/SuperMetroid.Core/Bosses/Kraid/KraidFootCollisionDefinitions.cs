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
        [EnemyFrameId.KraidFootPlantedTwin] = [new(8, 40), new(0, 0)],
        [EnemyFrameId.KraidFootPlantedOffset2By1] = [new(6, 39), new(-2, -1)],
        [EnemyFrameId.KraidFootPlantedOffset4By2] = [new(4, 38), new(-4, -2)],
        [EnemyFrameId.KraidFootPlantedOffset6By3] = [new(2, 37), new(-6, -3)],
        [EnemyFrameId.KraidFootPlantedOffset8By4] = [new(0, 36), new(-8, -4)],
        [EnemyFrameId.KraidFootLiftingOffset10By19] = [new(-2, 35), new(5, -15)],
        [EnemyFrameId.KraidFootLiftingOffset12By20] = [new(-4, 34), new(3, -16)],
        [EnemyFrameId.KraidFootLiftingOffset14By21] = [new(-6, 33), new(1, -17)],
        [EnemyFrameId.KraidFootLiftingOffset16By22] = [new(-8, 32), new(-1, -18)],
        [EnemyFrameId.KraidFootLiftingOffset18By23] = [new(-10, 31), new(-3, -19)],
        [EnemyFrameId.KraidFootRaisedOffset20By26] = [new(-12, 29), new(12, -26)],
        [EnemyFrameId.KraidFootSwingingOffset22By23] = [new(-14, 28), new(26, -23)],
        [EnemyFrameId.KraidFootSwingingOffset24By21] = [new(-16, 30), new(24, -21)],
        [EnemyFrameId.KraidFootSwingingOffset26By19] = [new(-18, 32), new(22, -19)],
        [EnemyFrameId.KraidFootSwingingOffset28By17] = [new(-20, 34), new(20, -17)],
        [EnemyFrameId.KraidFootSwingingOffset30By15] = [new(-22, 36), new(18, -15)],
        [EnemyFrameId.KraidFootSwingingOffset32By13] = [new(-24, 38), new(16, -13)],
        [EnemyFrameId.KraidFootSwingingOffset34By11] = [new(-26, 40), new(14, -11)],
        [EnemyFrameId.KraidFootReachingOffset32By10] = [new(-24, 40), new(16, -10)],
        [EnemyFrameId.KraidFootReachingOffset30By10] = [new(-22, 40), new(18, -10)],
        [EnemyFrameId.KraidFootReachingOffset28By10] = [new(-20, 40), new(20, -10)],
        [EnemyFrameId.KraidFootRaisedOffset26By15] = [new(-18, 40), new(6, -15)],
        [EnemyFrameId.KraidFootRaisedOffset24By15] = [new(-16, 40), new(8, -15)],
        [EnemyFrameId.KraidFootLiftingOffset22By14] = [new(-14, 40), new(-7, -10)],
        [EnemyFrameId.KraidFootLiftingOffset20By14] = [new(-12, 40), new(-5, -10)],
        [EnemyFrameId.KraidFootLiftingOffset18By14] = [new(-10, 40), new(-3, -10)],
        [EnemyFrameId.KraidFootLiftingOffset16By14] = [new(-8, 40), new(-1, -10)],
        [EnemyFrameId.KraidFootLiftingOffset14By14] = [new(-6, 40), new(1, -10)],
        [EnemyFrameId.KraidFootLiftingOffset12By14] = [new(-4, 40), new(3, -10)],
        [EnemyFrameId.KraidFootLiftingOffset10By14] = [new(-2, 40), new(5, -10)],
        [EnemyFrameId.KraidFootLiftingOffset8By14] = [new(0, 40), new(7, -10)],
        [EnemyFrameId.KraidFootPlantedOffset6By0] = [new(2, 40), new(-6, 0)],
        [EnemyFrameId.KraidFootPlantedOffset4By0] = [new(4, 40), new(-4, 0)],
        [EnemyFrameId.KraidFootPlantedTwinB] = [new(8, 40), new(0, 0)],
        [EnemyFrameId.KraidFootPlanted] = [new(8, 40), new(0, 0)],
    };

    /// <summary>The single shared rectangle every foot component collides with.</summary>
    internal static ImmutableArray<EnemyCollisionHitbox> SharedHitboxes { get; } =
        [new(-6, -6, 6, 6, KraidReactions.BackgroundTouch, EnemyReaction.Ignore)];
}
