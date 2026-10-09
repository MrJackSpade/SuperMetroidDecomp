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

    /// <summary>Emits the native initial, neutral, forward, lunge, and backward foot programs into mechanics and presentation selections.</summary>
    /// <param name="words">Collector receiving instruction addresses and their timing or callback words.</param>
    /// <param name="presentation">Collector receiving the presentation address associated with each emitted frame.</param>
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
            KraidInstructionCodes.Instruction_Kraid_XPositionMinus3);
        RequireCursor(cursor, LungeForward);

        AddForwardProgram(
            ref words,
            ref presentation,
            ref cursor,
            fast: true,
            KraidInstructionCodes.Instruction_Kraid_XPositionMinus3_duplicate);
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

    /// <summary>Selects one generated mechanics word by stream index or address, optionally matching either byte of its address.</summary>
    /// <param name="targetIndex">Zero-based emitted-word index to capture, or a negative value to disable index matching.</param>
    /// <param name="targetAddress">Address of the word to capture, or a value outside the stream to disable address matching.</param>
    /// <param name="includeHighByte">When true, also matches the second byte address of each emitted word.</param>
    internal struct MechanicsSelection(int targetIndex, int targetAddress, bool includeHighByte)
    {
        /// <summary>Number of mechanics words passed to this collector so far.</summary>
        private int count;
        /// <summary>Whether a word matched the configured index or address selector.</summary>
        internal bool Found;
        /// <summary>The last mechanics word that matched the configured selector.</summary>
        internal InstructionMechanicsWord Selected;

        /// <summary>Records a generated word and captures it when its index or address matches.</summary>
        /// <param name="word">Mechanics address and value emitted by the instruction program.</param>
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

    /// <summary>Captures one generated frame's presentation address by its zero-based frame index.</summary>
    /// <param name="targetIndex">Frame index whose presentation address this collector retains.</param>
    internal struct PresentationSelection(int targetIndex)
    {
        /// <summary>Number of frame presentation addresses passed to this collector so far.</summary>
        private int count;
        /// <summary>Presentation address recorded for the configured frame index.</summary>
        internal ushort Selected;

        /// <summary>Records a frame presentation address and retains it if the current index is selected.</summary>
        /// <param name="address">Address of the presentation data associated with a generated frame.</param>
        internal void Add(ushort address)
        {
            if (count == targetIndex) Selected = address;
            count++;
        }
    }

    /// <summary>Emits a forward-walking or fast-lunge program, including vertical movement, impact sound, and its sleep terminator.</summary>
    /// <param name="words">Collector receiving emitted instruction words.</param>
    /// <param name="presentation">Collector receiving presentation addresses for emitted frames.</param>
    /// <param name="cursor">Current program address, advanced as words and frames are emitted.</param>
    /// <param name="fast">Selects the shorter lunge timing and its corresponding frame sequence when true.</param>
    /// <param name="moveLeftInstruction">Horizontal movement callback used during the forward sequence.</param>
    private static void AddForwardProgram(
        ref MechanicsSelection words,
        ref PresentationSelection presentation,
        ref ushort cursor,
        bool fast,
        ushort moveLeftInstruction)
    {
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        for (int frame = 0; frame < 11; frame++)
            AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)4);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        for (int frame = 0; frame < 5; frame++)
            AddFrame(ref words, ref presentation, ref cursor, 1);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)4 : (ushort)0x10);

        AddVerticalAndHorizontal(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        AddVerticalAndHorizontal(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);

        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        AddVerticalAndHorizontal(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)3);
        AddVerticalAndHorizontal(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
            moveLeftInstruction);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddFrame(ref words, ref presentation, ref cursor, fast ? (ushort)1 : (ushort)3);

        for (int frame = 0; frame < 3; frame++)
        {
            AddVerticalAndHorizontal(ref words, ref cursor,
                KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
                moveLeftInstruction);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        AddVerticalAndHorizontal(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
            moveLeftInstruction);
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_QueueSFX76_Lib2_Max6);
        AddFrame(ref words, ref presentation, ref cursor, 1);

        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
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

    /// <summary>Emits the backward walk and return loop, including impact motion and the jump to its loop entry.</summary>
    /// <param name="words">Collector receiving emitted instruction words.</param>
    /// <param name="presentation">Collector receiving presentation addresses for emitted frames.</param>
    /// <param name="cursor">Current program address, advanced as words and frames are emitted.</param>
    private static void AddBackwardProgram(
        ref MechanicsSelection words,
        ref PresentationSelection presentation,
        ref ushort cursor)
    {
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_XPositionPlus3);
        AddFrame(ref words, ref presentation, ref cursor, 4);
        for (int frame = 0; frame < 6; frame++)
        {
            AddInstruction(ref words, ref cursor,
                KraidInstructionCodes.Instruction_Kraid_XPositionPlus3);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        for (int frame = 0; frame < 4; frame++)
        {
            AddVerticalAndHorizontal(ref words, ref cursor,
                KraidInstructionCodes.Instruction_Kraid_DecrementYPosition,
                KraidInstructionCodes.Instruction_Kraid_XPositionPlus3);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        for (int frame = 0; frame < 3; frame++)
        {
            AddVerticalAndHorizontal(ref words, ref cursor,
                KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking,
                KraidInstructionCodes.Instruction_Kraid_XPositionPlus3);
            AddFrame(ref words, ref presentation, ref cursor, 1);
        }
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_IncrementYPosition_SetScreenShaking);
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_QueueSFX76_Lib2_Max6);
        AddFrame(ref words, ref presentation, ref cursor, 1);
        AddInstruction(ref words, ref cursor,
            KraidInstructionCodes.Instruction_Kraid_NOP_A7B633);
        AddFrame(ref words, ref presentation, ref cursor, 0x14);
        for (int frame = 0; frame < 8; frame++)
            AddFrame(ref words, ref presentation, ref cursor, 4);
        for (int frame = 0; frame < 8; frame++)
            AddFrame(ref words, ref presentation, ref cursor, 1);
        AddInstruction(ref words, ref cursor, CommonEnemyInstructionCodes.Goto);
        AddInstruction(ref words, ref cursor, WalkingBackward);
    }

    /// <summary>Emits two consecutive callback words for vertical and horizontal movement within one animation interval.</summary>
    /// <param name="words">Collector receiving the callbacks.</param>
    /// <param name="cursor">Current program address, advanced after each emitted word.</param>
    /// <param name="vertical">Vertical movement callback to emit first.</param>
    /// <param name="horizontal">Horizontal movement callback to emit second.</param>
    private static void AddVerticalAndHorizontal(
        ref MechanicsSelection words,
        ref ushort cursor,
        ushort vertical,
        ushort horizontal)
    {
        AddInstruction(ref words, ref cursor, vertical);
        AddInstruction(ref words, ref cursor, horizontal);
    }

    /// <summary>Appends one instruction word at the current native program address and advances the cursor by a word.</summary>
    /// <param name="words">Collector receiving the instruction's address and value.</param>
    /// <param name="cursor">Address at which the instruction is emitted, advanced by two bytes afterward.</param>
    /// <param name="instruction">Instruction or callback word to store.</param>
    private static void AddInstruction(
        ref MechanicsSelection words,
        ref ushort cursor,
        ushort instruction)
    {
        words.Add(new(cursor, instruction));
        cursor = unchecked((ushort)(cursor + 2));
    }

    /// <summary>Appends a frame duration, records its following presentation address, and advances past both words.</summary>
    /// <param name="words">Collector receiving the duration word and its program address.</param>
    /// <param name="presentation">Collector receiving the presentation address following this duration.</param>
    /// <param name="cursor">Address of the duration word, advanced by the duration and presentation words.</param>
    /// <param name="duration">Number of updates for which the associated frame is held.</param>
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

    /// <summary>Ensures an emitted program ends at the next known native instruction-list address.</summary>
    /// <param name="actual">Cursor reached after emitting the current program.</param>
    /// <param name="expected">Address required by the following compiled program.</param>
    /// <exception cref="InvalidDataException">The generated word count does not end at the expected native address.</exception>
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
