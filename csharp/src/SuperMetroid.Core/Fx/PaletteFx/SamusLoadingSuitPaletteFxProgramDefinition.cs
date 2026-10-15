namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxProgramDefinition
{
    /// <summary>Bank-local position of the one-frame record following four counted groups.</summary>
    private const ushort FinalFrameOffset = 0x0140;
    /// <summary>Bank-local position of deletion after the terminal palette record.</summary>
    private const ushort DeleteInstructionOffset = 0x0164;

    /// <summary>Pairs one suit's palette-FX header with its bank-$8D instruction-list entry.</summary>
    internal SamusLoadingSuitPaletteFxProgramDefinition(
        SamusLoadingSuitPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
    }

    /// <summary>The mutually exclusive suit palette.</summary>
    public SamusLoadingSuitPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks><c>$8D:E1F4</c>, <c>$8D:E1F8</c>, or <c>$8D:E1FC</c>.</remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks><c>$8D:DB62</c>, <c>$8D:DCC8</c>, or <c>$8D:DE2E</c>.</remarks>
    public ushort ProgramStart { get; }

    /// <summary>The terminal one-frame record after all counted groups.</summary>
    public ushort FinalFramePointer => unchecked((ushort)(ProgramStart + FinalFrameOffset));

    /// <summary>The terminal <c>delete</c> command after the final record.</summary>
    public ushort DeleteInstructionPointer =>
        unchecked((ushort)(ProgramStart + DeleteInstructionOffset));

    /// <summary>Returns the first record of one counted group.</summary>
    public ushort GroupStartPointer(int group) => unchecked((ushort)(
        ProgramStart + SamusLoadingSuitPaletteFxProgramMechanicsDefinitions
            .GroupStartOffset(group)));

    /// <summary>Returns the byte operand that initializes one group's replay timer.</summary>
    public ushort GroupTimerBytePointer(int group) =>
        unchecked((ushort)(GroupStartPointer(group) - 1));

    /// <summary>Returns one of the nine timed-record pointers.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame == SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount - 1)
            return FinalFramePointer;

        int group = frame / SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup;
        int frameInGroup = frame %
            SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup;
        return unchecked((ushort)(GroupStartPointer(group) + frameInGroup *
            SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
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
            var item when item == ProgramStart + 2 =>
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            var item when item == DeleteInstructionPointer => (ushort)PaletteFxInstruction.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int group = 0;
             group < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupCount;
             group++)
        {
            ushort groupStart = GroupStartPointer(group);
            ushort setTimer = unchecked((ushort)(groupStart - 3));
            ushort decrement = unchecked((ushort)(groupStart +
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup *
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount));
            value = pointer switch
            {
                var item when item == setTimer => (ushort)PaletteFxInstruction.SetTimer,
                var item when item == decrement =>
                    (ushort)PaletteFxInstruction.DecrementTimerAndGoto,
                var item when item == decrement + 2 => groupStart,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        for (int frame = 0;
             frame < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => frame == SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount - 1
                    ? SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FinalFrameDuration
                    : SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupFrameDuration,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }

    /// <summary>Resolves one compiled byte-sized group timer.</summary>
    public bool TryReadMechanicsByte(ushort pointer, out byte value)
    {
        for (int group = 0;
             group < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupCount;
             group++)
        {
            if (pointer != GroupTimerBytePointer(group))
                continue;
            value = SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupTimerValue(group);
            return true;
        }

        value = 0;
        return false;
    }
}
