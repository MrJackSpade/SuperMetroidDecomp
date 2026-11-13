using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Stable visual-file identity for all authored timed projectile instructions.</summary>
public static class ProjectileFrameBindingFormat
{
    /// <summary>JSON resource filename for timed projectile record-to-spritemap assignments, separate from the composition artwork file.</summary>
    public const string FileName = "projectile-frame-bindings.json";
    /// <summary>Supported schema revision, three, requiring an explicit visual assignment for every timed record by its program and place there.</summary>
    public const int Version = 3;
    /// <summary>The timed records requiring bindings; not the number of sprite compositions or updates in one animation.</summary>
    public static int FrameCount => SamusProjectileFrameOrder.Frames.Count;
    /// <summary>Formats a timed record as the canonical JSON key, such as PowerUpFacingRight_0 for the first Power beam record.</summary>
    /// <param name="record">The timed record; formatting does not validate that it names one.</param>
    /// <returns>The record's program, an underscore and its place among the program's records.</returns>
    internal static string FrameName(SamusProjectileFrame record) =>
        $"{record.Animation}_{record.Show.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
}
