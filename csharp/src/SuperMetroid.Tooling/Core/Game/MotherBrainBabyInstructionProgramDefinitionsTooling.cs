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
    /// <summary>Declares bank $A9 for the baby's initial, draining, and fatal-blow lists.</summary>
    static int IDeclaredProgramBank.Bank => Bank;
    /// <summary>Counts two four-frame loops with closing branches and the two fatal-blow control words.</summary>
    public static int MechanicsWordCount => 2 * LoopWords + 2;
    /// <summary>Counts the initial and draining loop frames plus the fatal-blow frame selector.</summary>
    public static int PresentationWordCount => 2 * MotherBrainBabyInstructionProgramDefinitions.LoopFrames + 1;
    /// <summary>Finds a frame duration or loop control in the baby's two cycles or fatal-blow program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < LoopWords ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.Initial + index * MotherBrainBabyInstructionProgramDefinitions.FrameBytes) :
            index < 2 * LoopWords ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.DrainingMotherBrain + (index - LoopWords) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes) :
            (ushort)(MotherBrainBabyInstructionProgramDefinitions.TakingFatalBlow + (index - 2 * LoopWords) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes);
        return new(address, MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates a baby spritemap operand immediately after its corresponding duration word.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index < MotherBrainBabyInstructionProgramDefinitions.LoopFrames ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.Initial + index * MotherBrainBabyInstructionProgramDefinitions.FrameBytes + sizeof(ushort)) :
            index < 2 * MotherBrainBabyInstructionProgramDefinitions.LoopFrames ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.DrainingMotherBrain + (index - MotherBrainBabyInstructionProgramDefinitions.LoopFrames) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes + sizeof(ushort)) :
            (ushort)(MotherBrainBabyInstructionProgramDefinitions.TakingFatalBlow + sizeof(ushort));
    }
}
