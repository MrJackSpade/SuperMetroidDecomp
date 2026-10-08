namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Fake Kraid's walking, action-selection, and spit programs.
/// Interleaved spritemap operands are resolved by the installed visual definitions.
/// </summary>
internal abstract class FakeKraidInstructionProgramDefinitions
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

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - ChooseActionFacingLeft;
        if ((uint)offset >= 2 * (ChooseActionFacingRight - ChooseActionFacingLeft)) return false;
        int local = offset % (ChooseActionFacingRight - ChooseActionFacingLeft);
        return local is >= 4 and <= 16 && local % 4 == 0 ||
            local == 0x1c ||
            local is >= 0x22 and <= 0x2a && (local - 0x22) % 4 == 0 ||
            local is >= 0x32 and <= 0x3e && (local - 0x32) % 6 == 0 ||
            local == 0x42;
    }

    /// <summary>Returns fixed Fake Kraid control or rejects pointers outside the live programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryReadMechanicsWord(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Fake Kraid instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
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
}
