namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Chooses Shaktool's head aim by direction and each segment's collision-recovery animation.</summary>
internal static class ShaktoolPrograms
{
    /// <summary>The head animation aiming along <paramref name="orientation"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="orientation"/> is not one of the eight directions.</exception>
    internal static ShaktoolProgram ForOrientation(CompassDirection orientation) =>
        ShaktoolInstructionDefinitions.OrientationPrograms.TryGetValue(orientation, out ShaktoolProgram program)
            ? program
            : throw new InvalidDataException(
                $"Shaktool orientation {orientation} is not one of the eight directions.");

    /// <summary>Returns the collision-recovery animation for one physical segment.</summary>
    /// <exception cref="InvalidDataException"><paramref name="segmentIndex"/> is outside the seven segments.</exception>
    internal static ShaktoolProgram CollisionForSegment(int segmentIndex) =>
        (uint)segmentIndex < (uint)ShaktoolInstructionDefinitions.SegmentInstructions.Length
            ? ShaktoolInstructionDefinitions.SegmentInstructions[segmentIndex].CollisionProgram
            : throw new InvalidDataException(
                $"Shaktool instruction segment {segmentIndex} is outside seven records.");
}
