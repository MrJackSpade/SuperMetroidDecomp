using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The mutually exclusive old-Tourian escape accent palette program.</summary>
public enum OldTourianEscapeAccentPaletteOwner
{
    /// <summary>The old Tourian's orange railings, selecting the three-color orange-railing flicker of the <see cref="ColourEffectColours.OldTourianRailings"/>.</summary>
    OrangeRailings,
    /// <summary>The old Tourian's yellow panels, selecting the three-color yellow-panel flicker of the <see cref="ColourEffectColours.OldTourianPanels"/> with the same fifteen-record cadence.</summary>
    YellowPanels,
}
