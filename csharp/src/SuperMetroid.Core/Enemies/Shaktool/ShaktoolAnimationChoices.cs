namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Chooses Shaktool's head aim by direction and each segment's collision-recovery animation.</summary>
internal static class ShaktoolAnimationChoices
{
    /// <summary>The head animation of an eighth-turn direction bucket.</summary>
    /// <exception cref="InvalidDataException"><paramref name="directionBucket"/> is not one of the eight buckets.</exception>
    internal static ShaktoolSegment ForOrientationBucket(int directionBucket) =>
        ShaktoolAnimationDefinitions.OrientationAnimations.TryGetValue(directionBucket, out ShaktoolSegment segment)
            ? segment
            : throw new InvalidDataException(
                $"Shaktool orientation bucket {directionBucket} is invalid.");

    /// <summary>Returns the collision-recovery animation for one physical segment.</summary>
    /// <exception cref="InvalidDataException"><paramref name="segmentIndex"/> is outside the seven segments.</exception>
    internal static ShaktoolSegment CollisionForSegment(int segmentIndex) =>
        (uint)segmentIndex < (uint)ShaktoolAnimationDefinitions.SegmentAnimations.Length
            ? ShaktoolAnimationDefinitions.SegmentAnimations[segmentIndex].CollisionAnimation
            : throw new InvalidDataException(
                $"Shaktool instruction segment {segmentIndex} is outside seven records.");
}
