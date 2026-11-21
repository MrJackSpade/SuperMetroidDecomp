using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the Bomb and Golden Torizo bellies' glows and the colours each writes, for the import.</summary>
/// <remarks>
/// The two bellies show the same records in different colours.
/// </remarks>
public static class TorizoBellyPaletteFxColorDefinitions
{

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    private const int ColorsPerFrame = 3;

    /// <summary>The Bomb Torizo's belly glow.</summary>
    internal static TorizoBellyPaletteFxColorDefinition BombTorizoBelly { get; } = new(ColourEffectId.BombTorizoBelly, ColorsPerFrame);

    /// <summary>The Golden Torizo's belly glow.</summary>
    internal static TorizoBellyPaletteFxColorDefinition GoldenTorizoBelly { get; } = new(ColourEffectId.GoldenTorizoBelly, ColorsPerFrame);

    /// <summary>The Bomb and Golden Torizo programs in palette-definition order.</summary>
    public static IReadOnlyList<TorizoBellyPaletteFxColorDefinition> All { get; } = [BombTorizoBelly, GoldenTorizoBelly];
}
