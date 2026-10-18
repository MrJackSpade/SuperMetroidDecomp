namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the old-Tourian railing and panel flash loops.</summary>
/// <remarks>
/// Definitions <c>$FFDD</c> and <c>$FFE1</c> use identical fifteen-record timing.
/// Their 90 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class OldTourianEscapeAccentPaletteFxColorDefinitions
{

    /// <summary>Both loops contain fifteen timed records.</summary>
    public const int FrameCount = 15;

    /// <summary>Each record writes three live BGR555 colors.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 10;

    /// <summary>$8D:FBC1 selects $00D2: palette-six/color-nine railings.</summary>
    private const ushort RailingsColorByteIndex = (6 * 16 + 9) * sizeof(ushort);
    /// <summary>$8D:FC5F selects $00AA: palette-five/color-five panels.</summary>
    private const ushort PanelsColorByteIndex = (5 * 16 + 5) * sizeof(ushort);
    /// <summary>Projects the two palette destinations onto their separate but rhythm-matched lists.</summary>
    private static readonly ProgramDefinitions Definitions = new();
    /// <summary>Enumerates orange railings before yellow panels in definition order.</summary>
    private sealed class ProgramDefinitions : IReadOnlyList<OldTourianEscapeAccentPaletteFxColorDefinition>
    {
        /// <summary>The escape accent has separate railing and panel palette-FX definitions.</summary>
        public int Count => 2;
        /// <summary>Constructs the selected program with its object, first record and CGRAM byte.</summary>
        public OldTourianEscapeAccentPaletteFxColorDefinition this[int index] => index switch
        {
            (int)OldTourianEscapeAccentPaletteOwner.OrangeRailings => new(OldTourianEscapeAccentPaletteOwner.OrangeRailings,
                PaletteFxObject.OldTourianBackgroundRailingsFlashYellow, 581, RailingsColorByteIndex),
            (int)OldTourianEscapeAccentPaletteOwner.YellowPanels => new(OldTourianEscapeAccentPaletteOwner.YellowPanels,
                PaletteFxObject.OldTourianBackgroundPanelsFlashYellow, 596, PanelsColorByteIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Visits both accent programs in the same order as the owner enum.</summary>
        public IEnumerator<OldTourianEscapeAccentPaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Uses the typed program order for nongeneric collection callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The orange-railing and yellow-panel programs in definition order.</summary>
    public static IReadOnlyList<OldTourianEscapeAccentPaletteFxColorDefinition> All =>
        Definitions;
}
