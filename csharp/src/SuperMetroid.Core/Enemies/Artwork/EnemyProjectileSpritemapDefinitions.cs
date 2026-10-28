namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge visual identities translated for bank-$8D projectile drawing.</summary>
public static class EnemyProjectileSpritemapDefinitions
{
    /// <summary>Installed editable JSON filename for projectile OAM compositions and presentation-only timed-program frames.</summary>
    public const string FileName = "enemy-projectile-compositions.json";
    /// <summary>The installed names of the Ceres arrival frames and Skree/Metaree debris their bank-$86 programs select.</summary>
    internal static readonly string[] FrameNames =
    [
        "ceres_elevator_pad_0",
        "ceres_elevator_pad_1",
        "ceres_elevator_platform",
        "skree_debris",
        "metaree_debris",
    ];
}
