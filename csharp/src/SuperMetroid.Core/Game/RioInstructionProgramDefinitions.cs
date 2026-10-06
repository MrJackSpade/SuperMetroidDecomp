namespace SuperMetroid.Core.Game;

internal readonly record struct RioInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Rio's idle, swooping, and cooldown programs.
/// Their twenty-four spritemap operands select extracted presentation frames.
/// </summary>
internal static class RioInstructionProgramDefinitions
{
    /// <summary><c>InstList_Rio_Idle</c> at $A2:BB4B.</summary>
    internal const ushort Idle = 0xbb4b;

    /// <summary><c>InstList_Rio_PostSwoopIdle</c> at $A2:BB53.</summary>
    internal const ushort PostSwoopIdle = 0xbb53;

    /// <summary><c>InstList_Rio_Swooping_Part1</c> at $A2:BB7F.</summary>
    internal const ushort SwoopingPart1 = 0xbb7f;

    /// <summary><c>InstList_Rio_Swooping_Part2</c> at $A2:BB97.</summary>
    internal const ushort SwoopingPart2 = 0xbb97;

    /// <summary><c>InstList_Rio_SwoopCooldown</c> at $A2:BBA3.</summary>
    internal const ushort SwoopCooldown = 0xbba3;

    /// <summary>The first mechanics constant after Rio's programs, at $A2:BBBB.</summary>
    internal const ushort FirstAdjacentMechanicsData = 0xbbbb;

    /// <summary>$A2:BB4B-BB79: two initial and ten post-swoop idle poses, each four ticks.</summary>
    private const int IdleFrameCount = 12;
    /// <summary>$A2:BB7F-BB8F and BBA3-BBB3: five poses before each animation-finished callback.</summary>
    private const int TransitionFrameCount = 5;
    /// <summary>$A2:BB97-BB9B: two alternating poses while the swoop continues.</summary>
    private const int SwoopLoopFrameCount = 2;
    /// <summary>$A2:BB4B-BB79 idle instruction cadence. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort IdleFrameDuration = 4;
    /// <summary>$A2:BB7F-BBB3 swoop and recovery instruction cadence. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort SwoopFrameDuration = 3;

    internal static int MechanicsWordCount => PresentationWordCount + 8;
    internal static int PresentationWordCount => IdleFrameCount + 2 * TransitionFrameCount + SwoopLoopFrameCount;
    internal static RioInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < IdleFrameCount + 2)
            return BlockWord(Idle, IdleFrameCount, index, IdleFrameDuration, CommonEnemyInstructionCodes.Goto, Idle);
        index -= IdleFrameCount + 2;
        if (index < TransitionFrameCount + 2)
            return BlockWord(SwoopingPart1, TransitionFrameCount, index, SwoopFrameDuration,
                RioInstructionCodes.SetAnimationFinished, CommonEnemyInstructionCodes.Sleep);
        index -= TransitionFrameCount + 2;
        if (index < SwoopLoopFrameCount + 2)
            return BlockWord(SwoopingPart2, SwoopLoopFrameCount, index, SwoopFrameDuration,
                CommonEnemyInstructionCodes.Goto, SwoopingPart2);
        return BlockWord(SwoopCooldown, TransitionFrameCount, index - SwoopLoopFrameCount - 2, SwoopFrameDuration,
            RioInstructionCodes.SetAnimationFinished, CommonEnemyInstructionCodes.Sleep);
    }

    private static RioInstructionMechanicsWord BlockWord(ushort start, int frames, int index,
        ushort duration, ushort firstTail, ushort lastTail) => index < frames
            ? new((ushort)(start + index * 4), duration)
            : new((ushort)(start + frames * 4 + (index - frames) * 2), index == frames ? firstTail : lastTail);

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < IdleFrameCount) return (ushort)(Idle + index * 4 + 2);
        index -= IdleFrameCount;
        if (index < TransitionFrameCount) return (ushort)(SwoopingPart1 + index * 4 + 2);
        index -= TransitionFrameCount;
        return index < SwoopLoopFrameCount ? (ushort)(SwoopingPart2 + index * 4 + 2)
            : (ushort)(SwoopCooldown + (index - SwoopLoopFrameCount) * 4 + 2);
    }
    internal static bool IsPresentationWord(ushort address)
    {
        for (int index = 0; index < PresentationWordCount; index++)
        {
            if (PresentationWordAddress(index) == address)
                return true;
        }

        return false;
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Rio instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
