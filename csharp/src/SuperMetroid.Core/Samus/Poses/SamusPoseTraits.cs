using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What each pose defines (#2323), from its definition (<see cref="SamusPoseDefinitions"/>).</summary>
internal static class SamusPoseTraits
{
    /// <summary>The definition of <paramref name="pose"/>.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static SamusPoseDefinition Of(SamusPoseId pose) =>
        SamusPoseDefinitions.Poses.TryGetValue(pose, out SamusPoseDefinition? definition) ? definition
        : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");

    /// <summary>The way the pose faces. The front views face neither way.</summary>
    internal static SamusFacingDirection ReadFacing(SamusPoseId pose) => Of(pose).Facing;

    /// <summary>The pose's movement type, which selects how she moves.</summary>
    internal static SamusMovementType ReadMovement(SamusPoseId pose) => Of(pose).Movement;

    /// <summary>What a pose falls back to with no input.</summary>
    internal static SamusNoInputFallback ReadNoInputFallback(SamusPoseId pose) => Of(pose).NoInput;
}
