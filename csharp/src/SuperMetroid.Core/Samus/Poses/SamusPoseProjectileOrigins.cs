using static SuperMetroid.Core.Samus.Poses.SamusPoseProjectileOriginDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The projectile origin height of each pose.</summary>
internal static class SamusPoseProjectileOrigins
{
    /// <summary>$91:B62D PoseDefinitions Y offset, record stride eight. Beam/Grapple calculations zero-extend this byte, including the drained-pose $FC correction.</summary>
    internal static byte ReadYOffset(SamusPoseId pose) =>
        YOffsets.ContainsKey(pose) ? YOffsets[pose] : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}
