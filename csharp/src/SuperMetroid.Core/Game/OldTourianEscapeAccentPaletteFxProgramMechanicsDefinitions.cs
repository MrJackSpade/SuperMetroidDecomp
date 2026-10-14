namespace SuperMetroid.Core.Game;

/// <summary>Immutable mechanics for the old-Tourian railing and panel flash loops.</summary>
/// <remarks>
/// Definitions <c>$FFDD</c> and <c>$FFE1</c> use identical fifteen-record timing.
/// Their 90 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions
{
    /// <summary><c>PalFxDef_Crateria10</c> at <c>$8D:FFDD</c>.</summary>
    public const ushort OrangeRailingsDefinitionPointer = 0xffdd;

    /// <summary><c>PalFxInstList_Crateria10</c> at <c>$8D:FBC1</c>.</summary>
    public const ushort OrangeRailingsProgramStart = 0xfbc1;

    /// <summary><c>PalFxDef_Crateria20</c> at <c>$8D:FFE1</c>.</summary>
    public const ushort YellowPanelsDefinitionPointer = OrangeRailingsDefinitionPointer + 4;

    /// <summary><c>PalFxInstList_Crateria20</c> at <c>$8D:FC5F</c>.</summary>
    public const ushort YellowPanelsProgramStart = OrangeRailingsProgramStart + 4 + FrameCount * FrameByteCount + 4;

    /// <summary>Both loops contain fifteen timed records.</summary>
    public const int FrameCount = 15;

    /// <summary>Each record writes three live BGR555 colors.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 10;

    /// <summary>One authored flicker pass: neutral hold and optional tint phase held for an extra native tick.</summary>
    private readonly record struct FlickerPass(ushort NeutralHold, int? ExtendedTintPhase);
    /// <summary>$8D:FBC5-FC59/FC63-FCF7: exact neutral holds16/2/32 and extra-tick phases3/none/1 are authored animation choreography. Identical colors receive different holds; regenerating these choices would invent a different rhythm.</summary>
    private static readonly FlickerPass[] AuthoredCadence = [new(16, 3), new(2, null), new(32, 1)];
    /// <summary>Each pass contains the neutral palette followed by four tint stages.</summary>
    private const int TintPhases = 5;

    /// <summary>$8D:FBC1 selects $00D2: palette-six/color-nine railings.</summary>
    private const ushort RailingsColorByteIndex = (6 * 16 + 9) * sizeof(ushort);
    /// <summary>$8D:FC5F selects $00AA: palette-five/color-five panels.</summary>
    private const ushort PanelsColorByteIndex = (5 * 16 + 5) * sizeof(ushort);
    /// <summary>Projects the two palette destinations onto their separate but rhythm-matched lists.</summary>
    private static readonly ProgramDefinitions Definitions = new();
    /// <summary>Enumerates orange railings before yellow panels in definition order.</summary>
    private sealed class ProgramDefinitions : IReadOnlyList<OldTourianEscapeAccentPaletteFxProgramDefinition>
    {
        /// <summary>The escape accent has separate railing and panel palette-FX definitions.</summary>
        public int Count => 2;
        /// <summary>Constructs the selected program with its CGRAM byte and bank 8D entry pointer.</summary>
        public OldTourianEscapeAccentPaletteFxProgramDefinition this[int index] => index switch
        {
            (int)OldTourianEscapeAccentPaletteOwner.OrangeRailings => new(OldTourianEscapeAccentPaletteOwner.OrangeRailings,
                OrangeRailingsDefinitionPointer, OrangeRailingsProgramStart, RailingsColorByteIndex),
            (int)OldTourianEscapeAccentPaletteOwner.YellowPanels => new(OldTourianEscapeAccentPaletteOwner.YellowPanels,
                YellowPanelsDefinitionPointer, YellowPanelsProgramStart, PanelsColorByteIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Visits both accent programs in the same order as the owner enum.</summary>
        public IEnumerator<OldTourianEscapeAccentPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Uses the typed program order for nongeneric collection callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The orange-railing and yellow-panel programs in definition order.</summary>
    public static IReadOnlyList<OldTourianEscapeAccentPaletteFxProgramDefinition> All =>
        Definitions;

    /// <summary>Recognizes setup, timed holds, waits, and loop words in both lists while leaving colors live.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (OldTourianEscapeAccentPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }

    /// <summary>Applies each five-stage pass's authored neutral hold and extra-tick tint phase.</summary>
    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
        FlickerPass pass = AuthoredCadence[frame / TintPhases];
        int tint = frame % TintPhases;
        return tint == 0 ? pass.NeutralHold : (ushort)(tint == pass.ExtendedTintPhase ? 2 : 1);
    }
}
