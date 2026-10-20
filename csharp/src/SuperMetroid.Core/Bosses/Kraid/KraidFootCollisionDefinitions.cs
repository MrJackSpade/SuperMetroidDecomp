using System.Collections.Immutable;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's physical foot components for the initial frame at $A7:A565 and
/// the 35 selected extended frames at $A7:8CE3-$A7:8F47. The OAM records are presentation;
/// these offsets and the shared $A7:9453 hitbox list govern collision.
/// </summary>
internal static class KraidFootCollisionDefinitions
{
    /// <summary>Bank containing the Kraid foot's extended frame and shared hitbox records.</summary>
    internal const byte Bank = 0xa7;

    /// <summary>First authored Kraid-foot extended frame at $A7:8CE3.</summary>
    internal const ushort FirstFrame = 0x8ce3;

    /// <summary>Small-Kraid initial foot frame at $A7:A565; physically the first walk frame.</summary>
    internal const ushort InitialFrame = 0xa565;

    /// <summary>Each two-component frame occupies 18 bytes in bank $A7.</summary>
    internal const int FrameByteCount = 18;

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

    /// <summary>Maps an initial or 18-byte extended frame pointer to its two collision components.</summary>
    internal static bool TryGetComponents(ushort pointer,
        out ImmutableArray<KraidCollisionComponent> components)
    {
        if (pointer == InitialFrame)
        {
            components = FrameComponents[0];
            return true;
        }

        int distance = pointer - FirstFrame;
        if (distance < 0 || distance % FrameByteCount != 0 ||
            distance / FrameByteCount >= FrameCount)
        {
            components = [];
            return false;
        }

        components = FrameComponents[distance / FrameByteCount];
        return true;
    }

    /// <summary>The single shared rectangle every foot component collides with.</summary>
    internal static ImmutableArray<KraidCollisionHitbox> SharedHitboxes { get; } =
        [new(-6, -6, 6, 6, KraidHitboxTouch.Background, KraidHitboxShot.Inert)];

    /// <summary>Each walk frame's fixed and moving components, both colliding with the shared rectangle.</summary>
    private static readonly ImmutableArray<KraidCollisionComponent>[] FrameComponents =
        [.. Enumerable.Range(0, FrameCount).Select(frame => ImmutableArray.Create<KraidCollisionComponent>(
            new(new(FirstX(frame), FirstY(frame)), SharedHitboxes), new(new(SecondX(frame), SecondY(frame)), SharedHitboxes)))];
}
