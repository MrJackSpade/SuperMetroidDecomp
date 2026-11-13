using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the old-Tourian railing and panel flash loops.</summary>
/// <remarks>
/// The two definitions use identical fifteen-record timing.
/// Their 90 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class OldTourianEscapeAccentPaletteFxColorDefinitions
{

    /// <summary>Each record writes three live BGR555 colors.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>The orange-railing and yellow-panel programs in definition order, the railings and panels flashing on separate but rhythm-matched lists.</summary>
    public static IReadOnlyList<OldTourianEscapeAccentPaletteFxColorDefinition> All { get; } =
    [
        new()
        {
            Owner = OldTourianEscapeAccentPaletteOwner.OrangeRailings,
            Object = PaletteFxObject.OldTourianBackgroundRailingsFlashYellow,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.OldTourianBackgroundRailingsFlashYellowCycle, ColorsPerFrame),
        },
        new()
        {
            Owner = OldTourianEscapeAccentPaletteOwner.YellowPanels,
            Object = PaletteFxObject.OldTourianBackgroundPanelsFlashYellow,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.OldTourianBackgroundPanelsFlashYellowCycle, ColorsPerFrame),
        },
    ];
}
