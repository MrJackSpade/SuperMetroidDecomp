using static SuperMetroid.Core.Samus.Poses.SamusPoseAimDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The aim restriction each pose's table byte holds.</summary>
internal static class SamusPoseAimRestrictions
{
    /// <summary>Native PoseDefinitions column at $91:B62C with eight-byte stride; FD..FF preserve bounded adjacent instruction bytes.</summary>
    internal static byte Read(SamusPoseId pose) =>
        AimRestrictions.TryGetValue(pose, out byte value) ? value : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}
