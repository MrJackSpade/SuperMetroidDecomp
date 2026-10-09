using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainBabyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainBabyInstructionProgramDefinitions))]
internal abstract class MotherBrainBabyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>Each loop holds its four timed frames and the closing goto word.</summary>
    internal const int LoopWords = MotherBrainBabyInstructionProgramDefinitions.LoopFrames + 1;
    /// <summary>Native Mother Brain cutscene-baby instruction and OAM bank $A9.</summary>
    internal const byte Bank = 0xa9;

    /// <summary>Supplies the bank containing the Baby Metroid's cutscene instruction lists.</summary>
    static int IDeclaredProgramBank.Bank => Bank;

    /// <summary>Number of frame-loop and fatal-blow mechanics words across the three programs.</summary>
    public static int MechanicsWordCount => 2 * LoopWords + 2;

    /// <summary>Number of spritemap operands used by the initial, draining, and fatal-blow programs.</summary>
    public static int PresentationWordCount => 2 * MotherBrainBabyInstructionProgramDefinitions.LoopFrames + 1;

    /// <summary>Resolves an ordinal to a compiled duration or instruction word and its native address.</summary>
    /// <param name="index">Zero-based index among the programs' mechanics words.</param>
    /// <returns>The bank-local address and value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < LoopWords ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.Initial + index * MotherBrainBabyInstructionProgramDefinitions.FrameBytes) :
            index < 2 * LoopWords ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.DrainingMotherBrain + (index - LoopWords) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes) :
            (ushort)(MotherBrainBabyInstructionProgramDefinitions.TakingFatalBlow + (index - 2 * LoopWords) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes);
        return new(address, MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Returns the native address of an artwork operand in the initial, draining, or fatal-blow list.</summary>
    /// <param name="index">Zero-based index among the presentation operands.</param>
    /// <returns>The bank-local address containing the selected spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index < MotherBrainBabyInstructionProgramDefinitions.LoopFrames ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.Initial + index * MotherBrainBabyInstructionProgramDefinitions.FrameBytes + sizeof(ushort)) :
            index < 2 * MotherBrainBabyInstructionProgramDefinitions.LoopFrames ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.DrainingMotherBrain + (index - MotherBrainBabyInstructionProgramDefinitions.LoopFrames) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes + sizeof(ushort)) :
            (ushort)(MotherBrainBabyInstructionProgramDefinitions.TakingFatalBlow + sizeof(ushort));
    }
}
