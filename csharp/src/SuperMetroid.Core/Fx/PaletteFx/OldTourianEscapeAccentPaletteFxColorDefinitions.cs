namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the old-Tourian railing and panel flash loops.</summary>
/// <remarks>
/// Definitions <c>$FFDD</c> and <c>$FFE1</c> use identical fifteen-record timing.
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
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.Crateria101, ColorsPerFrame),
        },
        new()
        {
            Owner = OldTourianEscapeAccentPaletteOwner.YellowPanels,
            Object = PaletteFxObject.OldTourianBackgroundPanelsFlashYellow,
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.Crateria201, ColorsPerFrame),
        },
    ];
}
