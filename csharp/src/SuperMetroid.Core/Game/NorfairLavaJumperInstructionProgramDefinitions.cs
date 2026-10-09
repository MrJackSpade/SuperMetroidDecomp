namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Norfair lava jumper's parent and follower programs.
/// Their interleaved spritemap operands are compiled visual identities when
/// installed artwork is bound; diagnostic address spaces may still supply them.
/// </summary>
internal abstract class NorfairLavaJumperInstructionProgramDefinitions
{
    /// <summary><c>InstructionList_NorfairLavaJumper_Hidden</c> at $A2:BE3C.</summary>
    internal const ushort Hidden = 0xbe3c;

    /// <summary>The terminal sleep instruction for the hidden program at $A2:BE40.</summary>
    internal const ushort HiddenSleep = 0xbe40;

    /// <summary><c>InstructionList_NorfairLavaJumper_Jump</c> at $A2:BE42.</summary>
    internal const ushort Jump = 0xbe42;

    /// <summary>The terminal sleep instruction for the jump program at $A2:BE60.</summary>
    internal const ushort JumpSleep = 0xbe60;

    /// <summary><c>InstructionList_NorfairLavaJumper_Follower</c> at $A2:BE62.</summary>
    internal const ushort Follower = 0xbe62;

    /// <summary><c>Instruction_NorfairLavaJumper_SetAnimationFinished</c> at $A2:BE8E.</summary>
    internal const ushort AnimationFinishedCallback = 0xbe8e;

    // Jump pose holds are authored animation cadence (reviewed under #1165).
    /// <summary>Cartridge-authored update durations for the seven successive jump poses.</summary>
    private static readonly ushort[] JumpHolds = [1, 5, 9, 7, 3, 10, 1];

    /// <summary>Number of compiled timing and control words in the hidden, jump, and follower programs.</summary>
    public static int MechanicsWordCount => 23;

    /// <summary>Number of interleaved spritemap operands exposed for installed visual compositions.</summary>
    public static int PresentationWordCount => 14;

    /// <summary>
    /// $A2:BE3C-BE85: hidden pose/sleep, seven-pose jump/callback/sleep,
    /// then two follower startup poses and a four-pose counted loop.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 2) return index == 0 ? new(Hidden, 1) : new(HiddenSleep, CommonEnemyInstructionCodes.Sleep);
        if (index < 9) return new((ushort)(Jump + (index - 2) * 4), JumpHolds[index - 2]);
        if (index == 9) return new((ushort)(JumpSleep - 2), AnimationFinishedCallback);
        if (index == 10) return new(JumpSleep, CommonEnemyInstructionCodes.Sleep);
        if (index < 13) return new((ushort)(Follower + (index - 11) * 4), 1);
        if (index < 15) return new((ushort)(Follower + 8 + (index - 13) * 2), index == 13 ? CommonEnemyInstructionCodes.SetTimer : (ushort)1);
        if (index < 19) return new((ushort)(Follower + 12 + (index - 15) * 4), 1);
        return new((ushort)(Follower + 28 + (index - 19) * 2), (index - 19) switch
        {
            0 => CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate,
            1 => (ushort)(Follower + 12),
            2 => CommonEnemyInstructionCodes.Goto,
            _ => Follower,
        });
    }

    /// <summary>Maps a presentation-selector index to its operand address in the parent or follower instruction lists.</summary>
    /// <param name="index">Zero-based position among the hidden, jump, and follower visual selectors.</param>
    /// <returns>The bank-local address of the selected interleaved spritemap operand.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == 0) return Hidden + 2;
        if (index < 8) return (ushort)(Jump + 2 + (index - 1) * 4);
        if (index < 10) return (ushort)(Follower + 2 + (index - 8) * 4);
        return (ushort)(Follower + 14 + (index - 10) * 4);
    }
    /// <summary>Finds the compiled timing or control value assigned to one instruction-list address.</summary>
    /// <param name="address">Bank-local address of a mechanics-owned word.</param>
    /// <returns>The compiled duration, callback, branch, or sleep-command value at that address.</returns>
    /// <exception cref="InvalidDataException"><paramref name="address"/> is not part of the compiled mechanics layout.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Norfair lava-jumper instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}
