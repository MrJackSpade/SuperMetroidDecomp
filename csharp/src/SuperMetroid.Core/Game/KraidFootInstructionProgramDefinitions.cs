namespace SuperMetroid.Core.Game;

/// <summary>One compiled Kraid-foot mechanics word at its bank-$A7 address.</summary>
internal readonly record struct KraidFootInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, movement callbacks, sound callback, and flow control for Kraid's
/// physical foot actor. Interleaved extended-spritemap operands remain live cartridge
/// presentation data.
/// </summary>
internal static class KraidFootInstructionProgramDefinitions
{
    /// <summary><c>InstList_KraidFoot_Initial</c> at $A7:86E7.</summary>
    internal const ushort Initial = 0x86e7;
    /// <summary><c>InstList_KraidFoot_KraidIsBig_Neutral</c> at $A7:86ED.</summary>
    internal const ushort Neutral = 0x86ed;
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingForward_0</c> at $A7:86F3.</summary>
    internal const ushort WalkingForward = 0x86f3;
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingForward_1</c> at $A7:87BB.</summary>
    internal const ushort WalkingForwardFinished = 0x87bb;
    /// <summary><c>InstList_KraidFoot_LungeForward_0</c> at $A7:87BD.</summary>
    internal const ushort LungeForward = 0x87bd;
    /// <summary><c>InstList_KraidFoot_LungeForward_1</c> at $A7:8885.</summary>
    internal const ushort LungeForwardFinished = 0x8885;
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingBackwards_0</c> at $A7:8887.</summary>
    internal const ushort WalkingBackward = 0x8887;
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingBackwards_1</c> at $A7:8939.</summary>
    internal const ushort WalkingBackwardLoop = 0x8939;
    /// <summary>Adjacent unreferenced fast-backwards program at $A7:893D.</summary>
    internal const ushort AdjacentUnusedFastBackward = 0x893d;

    private static readonly KraidFootInstructionMechanicsWord[] Words;
    private static readonly ushort[] PresentationWords;

    static KraidFootInstructionProgramDefinitions()
    {
        var words = new List<KraidFootInstructionMechanicsWord>(capacity: 193);
        var presentation = new List<ushort>(capacity: 106);

        ushort cursor = Initial;
        AddFrame(words, presentation, ref cursor, 0x7fff);
        AddInstruction(words, ref cursor, CommonEnemyInstructionCodes.Sleep);
        RequireCursor(cursor, Neutral);

        AddFrame(words, presentation, ref cursor, 0x7fff);
        AddInstruction(words, ref cursor, CommonEnemyInstructionCodes.Sleep);
        RequireCursor(cursor, WalkingForward);

        AddForwardProgram(
            words,
            presentation,
            ref cursor,
            fast: false,
            KraidInstructionCodes.Instruction_Kraid_XPositionMinus3);
        RequireCursor(cursor, LungeForward);

        AddForwardProgram(
            words,
            presentation,
            ref cursor,
            fast: true,
            KraidInstructionCodes.Instruction_Kraid_XPositionMinus3_duplicate);
        RequireCursor(cursor, WalkingBackward);

        AddBackwardProgram(words, presentation, ref cursor);
        RequireCursor(cursor, AdjacentUnusedFastBackward);

        Words = words.ToArray();
        PresentationWords = presentation.ToArray();
    }

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KraidFootInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Kraid-foot control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Kraid foot mechanics pointer $A7:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }

    private static void AddForwardProgram(
        List<KraidFootInstructionMechanicsWord> words,
        List<ushort> presentation,
        ref ushort cursor,
        bool fast,
        ushort moveLeftInstruction)
    {
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        for (int frame = 0; frame < 11; frame++)
            AddFrame(words, presentation, ref cursor, fast ? (ushort)1 : (ushort)4);
        AddFrame(words, presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        for (int frame = 0; frame < 5; frame++)
            AddFrame(words, presentation, ref cursor, 1);
        AddFrame(words, presentation, ref cursor, fast ? (ushort)4 : (ushort)0x10);

        AddVerticalAndHorizontal(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(words, presentation, ref cursor, 1);
        AddVerticalAndHorizontal(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(words, presentation, ref cursor, 1);

        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddFrame(words, presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        AddVerticalAndHorizontal(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(words, presentation, ref cursor, 1);
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddFrame(words, presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        AddVerticalAndHorizontal(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(words, presentation, ref cursor, 1);
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddFrame(words, presentation, ref cursor, fast ? (ushort)1 : (ushort)3);

        for (int frame = 0; frame < 3; frame++)
        {
            AddVerticalAndHorizontal(words, ref cursor,
                KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
                moveLeftInstruction);
            AddFrame(words, presentation, ref cursor, 1);
        }
        AddVerticalAndHorizontal(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
            moveLeftInstruction);
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_QueueSFX76_Lib2_Max6);
        AddFrame(words, presentation, ref cursor, 1);

        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddInstruction(words, ref cursor, moveLeftInstruction);
        AddFrame(words, presentation, ref cursor, 1);
        for (int frame = 0; frame < 4; frame++)
        {
            AddInstruction(words, ref cursor, moveLeftInstruction);
            AddFrame(words, presentation, ref cursor, 1);
        }
        if (!fast)
        {
            AddInstruction(words, ref cursor, moveLeftInstruction);
            AddFrame(words, presentation, ref cursor, 1);
            AddFrame(words, presentation, ref cursor, 1);
        }
        else
        {
            AddFrame(words, presentation, ref cursor, 1);
            AddInstruction(words, ref cursor, moveLeftInstruction);
            AddFrame(words, presentation, ref cursor, 1);
        }
        AddInstruction(words, ref cursor, CommonEnemyInstructionCodes.Sleep);
    }

    private static void AddBackwardProgram(
        List<KraidFootInstructionMechanicsWord> words,
        List<ushort> presentation,
        ref ushort cursor)
    {
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_XPositionPlus3);
        AddFrame(words, presentation, ref cursor, 4);
        for (int frame = 0; frame < 6; frame++)
        {
            AddInstruction(words, ref cursor,
                KraidInstructionCodes.Instruction_Kraid_XPositionPlus3);
            AddFrame(words, presentation, ref cursor, 1);
        }
        for (int frame = 0; frame < 4; frame++)
        {
            AddVerticalAndHorizontal(words, ref cursor,
                KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
                KraidInstructionCodes.Instruction_Kraid_XPositionPlus3);
            AddFrame(words, presentation, ref cursor, 1);
        }
        for (int frame = 0; frame < 3; frame++)
        {
            AddVerticalAndHorizontal(words, ref cursor,
                KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
                KraidInstructionCodes.Instruction_Kraid_XPositionPlus3);
            AddFrame(words, presentation, ref cursor, 1);
        }
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking);
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_QueueSFX76_Lib2_Max6);
        AddFrame(words, presentation, ref cursor, 1);
        AddInstruction(words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddFrame(words, presentation, ref cursor, 0x14);
        for (int frame = 0; frame < 8; frame++)
            AddFrame(words, presentation, ref cursor, 4);
        for (int frame = 0; frame < 8; frame++)
            AddFrame(words, presentation, ref cursor, 1);
        AddInstruction(words, ref cursor, CommonEnemyInstructionCodes.Goto);
        AddInstruction(words, ref cursor, WalkingBackward);
    }

    private static void AddVerticalAndHorizontal(
        List<KraidFootInstructionMechanicsWord> words,
        ref ushort cursor,
        ushort vertical,
        ushort horizontal)
    {
        AddInstruction(words, ref cursor, vertical);
        AddInstruction(words, ref cursor, horizontal);
    }

    private static void AddInstruction(
        List<KraidFootInstructionMechanicsWord> words,
        ref ushort cursor,
        ushort instruction)
    {
        words.Add(new(cursor, instruction));
        cursor = unchecked((ushort)(cursor + 2));
    }

    private static void AddFrame(
        List<KraidFootInstructionMechanicsWord> words,
        List<ushort> presentation,
        ref ushort cursor,
        ushort duration)
    {
        words.Add(new(cursor, duration));
        presentation.Add(unchecked((ushort)(cursor + 2)));
        cursor = unchecked((ushort)(cursor + 4));
    }

    private static void RequireCursor(ushort actual, ushort expected)
    {
        if (actual != expected)
        {
            throw new InvalidDataException(
                $"Compiled Kraid foot program ended at $A7:{actual:X4}, expected " +
                $"$A7:{expected:X4}.");
        }
    }
}
