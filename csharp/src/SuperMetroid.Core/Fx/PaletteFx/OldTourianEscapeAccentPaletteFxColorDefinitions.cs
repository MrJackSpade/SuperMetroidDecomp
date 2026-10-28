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

    /// <summary>$8D:FBC1 selects $00D2: palette-six/color-nine railings.</summary>
    private const ushort RailingsColorByteIndex = (6 * 16 + 9) * sizeof(ushort);
    /// <summary>$8D:FC5F selects $00AA: palette-five/color-five panels.</summary>
    private const ushort PanelsColorByteIndex = (5 * 16 + 5) * sizeof(ushort);
    /// <summary>The orange-railing and yellow-panel programs in definition order, projecting the two palette destinations onto their separate but rhythm-matched lists.</summary>
    public static IReadOnlyList<OldTourianEscapeAccentPaletteFxColorDefinition> All { get; } =
    [
        new()
        {
            Owner = OldTourianEscapeAccentPaletteOwner.OrangeRailings,
            Object = PaletteFxObject.OldTourianBackgroundRailingsFlashYellow,
            ColorByteIndex = RailingsColorByteIndex,
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.Crateria101, ColorsPerFrame),
        },
        new()
        {
            Owner = OldTourianEscapeAccentPaletteOwner.YellowPanels,
            Object = PaletteFxObject.OldTourianBackgroundPanelsFlashYellow,
            ColorByteIndex = PanelsColorByteIndex,
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.Crateria201, ColorsPerFrame),
        },
    ];
}
