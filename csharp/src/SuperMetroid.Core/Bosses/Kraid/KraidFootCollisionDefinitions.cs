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

    /// <summary>The fixed component moves two pixels left per frame, then retraces.</summary>
    internal static short FirstX(int frame)
    {
        CheckFrame(frame);
        return (short)(8 - 2 * Math.Min(frame, 34 - frame));
    }

    /// <summary>The fixed component rises, turns at native pose A/B, then descends and holds.</summary>
    internal static short FirstY(int frame)
    {
        CheckFrame(frame);
        return (short)(frame < 10 ? 40 - frame : frame == 10 ? 29 : frame < 18 ? 28 + 2 * (frame - 11) : 40);
    }

    /// <summary>Moving-foot horizontal placement within native poses0,1,2,3 and the return sequence.</summary>
    /// <remarks>Independently reviewed for #1165 against bank_A7.asm. Pose changes
    /// occur at frames5,10,11,21,23,31; frame17 reverses travel. Each pose's
    /// placement advances by two pixels. Pose2 at frame10 is the raised transition.</remarks>
    internal static short SecondX(int frame)
    {
        CheckFrame(frame);
        return (short)(frame switch
        {
            < 5 => -2 * frame,
            < 10 => 5 - 2 * (frame - 5),
            10 => 12,
            < 18 => 26 - 2 * (frame - 11),
            < 21 => 16 + 2 * (frame - 18),
            < 23 => 6 + 2 * (frame - 21),
            < 31 => -7 + 2 * (frame - 23),
            _ => -6 + 2 * (frame - 31),
        });
    }

    /// <summary>Moving-foot vertical placement: rise, raised pose2, descent, then pose-specific holds.</summary>
    internal static short SecondY(int frame)
    {
        CheckFrame(frame);
        return (short)(frame switch
        {
            < 5 => -frame,
            < 10 => -15 - (frame - 5),
            10 => -26,
            < 18 => -23 + 2 * (frame - 11),
            < 21 => -10,
            < 23 => -15,
            < 31 => -10,
            _ => 0,
        });
    }

    /// <summary>Rejects an extended foot-frame index outside the authored walk sequence.</summary>
    private static void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
    }

    /// <summary>The walk frame each Kraid-foot frame shows; the small-Kraid initial frame is physically the first.</summary>
    private static readonly Dictionary<EnemyFrameId, int> WalkFrames = new()
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

    /// <summary>The two collision components of Kraid-foot frame <paramref name="frame"/>, when it is one.</summary>
    internal static bool TryGetComponents(EnemyFrameId? frame, out ImmutableArray<KraidCollisionComponent> components)
    {
        if (frame is { } shown && WalkFrames.TryGetValue(shown, out int walk))
        {
            components = FrameComponents[walk];
            return true;
        }
        components = [];
        return false;
    }

    /// <summary>The single shared rectangle every foot component collides with.</summary>
    internal static ImmutableArray<KraidCollisionHitbox> SharedHitboxes { get; } =
        [new(-6, -6, 6, 6, KraidHitboxTouch.Background, KraidHitboxShot.Inert)];

    /// <summary>Each walk frame's fixed and moving components, both colliding with the shared rectangle.</summary>
    private static readonly ImmutableArray<KraidCollisionComponent>[] FrameComponents =
        [.. Enumerable.Range(0, FrameCount).Select(frame => ImmutableArray.Create<KraidCollisionComponent>(
            new(new(FirstX(frame), FirstY(frame)), SharedHitboxes), new(new(SecondX(frame), SecondY(frame)), SharedHitboxes)))];
}
