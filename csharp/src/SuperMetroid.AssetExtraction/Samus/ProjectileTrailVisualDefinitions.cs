namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Appearance-bearing timed records in the four bank-$90 trail lists, excluding commands and terminators: the import's
/// document holds one appearance per record, and every record of one frame must hold the same (#2348).
/// </summary>
public static class ProjectileTrailVisualDefinitions
{
    /// <summary>Asset filename for editable OBJ appearances of the authored projectile trail records.</summary>
    public const string FileName = "projectile-trails.json";
    /// <summary>Supported JSON schema revision requiring all 42 native timed-record appearance keys.</summary>
    public const int Version = 1;
    /// <summary>The trail records' appearance names in the import's document, in address order: seventeen left-ice, seventeen right-ice, four wave and four missile records.</summary>
    public static IReadOnlyList<string> RecordNames { get; } =
    [
        "trail_B4CB",
        "trail_B4CF",
        "trail_B4D3",
        "trail_B4D7",
        "trail_B4DB",
        "trail_B4DF",
        "trail_B4E5",
        "trail_B4E9",
        "trail_B4EF",
        "trail_B4F5",
        "trail_B4FB",
        "trail_B501",
        "trail_B507",
        "trail_B50D",
        "trail_B513",
        "trail_B519",
        "trail_B51F",
        "trail_B52D",
        "trail_B531",
        "trail_B535",
        "trail_B539",
        "trail_B53D",
        "trail_B541",
        "trail_B547",
        "trail_B54B",
        "trail_B551",
        "trail_B557",
        "trail_B55D",
        "trail_B563",
        "trail_B569",
        "trail_B56F",
        "trail_B575",
        "trail_B57B",
        "trail_B581",
        "trail_B58F",
        "trail_B593",
        "trail_B597",
        "trail_B59B",
        "trail_B5A1",
        "trail_B5A5",
        "trail_B5A9",
        "trail_B5AD",
    ];

    /// <summary>The number of trail records.</summary>
    public const int RecordCount = 42;
}
