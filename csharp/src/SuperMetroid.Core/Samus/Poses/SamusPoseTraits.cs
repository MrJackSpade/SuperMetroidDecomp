namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What each pose defines (#2323), from its definition (<see cref="SamusPoseDefinitions"/>).</summary>
internal static class SamusPoseTraits
{
    /// <summary>The definition of <paramref name="pose"/>.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static SamusPoseDefinition Of(SamusPoseId pose) =>
        SamusPoseDefinitions.Poses.TryGetValue(pose, out SamusPoseDefinition? definition) ? definition
        : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");

    /// <summary>The definition of armed <paramref name="pose"/>, from which shots and the Grapple start.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined or drained, so nothing is fired from it.</exception>
    internal static SamusArmedPoseDefinition Armed(SamusPoseId pose) =>
        Of(pose) as SamusArmedPoseDefinition ?? throw new InvalidOperationException($"Nothing is fired from drained pose {pose}.");
}
