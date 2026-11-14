using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Development-tool members of <see cref="NintendoLogoFadePaletteFxColorDefinitions"/>; never linked by player hosts.</summary>
internal static class NintendoLogoFadePaletteFxColorDefinitionsTooling
{
    /// <summary>The one-entry list backing the copyright fade catalog.</summary>
    internal static readonly IReadOnlyList<NintendoLogoFadePaletteFxColorDefinition> ReadOnlyDefinitions = [new()];
    /// <summary>The copyright entry, calculated from its semantic entry contract.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<NintendoLogoFadePaletteFxColorDefinition> All => ReadOnlyDefinitions;
}
