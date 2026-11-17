using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Cartridge values and helpers of <see cref="ProjectileTrailSources"/> that only verification reads.</summary>
internal static class ProjectileTrailSourcesConstants
{
    /// <summary>The bank-$90 address of the first timed record that shows <paramref name="frame"/>.</summary>
    internal static ushort FrameAddress(ProjectileTrailFrame frame) =>
        ProjectileTrailSources.Records.FirstOrDefault(record => record.Value == frame) is { Key: not 0 } found
            ? found.Key
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, "No trail record shows this frame.");
}
