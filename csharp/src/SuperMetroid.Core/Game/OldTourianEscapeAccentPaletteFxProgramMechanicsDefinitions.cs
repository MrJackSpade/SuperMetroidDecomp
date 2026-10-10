namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive old-Tourian escape accent palette program.</summary>
public enum OldTourianEscapeAccentPaletteOwner
{
    /// <summary>PalFxDef_Crateria10 ($8D:FFDD), selecting the three-color orange-railing flicker at $FBC1 from CGRAM byte $D2.</summary>
    OrangeRailings,
    /// <summary>PalFxDef_Crateria20 ($8D:FFE1), selecting the three-color yellow-panel flicker at $FC5F from CGRAM byte $AA with the same fifteen-record cadence.</summary>
    YellowPanels,
}

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
    private static readonly ProgramDefinitions Definitions = new();
    private sealed class ProgramDefinitions : IReadOnlyList<OldTourianEscapeAccentPaletteFxProgramDefinition>
    {
        public int Count => 2;
        public OldTourianEscapeAccentPaletteFxProgramDefinition this[int index] => index switch
        {
            (int)OldTourianEscapeAccentPaletteOwner.OrangeRailings => new(OldTourianEscapeAccentPaletteOwner.OrangeRailings,
                OrangeRailingsDefinitionPointer, OrangeRailingsProgramStart, RailingsColorByteIndex),
            (int)OldTourianEscapeAccentPaletteOwner.YellowPanels => new(OldTourianEscapeAccentPaletteOwner.YellowPanels,
                YellowPanelsDefinitionPointer, YellowPanelsProgramStart, PanelsColorByteIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<OldTourianEscapeAccentPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The orange-railing and yellow-panel programs in definition order.</summary>
    public static IReadOnlyList<OldTourianEscapeAccentPaletteFxProgramDefinition> All =>
        Definitions;

    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
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

    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
        FlickerPass pass = AuthoredCadence[frame / TintPhases];
        int tint = frame % TintPhases;
        return tint == 0 ? pass.NeutralHold : (ushort)(tint == pass.ExtendedTintPhase ? 2 : 1);
    }
}

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxProgramDefinition
{
    internal OldTourianEscapeAccentPaletteFxProgramDefinition(
        OldTourianEscapeAccentPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
    }

    /// <summary>The mutually exclusive escape-accent owner.</summary>
    public OldTourianEscapeAccentPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(
        FirstFramePointer +
        OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount *
        OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >=
            OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frame));
        }
        return unchecked((ushort)(FirstFramePointer + frame *
            OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions
                .ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => (ushort)PaletteFxInstruction.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == LoopInstructionPointer => (ushort)PaletteFxInstruction.Goto,
            var item when item == LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Duration(frame),
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
