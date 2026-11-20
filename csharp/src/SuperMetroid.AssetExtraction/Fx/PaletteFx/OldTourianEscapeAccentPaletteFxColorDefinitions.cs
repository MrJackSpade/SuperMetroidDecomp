using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the old-Tourian railings' and panels' flashes and the colours each writes, for the import.</summary>
public static class OldTourianEscapeAccentPaletteFxColorDefinitions
{

    /// <summary>Each record writes three live BGR555 colors.</summary>
    private const int ColorsPerFrame = 3;

    /// <summary>The orange-railing and yellow-panel programs in definition order, the railings and panels flashing on separate but rhythm-matched lists.</summary>
    public static IReadOnlyList<OldTourianEscapeAccentPaletteFxColorDefinition> All { get; } =
    [
        new()
        {
            Owner = OldTourianEscapeAccentPaletteOwner.OrangeRailings,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.OldTourianBackgroundRailingsFlashYellow, ColorsPerFrame),
        },
        new()
        {
            Owner = OldTourianEscapeAccentPaletteOwner.YellowPanels,
            ColorFrames = PaletteFxColorFrames.Of(ColourEffectId.OldTourianBackgroundPanelsFlashYellow, ColorsPerFrame),
        },
    ];
}
