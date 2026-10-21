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
        [EnemyFrameId.KraidFootOam8CE3] = 0,
        [EnemyFrameId.KraidFootOam8CF5] = 1,
        [EnemyFrameId.KraidFootOam8D07] = 2,
        [EnemyFrameId.KraidFootOam8D19] = 3,
        [EnemyFrameId.KraidFootOam8D2B] = 4,
        [EnemyFrameId.KraidFootOam8D3D] = 5,
        [EnemyFrameId.KraidFootOam8D4F] = 6,
        [EnemyFrameId.KraidFootOam8D61] = 7,
        [EnemyFrameId.KraidFootOam8D73] = 8,
        [EnemyFrameId.KraidFootOam8D85] = 9,
        [EnemyFrameId.KraidFootOam8D97] = 10,
        [EnemyFrameId.KraidFootOam8DA9] = 11,
        [EnemyFrameId.KraidFootOam8DBB] = 12,
        [EnemyFrameId.KraidFootOam8DCD] = 13,
        [EnemyFrameId.KraidFootOam8DDF] = 14,
        [EnemyFrameId.KraidFootOam8DF1] = 15,
        [EnemyFrameId.KraidFootOam8E03] = 16,
        [EnemyFrameId.KraidFootOam8E15] = 17,
        [EnemyFrameId.KraidFootOam8E27] = 18,
        [EnemyFrameId.KraidFootOam8E39] = 19,
        [EnemyFrameId.KraidFootOam8E4B] = 20,
        [EnemyFrameId.KraidFootOam8E5D] = 21,
        [EnemyFrameId.KraidFootOam8E6F] = 22,
        [EnemyFrameId.KraidFootOam8E81] = 23,
        [EnemyFrameId.KraidFootOam8E93] = 24,
        [EnemyFrameId.KraidFootOam8EA5] = 25,
        [EnemyFrameId.KraidFootOam8EB7] = 26,
        [EnemyFrameId.KraidFootOam8EC9] = 27,
        [EnemyFrameId.KraidFootOam8EDB] = 28,
        [EnemyFrameId.KraidFootOam8EED] = 29,
        [EnemyFrameId.KraidFootOam8EFF] = 30,
        [EnemyFrameId.KraidFootOam8F11] = 31,
        [EnemyFrameId.KraidFootOam8F23] = 32,
        [EnemyFrameId.KraidFootOam8F47] = 34,
        [EnemyFrameId.KraidFootOamA565] = 0,
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
