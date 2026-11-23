namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// A head pose Botwoon's head shows (#2400): the hidden head's empty frame, which faces no way, or his mouth closed or open
/// facing a way. Only the poses its factories make exist.
/// </summary>
internal readonly partial record struct BotwoonHeadFrame
{
    /// <summary>The pose <paramref name="pose"/> facing <paramref name="facing"/>, or no way for the hidden head.</summary>
    private BotwoonHeadFrame(BotwoonHeadPose pose, CompassDirection? facing)
    {
        Pose = pose;
        Facing = facing;
    }

    /// <summary>What the head shows: nothing, or its mouth closed or open.</summary>
    internal BotwoonHeadPose Pose { get; }

    /// <summary>The way a drawn head faces; the hidden head faces no way.</summary>
    internal CompassDirection? Facing { get; }

    /// <summary>The empty frame of the hidden head.</summary>
    internal static BotwoonHeadFrame Hidden { get; } = new(BotwoonHeadPose.Hidden, null);

    /// <summary>The closed mouth facing <paramref name="facing"/>.</summary>
    internal static BotwoonHeadFrame Closed(CompassDirection facing) => new(BotwoonHeadPose.MouthClosed, facing);

    /// <summary>The open mouth facing <paramref name="facing"/>.</summary>
    internal static BotwoonHeadFrame Open(CompassDirection facing) => new(BotwoonHeadPose.MouthOpen, facing);
}
