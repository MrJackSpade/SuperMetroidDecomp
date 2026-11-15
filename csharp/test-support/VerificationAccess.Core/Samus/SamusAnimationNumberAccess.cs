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
            get => SamusAnimationDelays.AnimationForPose(samus.AnimationPose).OffsetOf(samus.AnimationFrame);
            set => samus.AnimationFrame = SamusAnimationDelays.AnimationForPose(samus.AnimationPose).FrameAt(value);
        }

        /// <summary>The frame of the animation she animates by that the native game numbers <paramref name="number"/>.</summary>
        internal SamusAnimationFrame AnimationFrameNumbered(int number) =>
            SamusAnimationDelays.AnimationForPose(samus.AnimationPose).FrameAt(number);

        /// <summary>The frame of her pose's animation that the native game numbers <paramref name="number"/>, which starting that animation sets.</summary>
        internal SamusAnimationFrame PoseFrameNumbered(int number) =>
            SamusAnimationDelays.AnimationForPose(samus.Pose).FrameAt(number);

        /// <summary>The frame of her pose's animation that the native game numbers <paramref name="number"/>, as a path names it.</summary>
        internal SamusAnimationPlace PosePlaceNumbered(int number) =>
            new(SamusAnimationDelays.AnimationForPose(samus.Pose), samus.PoseFrameNumbered(number));
    }

    /// <summary>Reads the cover's installed displacements by native frame number.</summary>
    extension(SuperMetroid.Core.Samus.Artwork.SamusArmCannonPlacement placement)
    {
        /// <summary>The cover's displacement at the frame of <paramref name="pose"/>'s animation the native game numbers <paramref name="number"/>.</summary>
        internal Offset16<RoomPixels> OffsetNumbered(SamusPoseId pose, int number) =>
            placement.Offset(SamusAnimationDelays.AnimationForPose(pose), SamusAnimationNumbers.Frame(pose, number));
    }
}

/// <summary>Verification access: frames of a pose's animation by the native game's numbers (#2048).</summary>
internal static class SamusAnimationNumbers
{
    /// <summary>The frame of <paramref name="pose"/>'s animation the native game numbers <paramref name="number"/>.</summary>
    internal static SamusAnimationFrame Frame(SamusPoseId pose, int number) => SamusAnimationDelays.AnimationForPose(pose).FrameAt(number);

    /// <summary>The native number of <paramref name="frame"/> of <paramref name="pose"/>'s animation.</summary>
    internal static int Number(SamusPoseId pose, SamusAnimationFrame frame) => SamusAnimationDelays.AnimationForPose(pose).OffsetOf(frame);
}
