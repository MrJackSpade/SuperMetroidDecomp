using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's physical foot components for its walk frames and the small-Kraid initial frame. The OAM records are
/// presentation; these offsets and the one shared hitbox govern collision.
/// </summary>
internal static class KraidFootCollisionDefinitions
{

    /// <summary>Number of authored extended foot frames in the walk sequence.</summary>
    internal const int FrameCount = 35;

    /// <summary>The walk frame each Kraid-foot frame shows; the small-Kraid initial frame is physically the first.</summary>
    internal static readonly Dictionary<EnemyFrameId, int> WalkFrames = new()
    {
        [EnemyFrameId.KraidFoot0] = 0,
        [EnemyFrameId.KraidFoot1] = 1,
        [EnemyFrameId.KraidFoot2] = 2,
        [EnemyFrameId.KraidFoot3] = 3,
        [EnemyFrameId.KraidFoot4] = 4,
        [EnemyFrameId.KraidFoot5] = 5,
        [EnemyFrameId.KraidFoot6] = 6,
        [EnemyFrameId.KraidFoot7] = 7,
        [EnemyFrameId.KraidFoot8] = 8,
        [EnemyFrameId.KraidFoot9] = 9,
        [EnemyFrameId.KraidFoot10] = 10,
        [EnemyFrameId.KraidFoot11] = 11,
        [EnemyFrameId.KraidFoot12] = 12,
        [EnemyFrameId.KraidFoot13] = 13,
        [EnemyFrameId.KraidFoot14] = 14,
        [EnemyFrameId.KraidFoot15] = 15,
        [EnemyFrameId.KraidFoot16] = 16,
        [EnemyFrameId.KraidFoot17] = 17,
        [EnemyFrameId.KraidFoot18] = 18,
        [EnemyFrameId.KraidFoot19] = 19,
        [EnemyFrameId.KraidFoot20] = 20,
        [EnemyFrameId.KraidFoot21] = 21,
        [EnemyFrameId.KraidFoot22] = 22,
        [EnemyFrameId.KraidFoot23] = 23,
        [EnemyFrameId.KraidFoot24] = 24,
        [EnemyFrameId.KraidFoot25] = 25,
        [EnemyFrameId.KraidFoot26] = 26,
        [EnemyFrameId.KraidFoot27] = 27,
        [EnemyFrameId.KraidFoot28] = 28,
        [EnemyFrameId.KraidFoot29] = 29,
        [EnemyFrameId.KraidFoot30] = 30,
        [EnemyFrameId.KraidFoot31] = 31,
        [EnemyFrameId.KraidFoot32] = 32,
        [EnemyFrameId.KraidFoot33] = 34,
        [EnemyFrameId.KraidFootStanding] = 0,
    };

    /// <summary>The single shared rectangle every foot component collides with.</summary>
    internal static ImmutableArray<KraidCollisionHitbox> SharedHitboxes { get; } =
        [new(-6, -6, 6, 6, KraidHitboxTouch.Background, KraidHitboxShot.Inert)];
}
