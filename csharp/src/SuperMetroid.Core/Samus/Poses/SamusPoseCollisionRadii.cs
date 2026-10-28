using static SuperMetroid.Core.Samus.Poses.SamusPoseCollisionDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The vertical collision radius of each pose.</summary>
internal static class SamusPoseCollisionRadii
{
    /// <summary>Native PoseDefinitions column at $91:B62F with eight-byte stride; FD..FF preserve bounded adjacent instruction bytes.</summary>
    internal static byte ReadVerticalRadius(SamusPoseId pose) =>
        VerticalRadii.ContainsKey(pose) ? VerticalRadii[pose] : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}
