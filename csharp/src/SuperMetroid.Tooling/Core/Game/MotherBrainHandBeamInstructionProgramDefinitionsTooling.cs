using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainHandBeamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainHandBeamInstructionProgramDefinitions))]
internal abstract class MotherBrainHandBeamInstructionProgramDefinitionsTooling : IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordAddress(index);
    internal const int WordsPerStage = MotherBrainHandBeamInstructionProgramDefinitions.FramesPerStage + 1;
    internal static int NativeWordCount => MotherBrainHandBeamInstructionProgramDefinitions.StageCount * WordsPerStage + 1;
    internal static InstructionMechanicsWord NativeWord(int index)
    {
        if ((uint)index >= NativeWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == NativeWordCount - 1) return new(MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        int record = index % WordsPerStage;
        ushort address = (ushort)(MotherBrainHandBeamInstructionProgramDefinitions.StageStart(index / WordsPerStage) +
            (record == 1 ? MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes : MotherBrainHandBeamInstructionProgramDefinitions.FrameOffset(record == 0 ? 0 : record - 1)));
        return new(address, MotherBrainHandBeamInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int bankAddress = (ushort)address;
        if (bankAddress is MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete or (MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete + 1)) return true;
        if (bankAddress is < MotherBrainHandBeamInstructionProgramDefinitions.Initial or >= MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete) return false;
        int offset = (bankAddress - MotherBrainHandBeamInstructionProgramDefinitions.Initial) % MotherBrainHandBeamInstructionProgramDefinitions.StageBytes;
        if (offset < sizeof(ushort)) return true;
        if (offset < MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes) return false;
        if (offset < MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes + MotherBrainHandBeamInstructionProgramDefinitions.CallbackBytes) return true;
        return (offset - MotherBrainHandBeamInstructionProgramDefinitions.CallbackBytes) % MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes < sizeof(ushort);
    }
}
