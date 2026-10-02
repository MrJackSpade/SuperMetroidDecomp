namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct FakeKraidInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Fake Kraid's walking, action-selection, and spit programs.
/// Interleaved spritemap operands are resolved by the installed visual definitions.
/// </summary>
internal static class FakeKraidInstructionProgramDefinitions
{
    /// <summary><c>InstList_MiniKraid_ChooseAction</c> at $A6:99AC.</summary>
    internal const ushort ChooseActionFacingLeft = 0x99ac;
    /// <summary><c>InstList_MiniKraid_StepForwards_FacingLeft</c> at $A6:99AE.</summary>
    internal const ushort StepForwardsFacingLeft = 0x99ae;
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate</c> at $A6:99C4.</summary>
    internal const ushort ChooseAlternateActionFacingLeft = 0x99c4;
    /// <summary><c>InstList_MiniKraid_StepBackwards_FacingLeft</c> at $A6:99C6.</summary>
    internal const ushort StepBackwardsFacingLeft = 0x99c6;
    /// <summary><c>InstList_MiniKraid_FireSpit_FacingLeft</c> at $A6:99DC.</summary>
    internal const ushort FireSpitFacingLeft = 0x99dc;
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate_again2</c> at $A6:99FA.</summary>
    internal const ushort ChooseActionFacingRight = 0x99fa;
    /// <summary><c>InstList_MiniKraid_StepForwards_FacingRight</c> at $A6:99FC.</summary>
    internal const ushort StepForwardsFacingRight = 0x99fc;
    /// <summary><c>InstList_MiniKraid_ChooseAction_duplicate_again3</c> at $A6:9A12.</summary>
    internal const ushort ChooseAlternateActionFacingRight = 0x9a12;
    /// <summary><c>InstList_MiniKraid_StepBackwards_FacingRight</c> at $A6:9A14.</summary>
    internal const ushort StepBackwardsFacingRight = 0x9a14;
    /// <summary><c>InstList_MiniKraid_FireSpit_FacingRight</c> at $A6:9A2A.</summary>
    internal const ushort FireSpitFacingRight = 0x9a2a;

    internal static int MechanicsWordCount => 48;
    internal static int PresentationWordCount => 24;

    /// <summary>Enumerates control words in native program order, skipping visual operands.</summary>
    internal static FakeKraidInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int address = ChooseActionFacingLeft; address < FireSpitFacingRight + 24; address += 2)
        {
            if (TryReadMechanicsWord((ushort)address, out ushort value) && index-- == 0)
                return new((ushort)address, value);
        }
        throw new InvalidOperationException("Fake Kraid instruction enumeration is incomplete.");
    }

    /// <summary>Visual operands follow each frame delay in the paired walking and firing programs.</summary>
    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        int facing = index / 12;
        int frame = index % 12;
        int offset = frame switch
        {
            < 4 => 4 + 4 * frame,
            4 => 0x1c,
            < 8 => 0x22 + 4 * (frame - 5),
            < 11 => 0x32 + 6 * (frame - 8),
            _ => 0x42,
        };
        return (ushort)(ChooseActionFacingLeft +
            facing * (ChooseActionFacingRight - ChooseActionFacingLeft) + offset);
    }

    /// <summary>Returns fixed Fake Kraid control or rejects pointers outside the live programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryReadMechanicsWord(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Fake Kraid instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    private static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        bool right = address >= ChooseActionFacingRight;
        ushort choose = right ? ChooseActionFacingRight : ChooseActionFacingLeft;
        int offset = address - choose;
        // The two facing programs have identical control flow. Walking moves after
        // the forward cycle but after the first backward frame. Firing cries,
        // opens the mouth, emits spit, closes it, then chooses the next action.
        int word = offset switch
        {
            0 or 0x18 => EnemyInstructionCodePointers.Instruction_MiniKraid_ChooseAction,
            2 or 0x1a or 0x30 or 0x3c => 16,
            6 or 0xe or 0x20 or 0x28 => 12,
            0xa or 0x24 or 0x36 or 0x40 => 8,
            0x12 or 0x1e => EnemyInstructionCodePointers.Instruction_MiniKraid_Move,
            0x14 or 0x2c or 0x44 => CommonEnemyInstructionCodes.Goto,
            0x16 or 0x46 => choose,
            0x2e => right ? ChooseAlternateActionFacingRight : ChooseAlternateActionFacingLeft,
            0x34 => EnemyInstructionCodePointers.Instruction_MiniKraid_PlayCrySFX,
            0x3a => right
                ? EnemyInstructionCodePointers.Instruction_MiniKraid_FireSpitRight
                : EnemyInstructionCodePointers.Instruction_MiniKraid_FireSpitLeft,
            _ => -1,
        };
        value = unchecked((ushort)word);
        return word >= 0;
    }

    internal static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa60000 &&
        TryReadMechanicsWord(unchecked((ushort)(address & ~1)), out _);
}
