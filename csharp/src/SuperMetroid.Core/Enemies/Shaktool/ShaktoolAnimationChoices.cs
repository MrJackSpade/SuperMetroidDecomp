namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Chooses Shaktool's head aim by direction and each segment's collision-recovery animation.</summary>
internal static class ShaktoolAnimationChoices
{
    /// <summary>The head animation aiming along <paramref name="orientation"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="orientation"/> is not one of the eight directions.</exception>
    internal static ShaktoolSegment ForOrientation(CompassDirection orientation) =>
        ShaktoolAnimationDefinitions.OrientationAnimations.TryGetValue(orientation, out ShaktoolSegment segment)
            ? segment
            : throw new InvalidDataException(
                $"Shaktool orientation {orientation} is not one of the eight directions.");

    /// <summary>Returns the collision-recovery animation for one physical segment.</summary>
    /// <exception cref="InvalidDataException"><paramref name="segmentIndex"/> is outside the seven segments.</exception>
    internal static ShaktoolSegment CollisionForSegment(int segmentIndex) =>
        (uint)segmentIndex < (uint)ShaktoolAnimationDefinitions.SegmentAnimations.Length
            ? ShaktoolAnimationDefinitions.SegmentAnimations[segmentIndex].CollisionAnimation
            : throw new InvalidDataException(
                $"Shaktool segment {segmentIndex} is not one of the seven segments.");
}
