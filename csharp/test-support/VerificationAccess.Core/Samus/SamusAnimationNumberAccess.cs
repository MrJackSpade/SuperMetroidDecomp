using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Verification access: Samus's animation frame as the native game numbers it, the place the frame takes in the game's
/// layout of the animation she animates by (#2048), so oracles keep comparing native state and fixtures keep writing it.
/// </summary>
internal static class SamusAnimationNumberAccess
{
    /// <summary>Reads and writes her animation frame by its native number.</summary>
    extension(SamusState samus)
    {
        /// <summary>Her animation frame's native number: its place in the game's layout of the animation she animates by.</summary>
        internal int AnimationNumber
        {
            get => SamusAnimationNumbers.Number(samus.AnimationPose, samus.AnimationFrame);
            set => samus.AnimationFrame = SamusAnimationNumbers.Frame(samus.AnimationPose, value);
        }

        /// <summary>The frame of the animation she animates by that the native game numbers <paramref name="number"/>.</summary>
        internal SamusAnimationFrame AnimationFrameNumbered(int number) => SamusAnimationNumbers.Frame(samus.AnimationPose, number);

        /// <summary>The frame of her pose's animation that the native game numbers <paramref name="number"/>, which starting that animation sets.</summary>
        internal SamusAnimationFrame PoseFrameNumbered(int number) => SamusAnimationNumbers.Frame(samus.Pose, number);
    }

    /// <summary>Reads the cover's installed displacements by native frame number.</summary>
    extension(SuperMetroid.Core.Samus.Artwork.SamusArmCannonPlacement placement)
    {
        /// <summary>The cover's displacement at the frame of <paramref name="pose"/>'s animation the native game numbers <paramref name="number"/>.</summary>
        internal Offset16<RoomPixels> OffsetNumbered(SamusPoseId pose, int number) => placement.Offset(SamusAnimationNumbers.Frame(pose, number));
    }
}

/// <summary>
/// Verification access: frames of a pose's animation by the native game's numbers (#2048). The native game numbers an
/// animation's frames from its start, through each segment's frames, its cue and the values the cue carries.
/// </summary>
internal static class SamusAnimationNumbers
{
    /// <summary>The frame of <paramref name="pose"/>'s animation the native game numbers <paramref name="number"/>.</summary>
    internal static SamusAnimationFrame Frame(SamusPoseId pose, int number) => Frame(SamusAnimationDelays.AnimationForPose(pose), number);

    /// <summary>The frame of <paramref name="animation"/> the native game numbers <paramref name="number"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is negative.</exception>
    internal static SamusAnimationFrame Frame(SamusPoseAnimation animation, int number)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(number);
        int start = 0;
        for (int segment = 0; segment < animation.Segments.Count - 1; segment++)
        {
            int length = animation.Segments[segment].Length;
            if (number < start + length)
                return new SamusAnimationFrame(segment, number - start);
            start += length;
        }
        return new SamusAnimationFrame(animation.Segments.Count - 1, number - start);
    }

    /// <summary>The native number of <paramref name="frame"/> of <paramref name="pose"/>'s animation.</summary>
    internal static int Number(SamusPoseId pose, SamusAnimationFrame frame) => Number(SamusAnimationDelays.AnimationForPose(pose), frame);

    /// <summary>The native number of <paramref name="frame"/> of <paramref name="animation"/>.</summary>
    internal static int Number(SamusPoseAnimation animation, SamusAnimationFrame frame)
    {
        _ = animation.SegmentName(frame);
        return animation.Segments.Take(frame.Segment).Sum(segment => segment.Length) + frame.Frame;
    }

    /// <summary>The target naming the frame of <paramref name="animation"/> the native game numbers <paramref name="number"/>.</summary>
    internal static SamusAnimationTarget Target(SamusPoseAnimation animation, int number)
    {
        SamusAnimationFrame frame = Frame(animation, number);
        return new SamusAnimationTarget(animation.SegmentName(frame), frame.Frame);
    }
}
