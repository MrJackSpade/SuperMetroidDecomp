using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainHandBeamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainHandBeamInstructionProgramDefinitions))]
internal abstract class MotherBrainHandBeamInstructionProgramDefinitionsTooling : IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Defines words per stage.</summary>
    internal const int WordsPerStage = MotherBrainHandBeamInstructionProgramDefinitions.FramesPerStage + 1;
    /// <summary>Gets native word count.</summary>
    internal static int NativeWordCount => MotherBrainHandBeamInstructionProgramDefinitions.StageCount * WordsPerStage + 1;
    /// <summary>native word.</summary>
    internal static InstructionMechanicsWord NativeWord(int index)
    {
        if ((uint)index >= NativeWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == NativeWordCount - 1) return new(MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete, (ushort)EnemyProjectileInstruction.Delete);
        int record = index % WordsPerStage;
        ushort address = (ushort)(MotherBrainHandBeamInstructionProgramDefinitions.StageStart(index / WordsPerStage) +
            (record == 1 ? MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes : MotherBrainHandBeamInstructionProgramDefinitions.FrameOffset(record == 0 ? 0 : record - 1)));
        return new(address, MotherBrainHandBeamInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
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
