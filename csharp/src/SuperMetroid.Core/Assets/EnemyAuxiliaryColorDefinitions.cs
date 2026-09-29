namespace SuperMetroid.Core.Assets;

/// <summary>Four distinct RGB5 animation sources; values are identities, not composable bits.</summary>
public enum EnemyAuxiliaryPalette
{
    FaceBlock,
    DeadSidehopper,
    GoldenTorizoBody,
    GoldenTorizoBelly,
}

/// <summary>Import provenance and bounded dimensions; runtime AI owns cadence and frame selection.</summary>
internal static class EnemyAuxiliaryColorDefinitions
{
    /// <summary>Blue Brinstar face-block four-color cycle at $A8:E7CC, eight frames.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition FaceBlock = new(
        EnemyAuxiliaryPalette.FaceBlock, 0xa8e7cc, 8, 4, 4);
    /// <summary>Dead sidehopper's seven stages at $A9:EBCC; each native sixteen-color row supplies fifteen colors.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition DeadSidehopper = new(
        EnemyAuxiliaryPalette.DeadSidehopper, 0xa9ebcc, 7, 15, 16);
    /// <summary>Golden Torizo body health bands at $84:8032, eight sixteen-color rows.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition GoldenTorizoBody = new(
        EnemyAuxiliaryPalette.GoldenTorizoBody, 0x848032, 8, 16, 16);
    /// <summary>Golden Torizo belly health bands at $84:8132, eight sixteen-color rows.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition GoldenTorizoBelly = new(
        EnemyAuxiliaryPalette.GoldenTorizoBelly, 0x848132, 8, 16, 16);

    private static readonly EnemyAuxiliaryPaletteDefinition[] Definitions =
        [FaceBlock, DeadSidehopper, GoldenTorizoBody, GoldenTorizoBelly];
    internal static ReadOnlySpan<EnemyAuxiliaryPaletteDefinition> All => Definitions;
}

internal readonly record struct EnemyAuxiliaryPaletteDefinition(
    EnemyAuxiliaryPalette Id, int SourceAddress, int FrameCount, int ColorCount, int NativeFrameStrideColors);

/// <summary>Installed auxiliary-palette document identity, separate from its consumer.</summary>
public static class EnemyAuxiliaryColorFormat
{
    public const string FileName = "enemy-auxiliary-colors.json";
    public const int Version = 1;
}
