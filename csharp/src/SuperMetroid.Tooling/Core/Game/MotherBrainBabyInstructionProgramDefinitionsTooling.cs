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
    static int IDeclaredProgramBank.Bank => Bank;
    public static int MechanicsWordCount => 2 * LoopWords + 2;
    public static int PresentationWordCount => 2 * MotherBrainBabyInstructionProgramDefinitions.LoopFrames + 1;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < LoopWords ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.Initial + index * MotherBrainBabyInstructionProgramDefinitions.FrameBytes) :
            index < 2 * LoopWords ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.DrainingMotherBrain + (index - LoopWords) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes) :
            (ushort)(MotherBrainBabyInstructionProgramDefinitions.TakingFatalBlow + (index - 2 * LoopWords) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes);
        return new(address, MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index < MotherBrainBabyInstructionProgramDefinitions.LoopFrames ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.Initial + index * MotherBrainBabyInstructionProgramDefinitions.FrameBytes + sizeof(ushort)) :
            index < 2 * MotherBrainBabyInstructionProgramDefinitions.LoopFrames ? (ushort)(MotherBrainBabyInstructionProgramDefinitions.DrainingMotherBrain + (index - MotherBrainBabyInstructionProgramDefinitions.LoopFrames) * MotherBrainBabyInstructionProgramDefinitions.FrameBytes + sizeof(ushort)) :
            (ushort)(MotherBrainBabyInstructionProgramDefinitions.TakingFatalBlow + sizeof(ushort));
    }
}
