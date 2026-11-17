using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What each pose defines (#2323): an authored pose's definition (<see cref="SamusPoseDefinitions"/>), or what an
/// unauthored pose reads (<see cref="SamusUnauthoredPoseDefinitions"/>).
/// </summary>
internal static class SamusPoseTraits
{
    /// <summary>The definition of authored pose <paramref name="pose"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose is unauthored, and reads through its unauthored definition.</exception>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static SamusPoseDefinition Authored(SamusPoseId pose) =>
        SamusPoseDefinitions.Poses.TryGetValue(pose, out SamusPoseDefinition? definition) ? definition
        : SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out _) ? throw new ArgumentOutOfRangeException(
            nameof(pose), pose, "An unauthored pose reads through its unauthored definition.")
        : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");

    /// <summary>
    /// The way the pose faces: its authored facing, or what an unauthored pose reads
    /// (<see cref="SamusUnauthoredPoseDefinitions"/>). The front views face neither way.
    /// </summary>
    internal static SamusFacingDirection ReadFacing(SamusPoseId pose) =>
        SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out SamusUnauthoredPoseDefinition? unauthored)
            ? unauthored.Facing
            : Authored(pose).Facing;

    /// <summary>
    /// The pose's movement type, which selects how she moves: its authored movement, or what an unauthored pose reads
    /// (<see cref="SamusUnauthoredPoseDefinitions"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">The pose has no movement type, so no movement runs it.</exception>
    internal static SamusMovementType ReadMovement(SamusPoseId pose) =>
        SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out SamusUnauthoredPoseDefinition? unauthored)
            ? unauthored.Movement ?? throw new InvalidDataException(
                $"Pose {pose} has no movement type; no movement runs it.")
            : Authored(pose).Movement;

    /// <summary>
    /// What a pose falls back to with no input: its authored fallback, or what an unauthored pose reads
    /// (<see cref="SamusUnauthoredPoseDefinitions"/>).
    /// </summary>
    internal static SamusNoInputFallback ReadNoInputFallback(SamusPoseId pose) =>
        SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out SamusUnauthoredPoseDefinition? unauthored)
            ? unauthored.NoInputFallback
            : Authored(pose).NoInput;
}
