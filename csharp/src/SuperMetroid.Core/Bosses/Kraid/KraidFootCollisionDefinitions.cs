using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's physical foot components for its walk frames and the small-Kraid initial frame. The pictures are
/// presentation; these offsets and the one shared hitbox govern collision.
/// </summary>
internal static partial class KraidFootCollisionDefinitions
{
    /// <summary>
    /// The offsets of each foot picture's two components from the foot where its walk first shows the picture, in collision
    /// order (#1867, #2210): the first slides back and forth along the floor through the walk, and the second lifts and comes
    /// down again. A pose that steps the picture moves both by its placement.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, Offset16<RoomPixels>[]> ComponentOffsets = new()
    {
        [EnemyFrameId.KraidFootPlanted] = [new(8, 40), new(0, 0)],
        [EnemyFrameId.KraidFootLifting] = [new(-2, 35), new(5, -15)],
        [EnemyFrameId.KraidFootRaised] = [new(-12, 29), new(12, -26)],
        [EnemyFrameId.KraidFootSwinging] = [new(-14, 28), new(26, -23)],
        [EnemyFrameId.KraidFootReaching] = [new(-24, 40), new(16, -10)],
    };
    /// <summary>The single shared rectangle every foot component collides with.</summary>
    internal static ImmutableArray<EnemyCollisionHitbox> SharedHitboxes { get; } =
        [new(-6, -6, 6, 6, KraidReactions.BodyTouch, EnemyReaction.Ignore)];
}
