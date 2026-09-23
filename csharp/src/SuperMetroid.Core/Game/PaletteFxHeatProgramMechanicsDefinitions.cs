namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable control words for the three Samus-in-heat palette programs in bank $8D.
/// </summary>
/// <remarks>
/// Each timed record contains fifteen BGR555 colors between its duration and terminal
/// wait command. Those 720 color words remain live presentation data; this catalog owns
/// only program setup, timing, waits, and loop control.
/// Power, Varia, and Gravity enter at $8D:E45E, $E68A, and $E8B6;
/// each installs heat pre-instruction $E379 and CGRAM byte $0182.
/// Their sixteen records start at $E466, $E692, or $E8BE plus $22*p,
/// p=0..15, and each ends in $C595 wait. Gotos at $E686, $E8B2,
/// and $EADE return to phase zero; p=16 reaches control. All 114
/// mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class PaletteFxHeatProgramMechanicsDefinitions
{
    private static readonly ushort[] PowerDurations =
        [16, 4, 4, 5, 6, 7, 8, 8, 8, 8, 7, 6, 5, 4, 4, 3];
    private static readonly ushort[] ProtectedSuitDurations =
        [16, 4, 4, 5, 6, 7, 8, 8, 8, 8, 7, 6, 5, 4, 4, 16];

    private static readonly PaletteFxHeatProgramDefinition[] Definitions =
    [
        new(PaletteFxHeatSuit.Power, 0xe45e, 0xe686, PowerDurations),
        new(PaletteFxHeatSuit.Varia, 0xe68a, 0xe8b2, ProtectedSuitDurations),
        new(PaletteFxHeatSuit.Gravity, 0xe8b6, 0xeade, ProtectedSuitDurations),
    ];
    private static readonly IReadOnlyList<PaletteFxHeatProgramDefinition> ReadOnlyDefinitions =
        Array.AsReadOnly(Definitions);

    /// <summary>The Power, Varia, and Gravity programs in native selection order.</summary>
    public static IReadOnlyList<PaletteFxHeatProgramDefinition> All => ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across all three programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (PaletteFxHeatProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One suit-specific Samus-in-heat palette control program.</summary>
public sealed class PaletteFxHeatProgramDefinition
{
    /// <summary>Fifteen BGR555 colors follow every timed duration word.</summary>
    public const int ColorsPerFrame = 15;

    private readonly PaletteFxHeatProgramFrameDefinition[] frames;
    private readonly IReadOnlyList<PaletteFxHeatProgramFrameDefinition> readOnlyFrames;

    internal PaletteFxHeatProgramDefinition(
        PaletteFxHeatSuit suit,
        ushort programStart,
        ushort loopInstructionPointer,
        ushort[] durations)
    {
        if (durations.Length != PaletteFxHeatInstructionListDefinitions.PhaseCount)
        {
            throw new ArgumentException(
                $"A heat program requires {PaletteFxHeatInstructionListDefinitions.PhaseCount} durations.",
                nameof(durations));
        }

        Suit = suit;
        ProgramStart = programStart;
        LoopInstructionPointer = loopInstructionPointer;
        frames = new PaletteFxHeatProgramFrameDefinition[durations.Length];
        for (ushort phase = 0; phase < durations.Length; phase++)
        {
            frames[phase] = new PaletteFxHeatProgramFrameDefinition(
                PaletteFxHeatInstructionListDefinitions.Resolve(suit, phase),
                durations[phase]);
        }
        readOnlyFrames = Array.AsReadOnly(frames);
    }

    /// <summary>The mutually exclusive suit palette represented by this program.</summary>
    public PaletteFxHeatSuit Suit { get; }

    /// <summary>The setup entry chosen by palette-FX definition $F761.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The terminal <c>goto</c> command after this suit's final color record.</summary>
    public ushort LoopInstructionPointer { get; }

    /// <summary>The sixteen timed color records addressed by the shared heat phase.</summary>
    /// <remarks>
    /// For p=0..14, let d=min(p,15-p): duration is 16 at p=0,
    /// otherwise min(8,max(4,d+2)). At p=15 it is 3 for Power and
    /// 16 for Varia/Gravity. Thus Power loops in 103 frames and the
    /// protected suits in 116 frames. The two 16-element duration
    /// schedules match every corresponding pinned-ROM word.
    /// </remarks>
    public IReadOnlyList<PaletteFxHeatProgramFrameDefinition> Frames => readOnlyFrames;

    /// <summary>Reads one control word while excluding every BGR555 presentation word.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        int setupOffset = pointer - ProgramStart;
        ushort? setupWord = setupOffset switch
        {
            0 => PaletteFxInstructionCodes.SetPreInstruction,
            2 => PaletteFxPreInstructionCodes.Heat,
            4 => PaletteFxInstructionCodes.SetColorIndex,
            6 => 0x0182,
            _ => null,
        };
        if (setupWord.HasValue)
        {
            value = setupWord.Value;
            return true;
        }

        if (pointer == LoopInstructionPointer)
        {
            value = PaletteFxInstructionCodes.Goto;
            return true;
        }
        if (pointer == unchecked((ushort)(LoopInstructionPointer + sizeof(ushort))))
        {
            value = frames[0].InstructionPointer;
            return true;
        }

        foreach (PaletteFxHeatProgramFrameDefinition frame in frames)
        {
            if (pointer == frame.InstructionPointer)
            {
                value = frame.Duration;
                return true;
            }
            if (pointer == frame.WaitInstructionPointer)
            {
                value = PaletteFxInstructionCodes.Wait;
                return true;
            }
        }

        value = 0;
        return false;
    }
}

/// <summary>One timed fifteen-color record in a Samus-in-heat palette program.</summary>
public readonly record struct PaletteFxHeatProgramFrameDefinition(
    ushort InstructionPointer,
    ushort Duration)
{
    /// <summary>The first live BGR555 presentation word after the duration.</summary>
    /// <remarks>
    /// For Power Suit phases p=0..15, fifteen live colors per record
    /// start at $8D:E468 + $22*p. Five distinct authored rows first
    /// occur at phases 0,1,3,5,7; the bounded row selector is
    /// (0,1,1,2,2,3,3,4,4,3,3,2,2,1,1,1).
    /// All 240 words match the pinned NTSC J/U v1.0 ROM. Channel-wise
    /// floor interpolation between the first and fifth authored rows
    /// misses seven of 75 distinct words, so these rows remain live
    /// presentation data. Phase sixteen reaches the terminal goto.
    /// </remarks>
    public ushort FirstColorPointer => unchecked((ushort)(InstructionPointer + sizeof(ushort)));

    /// <summary>The terminal wait command after fifteen live BGR555 colors.</summary>
    public ushort WaitInstructionPointer => unchecked((ushort)(
        FirstColorPointer + PaletteFxHeatProgramDefinition.ColorsPerFrame * sizeof(ushort)));
}
