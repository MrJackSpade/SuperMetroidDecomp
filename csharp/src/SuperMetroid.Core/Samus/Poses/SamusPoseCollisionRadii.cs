using static SuperMetroid.Core.Samus.Poses.SamusPoseCollisionDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The vertical collision radius of each pose.</summary>
internal static class SamusPoseCollisionRadii
{
    /// <summary>
    /// The vertical collision radius of <paramref name="pose"/>, in pixels: its authored radius, or what an unauthored
    /// pose reads (<see cref="SamusUnauthoredPoseDefinitions"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static int ReadVerticalRadius(SamusPoseId pose) =>
        SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out SamusUnauthoredPoseDefinition? unauthored) ? unauthored.VerticalRadius
        : VerticalRadii.TryGetValue(pose, out int radius) ? radius
        : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}
