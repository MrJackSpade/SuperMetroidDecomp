namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Appearance-bearing timed records in the four bank-$90 trail lists, excluding commands and terminators.</summary>
public static class ProjectileTrailVisualDefinitions
{
    /// <summary>Asset filename for editable OBJ appearances of the authored projectile trail records.</summary>
    public const string FileName = "projectile-trails.json";
    /// <summary>Supported JSON schema revision requiring all 42 native timed-record appearance keys.</summary>
    public const int Version = 1;
    /// <summary>The trail frames' installed appearance names: seventeen left-ice, seventeen right-ice, four wave and four missile poses.</summary>
    public static IReadOnlyList<string> FrameNames { get; } =
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

    /// <summary>The number of trail frames.</summary>
    public const int FrameCount = 42;

    /// <summary>The trail frames, in the order of <see cref="FrameNames"/>.</summary>
    public static IReadOnlyList<SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame> Frames { get; } =
    [
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce0,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce1,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce2,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce3,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce4,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce5,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce6,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce7,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce8,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce9,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce10,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce11,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce12,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce13,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce14,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce15,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.LeftIce16,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce0,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce1,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce2,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce3,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce4,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce5,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce6,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce7,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce8,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce9,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce10,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce11,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce12,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce13,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce14,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce15,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.RightIce16,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.Wave0,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.Wave1,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.Wave2,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.Wave3,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.Missile0,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.Missile1,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.Missile2,
        SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrame.Missile3,
    ];
}
