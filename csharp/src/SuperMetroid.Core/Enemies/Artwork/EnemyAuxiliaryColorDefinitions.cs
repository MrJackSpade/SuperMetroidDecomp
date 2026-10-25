using System.Collections.Immutable;
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The auxiliary colour families' bounded dimensions; runtime AI owns cadence and frame selection.</summary>
internal static class EnemyAuxiliaryColorDefinitions
{
    /// <summary>Blue Brinstar face-block four-color cycle, eight frames.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition FaceBlock = new(EnemyAuxiliaryPalette.FaceBlock, 8, 4);
    /// <summary>Dead sidehopper's seven stages of fifteen colors.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition DeadSidehopper = new(EnemyAuxiliaryPalette.DeadSidehopper, 7, 15);
    /// <summary>Golden Torizo body health bands, eight sixteen-color rows.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition GoldenTorizoBody = new(EnemyAuxiliaryPalette.GoldenTorizoBody, 8, 16);
    /// <summary>Golden Torizo belly health bands, eight sixteen-color rows.</summary>
    internal static readonly EnemyAuxiliaryPaletteDefinition GoldenTorizoBelly = new(EnemyAuxiliaryPalette.GoldenTorizoBelly, 8, 16);

    /// <summary>Four imported palette families in stable face-block, corpse, body, and belly order.</summary>
    internal static ImmutableArray<EnemyAuxiliaryPaletteDefinition> All { get; } =
        [FaceBlock, DeadSidehopper, GoldenTorizoBody, GoldenTorizoBelly];
}
