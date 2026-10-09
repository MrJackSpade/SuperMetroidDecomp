namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Rio's idle, swooping, and cooldown programs.
/// Their twenty-four spritemap operands select extracted presentation frames.
/// </summary>
internal abstract class RioInstructionProgramDefinitions
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

    /// <summary>Number of compiled address/value words, including eight control-tail words beyond the pose records.</summary>
    public static int MechanicsWordCount => PresentationWordCount + 8;
    /// <summary>Number of frame-duration words that carry visual operands across idle, transition, swoop, and cooldown lists.</summary>
    public static int PresentationWordCount => IdleFrameCount + 2 * TransitionFrameCount + SwoopLoopFrameCount;

    /// <summary>Gets the native address and operand for one word of Rio's compiled idle, swoop, or cooldown programs.</summary>
    /// <param name="index">Zero-based ordinal in the mechanics word sequence.</param>
    /// <returns>The instruction address paired with its duration or control operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
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

    /// <summary>Compiles a word within a frame block and its trailing callback or loop control words.</summary>
    /// <param name="start">Native start address of the instruction list.</param>
    /// <param name="frames">Number of four-byte frame records before the control tail.</param>
    /// <param name="index">Zero-based word ordinal within the frame block and tail.</param>
    /// <param name="duration">Authored duration written into each frame record.</param>
    /// <param name="firstTail">Operand for the first control word after the frames.</param>
    /// <param name="lastTail">Operand for the final control word after the frames.</param>
    /// <returns>The address and value of the selected frame or control word.</returns>
    private static InstructionMechanicsWord BlockWord(ushort start, int frames, int index,
        ushort duration, ushort firstTail, ushort lastTail) => index < frames
            ? new((ushort)(start + index * 4), duration)
            : new((ushort)(start + frames * 4 + (index - frames) * 2), index == frames ? firstTail : lastTail);

    /// <summary>Returns the address of a frame record's visual operand in Rio's instruction lists.</summary>
    /// <param name="index">Zero-based ordinal among all idle, transition, swoop, and cooldown poses.</param>
    /// <returns>The bank-$A2 word address containing the selected spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled pose sequence.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < IdleFrameCount) return (ushort)(Idle + index * 4 + 2);
        index -= IdleFrameCount;
        if (index < TransitionFrameCount) return (ushort)(SwoopingPart1 + index * 4 + 2);
        index -= TransitionFrameCount;
        return index < SwoopLoopFrameCount ? (ushort)(SwoopingPart2 + index * 4 + 2)
            : (ushort)(SwoopCooldown + (index - SwoopLoopFrameCount) * 4 + 2);
    }
    /// <summary>Checks whether an instruction address is one of the pose records' visual operands.</summary>
    /// <param name="address">Address to compare with compiled pose operand locations.</param>
    /// <returns><see langword="true"/> when the address belongs to a presentation word.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        for (int index = 0; index < PresentationWordCount; index++)
        {
            if (PresentationWordAddress(index) == address)
                return true;
        }

        return false;
    }

    /// <summary>Reads the compiled mechanics operand stored at an exact Rio instruction address.</summary>
    /// <param name="address">Bank-$A2 address to resolve.</param>
    /// <returns>The duration or control operand associated with the address.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled mechanics word.</exception>
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
}
