namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge visual identities translated for bank-$8D projectile drawing.</summary>
public static class EnemyProjectileSpritemapDefinitions
{
    /// <summary>Installed editable JSON filename for projectile OAM compositions and presentation-only timed-program frames.</summary>
    public const string FileName = "enemy-projectile-compositions.json";
    /// <summary>Three bank-$8D compositions in the version-1 Ceres-only catalog, retained when merging that legacy override with current stock.</summary>
    public const int LegacyFrameCount = 3;
    /// <summary>Maximum ordered OBJ parts admitted per composition, matching the SNES's 128 physical sprite slots; empty compositions are valid.</summary>
    public const int MaximumParts = 128;
    /// <summary>Sixteen 8-pixel character columns per OBJ atlas row; row times 16 plus column forms the packed character index.</summary>
    public const int TileColumns = 16;
    /// <summary>Schema-level bound of 64 atlas rows; the selected row/column character number must also fit the nine-bit hardware OBJ field during packed-word compilation.</summary>
    public const int TileRows = 64;

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
