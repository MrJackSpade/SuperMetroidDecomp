namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Torizo whose belly palette is animated.</summary>
public enum TorizoBellyPaletteOwner
{
    /// <summary>Bomb Torizo belly object $8D:F759, selecting the six-frame $E2E9 program at CGRAM byte $0132 until enemy slot zero dies.</summary>
    BombTorizo,
    /// <summary>Golden Torizo belly object $8D:F75D, selecting the six-frame $E331 program at CGRAM byte $0132 with its own colors and the same enemy-zero death lifetime.</summary>
    GoldenTorizo,
}

/// <summary>Immutable control words for the Bomb and Golden Torizo belly palette loops.</summary>
/// <remarks>
/// Palette-FX definitions <c>$F759</c> and <c>$F75D</c> install matching six-frame
/// programs with different live BGR555 colors. This catalog owns only color-index setup,
/// enemy-death pre-instruction setup, durations, waits, and loop control.
/// Bomb and Golden programs start at $8D:E2E9 and $8D:E331, each selecting
/// CGRAM byte $0132 and installing enemy-zero-death pre-instruction $E2E0.
/// Records f=0..5 begin at $E2F1 or $E339 plus 10*f, write three live
/// colors, and end in $C595 wait. Gotos at $E32D and $E375 return to
/// the respective first records after a 52-frame cycle; f=6 reaches
/// control. All 36 mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class TorizoBellyPaletteFxProgramMechanicsDefinitions
{
    /// <summary>Bomb Torizo palette-FX identity at $8D:F759.</summary>
    private const ushort BombDefinition = 0xf759;
    /// <summary>Golden Torizo palette-FX identity at $8D:F75D.</summary>
    private const ushort GoldenDefinition = 0xf75d;
    /// <summary>Bomb Torizo belly setup program at $8D:E2E9.</summary>
    private const ushort BombProgram = 0xe2e9;
    /// <summary>Golden Torizo belly setup program at $8D:E331.</summary>
    private const ushort GoldenProgram = 0xe331;

    private sealed class ProgramDefinitions : IReadOnlyList<TorizoBellyPaletteFxProgramDefinition>
    {
        public int Count => 2;
        public TorizoBellyPaletteFxProgramDefinition this[int index] => index switch
        {
            (int)TorizoBellyPaletteOwner.BombTorizo => Create(TorizoBellyPaletteOwner.BombTorizo, BombDefinition, BombProgram),
            (int)TorizoBellyPaletteOwner.GoldenTorizo => Create(TorizoBellyPaletteOwner.GoldenTorizo, GoldenDefinition, GoldenProgram),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

        private static TorizoBellyPaletteFxProgramDefinition Create(
            TorizoBellyPaletteOwner owner, ushort definition, ushort start) =>
            new(owner, definition, start, (ushort)(start + 8),
                (ushort)(start + 8 + FrameCount * FrameByteCount));

        public IEnumerator<TorizoBellyPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Six timed records form one complete belly-color cycle.</summary>
    public const int FrameCount = 6;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 10;

    /// <summary>The byte index of the first Torizo belly color in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x0132;

    /// <summary>The Bomb and Golden Torizo programs in palette-definition order.</summary>
    public static IReadOnlyList<TorizoBellyPaletteFxProgramDefinition> All { get; } = new ProgramDefinitions();

    /// <summary>Resolves one compiled mechanics word across both Torizo programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TorizoBellyPaletteFxProgramDefinition definition in All)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }

    /// <summary>Bounded duration shared by both Torizo belly programs.</summary>
    /// <remarks>
    /// For f=0..5, duration is 10 when f mod 3 is zero, otherwise 8:
    /// exactly 10,8,8,10,8,8 in the pinned ROM, totaling 52 frames.
    /// </remarks>
    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(frame % 3 == 0 ? 10 : 8);
    }
}

/// <summary>One Torizo-specific entry and six-frame belly palette loop.</summary>
public sealed class TorizoBellyPaletteFxProgramDefinition
{
    internal TorizoBellyPaletteFxProgramDefinition(
        TorizoBellyPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort firstFramePointer,
        ushort loopInstructionPointer)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        FirstFramePointer = firstFramePointer;
        LoopInstructionPointer = loopInstructionPointer;
    }

    /// <summary>The Torizo variant represented by this program.</summary>
    public TorizoBellyPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The setup entry for this program.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first timed color record.</summary>
    public ushort FirstFramePointer { get; }

    /// <summary>The terminal <c>goto</c> command after the sixth record.</summary>
    public ushort LoopInstructionPointer { get; }

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer +
            frame * TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one contiguous presentation-color address within a frame.</summary>
    /// <remarks>Color payload derivation remains pending under issue #1165.</remarks>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Reads one mechanics word while excluding live BGR555 colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        int setupOffset = pointer - ProgramStart;
        ushort? setupWord = setupOffset switch
        {
            0 => PaletteFxInstructionCodes.SetColorIndex,
            2 => TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            4 => PaletteFxInstructionCodes.SetPreInstruction,
            6 => PaletteFxPreInstructionCodes.DeleteWhenEnemyZeroDies,
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
            value = FirstFramePointer;
            return true;
        }

        for (int frame = 0;
             frame < TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort framePointer = FramePointer(frame);
            if (pointer == framePointer)
            {
                value = TorizoBellyPaletteFxProgramMechanicsDefinitions.Duration(frame);
                return true;
            }
            if (pointer == unchecked((ushort)(framePointer +
                TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                sizeof(ushort))))
            {
                value = PaletteFxInstructionCodes.Wait;
                return true;
            }
        }

        value = 0;
        return false;
    }
}
