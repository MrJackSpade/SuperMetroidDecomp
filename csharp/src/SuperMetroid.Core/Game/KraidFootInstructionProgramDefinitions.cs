namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing, movement callbacks, sound callback, and flow control for Kraid's
/// physical foot actor. Interleaved selections resolve compiled identities to installed
/// extended OAM compositions.
/// </summary>
internal abstract class KraidFootInstructionProgramDefinitions
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

    internal static void Generate(ref MechanicsSelection words, ref PresentationSelection presentation)
    {
        ushort cursor = Initial;
        AddFrame(ref words, ref presentation, ref cursor, 0x7fff);
        AddInstruction(ref words, ref cursor, CommonEnemyInstructionCodes.Sleep);
        RequireCursor(cursor, Neutral);

        AddFrame(ref words, ref presentation, ref cursor, 0x7fff);
        AddInstruction(ref words, ref cursor, CommonEnemyInstructionCodes.Sleep);
        RequireCursor(cursor, WalkingForward);

        AddForwardProgram(
            ref words,
            ref presentation,
            ref cursor,
            fast: false,
            (ushort)KraidFootInstruction.Instruction_Kraid_XPositionMinus3);
        RequireCursor(cursor, LungeForward);

        AddForwardProgram(
            ref words,
            ref presentation,
            ref cursor,
            fast: true,
            (ushort)KraidFootInstruction.Instruction_Kraid_XPositionMinus3_duplicate);
        RequireCursor(cursor, WalkingBackward);

        AddBackwardProgram(ref words, ref presentation, ref cursor);
        RequireCursor(cursor, AdjacentUnusedFastBackward);
    }

    /// <summary>Evaluates the native program grammar for one mechanics address,
    /// without storing its generated words or presentation offsets.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        var words = new MechanicsSelection(-1, address, false);
        var presentation = new PresentationSelection(-1);
        Generate(ref words, ref presentation);
        if (words.Found) return words.Selected.Value;
        throw new InvalidDataException($"Kraid foot mechanics pointer $A7:{address:X4} is not compiled.");
    }

    internal struct MechanicsSelection(int targetIndex, int targetAddress, bool includeHighByte)
    {
        private int count;
        internal bool Found;
        internal InstructionMechanicsWord Selected;
        internal void Add(InstructionMechanicsWord word)
        {
            if (count == targetIndex || word.Address == targetAddress ||
                includeHighByte && unchecked((ushort)(word.Address + 1)) == targetAddress)
            {
                Found = true;
                Selected = word;
            }
            count++;
        }
    }

    internal struct PresentationSelection(int targetIndex)
    {
        private int count;
        internal ushort Selected;
        internal void Add(ushort address)
        {
            if (count == targetIndex) Selected = address;
            count++;
        }
    }

    private static void AddForwardProgram(
        ref MechanicsSelection words,
        ref PresentationSelection presentation,
        ref ushort cursor,
        bool fast,
        ushort moveLeftInstruction)
    {
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_NOP_A7B633);
        for (int frame = 0; frame < 11; frame++)
            AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)4);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        for (int frame = 0; frame < 5; frame++)
            AddFrame(ref words, ref presentation, ref cursor, 1);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)4 : (ushort)0x10);

        AddVerticalAndHorizontal(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        AddVerticalAndHorizontal(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);

        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_NOP_A7B633);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        AddVerticalAndHorizontal(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_NOP_A7B633);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        AddVerticalAndHorizontal(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_NOP_A7B633);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)3);

        for (int frame = 0; frame < 3; frame++)
        {
            AddVerticalAndHorizontal(ref words, ref cursor,
                (ushort)KraidFootInstruction.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
                moveLeftInstruction);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        AddVerticalAndHorizontal(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
            moveLeftInstruction);
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_QueueSFX76_Lib2_Max6);
        AddFrame(ref words, ref presentation, ref cursor, 1);

        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_NOP_A7B633);
        AddInstruction(ref words, ref cursor, moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        for (int frame = 0; frame < 4; frame++)
        {
            AddInstruction(ref words, ref cursor, moveLeftInstruction);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        if (!fast)
        {
            AddInstruction(ref words, ref cursor, moveLeftInstruction);
            AddFrame(ref words, ref presentation, ref cursor, 1);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        else
        {
            AddFrame(ref words, ref presentation, ref cursor, 1);
            AddInstruction(ref words, ref cursor, moveLeftInstruction);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        AddInstruction(ref words, ref cursor, CommonEnemyInstructionCodes.Sleep);
    }

    private static void AddBackwardProgram(
        ref MechanicsSelection words,
        ref PresentationSelection presentation,
        ref ushort cursor)
    {
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_NOP_A7B633);
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_XPositionPlus3);
        AddFrame(ref words, ref presentation, ref cursor, 4);
        for (int frame = 0; frame < 6; frame++)
        {
            AddInstruction(ref words, ref cursor,
                (ushort)KraidFootInstruction.Instruction_Kraid_XPositionPlus3);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        for (int frame = 0; frame < 4; frame++)
        {
            AddVerticalAndHorizontal(ref words, ref cursor,
                (ushort)KraidFootInstruction.Instruction_Kraid_DecrementYPosition,
                (ushort)KraidFootInstruction.Instruction_Kraid_XPositionPlus3);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        for (int frame = 0; frame < 3; frame++)
        {
            AddVerticalAndHorizontal(ref words, ref cursor,
                (ushort)KraidFootInstruction.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
                (ushort)KraidFootInstruction.Instruction_Kraid_XPositionPlus3);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_IncrementYPosition_SetScreenShaking);
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_QueueSFX76_Lib2_Max6);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        AddInstruction(ref words, ref cursor,
            (ushort)KraidFootInstruction.Instruction_Kraid_NOP_A7B633);
        AddFrame(ref words, ref presentation, ref cursor, 0x14);
        for (int frame = 0; frame < 8; frame++)
            AddFrame(ref words, ref presentation, ref cursor, 4);
        for (int frame = 0; frame < 8; frame++)
            AddFrame(ref words, ref presentation, ref cursor, 1);
        AddInstruction(ref words, ref cursor, CommonEnemyInstructionCodes.Goto);
        AddInstruction(ref words, ref cursor, WalkingBackward);
    }

    private static void AddVerticalAndHorizontal(
        ref MechanicsSelection words,
        ref ushort cursor,
        ushort vertical,
        ushort horizontal)
    {
        AddInstruction(ref words, ref cursor, vertical);
        AddInstruction(ref words, ref cursor, horizontal);
    }

    private static void AddInstruction(
        ref MechanicsSelection words,
        ref ushort cursor,
        ushort instruction)
    {
        words.Add(new(cursor, instruction));
        cursor = unchecked((ushort)(cursor + 2));
    }

    private static void AddFrame(
        ref MechanicsSelection words,
        ref PresentationSelection presentation,
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
