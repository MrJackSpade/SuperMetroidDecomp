using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Stable visual-file identity for all authored timed projectile instructions.</summary>
public static class ProjectileFrameBindingFormat
{
    /// <summary>JSON resource filename for timed projectile record-to-spritemap assignments, separate from the composition artwork file.</summary>
    public const string FileName = "projectile-frame-bindings.json";
    /// <summary>
    /// Supported schema revision, seven, requiring an explicit visual assignment for every frame by its name, named for what
    /// it shows (#2124) with a frame shown again under its own name once (#2274), each naming its composition by family and
    /// pose (#1762).
    /// </summary>
    public const int Version = 7;
    /// <summary>The frames requiring bindings; not the number of timed records, sprite compositions or updates in one animation.</summary>
    public static int FrameCount => SamusProjectileFrameOrder.Frames.Count;
    /// <summary>Formats a timed record as the canonical JSON key: its frame's name, such as PowerUpFacingRight for the Power beam fired up.</summary>
    /// <param name="record">The timed record's frame.</param>
    /// <returns>The frame's name.</returns>
    internal static string FrameName(SamusProjectileFrameId record) => record.ToString();
}
