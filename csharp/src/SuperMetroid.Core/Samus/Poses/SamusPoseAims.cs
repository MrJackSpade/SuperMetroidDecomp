using static SuperMetroid.Core.Samus.Poses.SamusPoseAimDefinitions;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What each pose does with Samus's arm cannon.</summary>
internal static class SamusPoseAims
{
    /// <summary>
    /// The aim of <paramref name="pose"/>: its authored aim, or what an unauthored pose reads
    /// (<see cref="SamusUnauthoredPoseDefinitions"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static SamusPoseAim Read(SamusPoseId pose) =>
        SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out SamusUnauthoredPoseDefinition? unauthored) ? unauthored.Aim
        : Aims.TryGetValue(pose, out SamusPoseAim aim) ? aim
        : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");
}
