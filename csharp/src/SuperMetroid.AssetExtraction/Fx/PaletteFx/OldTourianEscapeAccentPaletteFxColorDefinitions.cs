using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the old-Tourian railings' and panels' flashes and the colours each writes, for the import.</summary>
public static class OldTourianEscapeAccentPaletteFxColorDefinitions
{

    /// <summary>Each record writes three live BGR555 colors.</summary>
    private const int ColorsPerFrame = 3;

    /// <summary>The orange railings' flash.</summary>
    internal static OldTourianEscapeAccentPaletteFxColorDefinition OldTourianBackgroundRailingsFlashYellow { get; } = new(ColourEffectId.OldTourianBackgroundRailingsFlashYellow, ColorsPerFrame);

    /// <summary>The yellow panels' flash.</summary>
    internal static OldTourianEscapeAccentPaletteFxColorDefinition OldTourianBackgroundPanelsFlashYellow { get; } = new(ColourEffectId.OldTourianBackgroundPanelsFlashYellow, ColorsPerFrame);
}
