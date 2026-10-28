namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Chooses Shaktool's head aim by direction and each segment's collision-recovery animation.</summary>
internal static class ShaktoolPrograms
{
    /// <summary>The head animation of an eighth-turn direction bucket.</summary>
    /// <exception cref="InvalidDataException"><paramref name="directionBucket"/> is not one of the eight buckets.</exception>
    internal static ShaktoolProgram ForOrientationBucket(int directionBucket) =>
        ShaktoolInstructionDefinitions.OrientationPrograms.TryGetValue(directionBucket, out ShaktoolProgram program)
            ? program
            : throw new InvalidDataException(
                $"Shaktool orientation bucket ${directionBucket:X4} is invalid.");

    /// <summary>Returns the collision-recovery animation for one physical segment.</summary>
    /// <exception cref="InvalidDataException"><paramref name="segmentIndex"/> is outside the seven segments.</exception>
    internal static ShaktoolProgram CollisionForSegment(int segmentIndex) =>
        (uint)segmentIndex < (uint)ShaktoolInstructionDefinitions.SegmentInstructions.Length
            ? ShaktoolInstructionDefinitions.SegmentInstructions[segmentIndex].CollisionProgram
            : throw new InvalidDataException(
                $"Shaktool instruction segment {segmentIndex} is outside seven records.");
}
