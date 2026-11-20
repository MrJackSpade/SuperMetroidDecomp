namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Appearance-bearing timed records in the four bank-$90 trail lists, excluding commands and terminators: every record
/// of one frame holds the same appearance (#2348), so the import's document holds one appearance per trail frame, keyed
/// by the frame's installed key (#2556).
/// </summary>
public static class ProjectileTrailVisualDefinitions
{
    /// <summary>Asset filename for editable OBJ appearances of the authored projectile trail records.</summary>
    public const string FileName = "projectile-trails.json";
    /// <summary>Supported JSON schema revision, keying each appearance by the trail frame it shows (#2556).</summary>
    public const int Version = 2;
    /// <summary>The number of trail records.</summary>
    public const int RecordCount = 42;
}
