namespace SuperMetroid.Core.Game;

/// <summary>One Torizo-specific entry and six-frame belly palette loop.</summary>
public sealed class TorizoBellyPaletteFxProgramDefinition
{
    /// <summary>Stores the native definition and derived instruction offsets for one Torizo palette loop.</summary>
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
            0 => (ushort)PaletteFxInstruction.SetColorIndex,
            2 => TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            4 => (ushort)PaletteFxInstruction.SetPreInstruction,
            6 => (ushort)PaletteFxPreInstruction.DeleteWhenEnemyZeroDies,
            _ => null,
        };
        if (setupWord.HasValue)
        {
            value = setupWord.Value;
            return true;
        }

        if (pointer == LoopInstructionPointer)
        {
            value = (ushort)PaletteFxInstruction.Goto;
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
                value = (ushort)PaletteFxInstruction.Wait;
                return true;
            }
        }

        value = 0;
        return false;
    }
}
