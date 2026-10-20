using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge visual identities translated for bank-$8D projectile drawing.</summary>
public static class EnemyProjectileSpritemapDefinitions
{
    /// <summary>
    /// EnemyProjSpritemaps_Blank_Default at $8D:8000; the native count is zero.
    /// Newly initialized bank-$86 slots select it before their first timed frame.
    /// </summary>
    public const ushort BlankSpritemap = 0x8000;

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

    /// <summary>Ceres arrival frames and Skree/Metaree debris selected by their bank-$86 programs.</summary>
    internal static readonly SpritemapFrameDefinition[] Frames =
    [
        new(0xb1ba, "ceres_elevator_pad_0"),
        new(0xb1d0, "ceres_elevator_pad_1"),
        new(0x846d, "ceres_elevator_platform"),
        new(SkreeMetareeParticleVisualDefinitions.SkreeComposition, "skree_debris"),
        new(SkreeMetareeParticleVisualDefinitions.MetareeComposition, "metaree_debris"),
    ];
}
