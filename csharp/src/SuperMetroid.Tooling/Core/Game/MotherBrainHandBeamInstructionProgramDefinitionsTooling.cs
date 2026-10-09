using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainHandBeamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainHandBeamInstructionProgramDefinitions))]
internal abstract class MotherBrainHandBeamInstructionProgramDefinitionsTooling : IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of live spritemap operands across the three hand-beam animation stages.</summary>
    public static int PresentationWordCount => MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of a frame's spritemap operand.</summary>
    /// <param name="index">Zero-based frame index across all stages.</param>
    /// <returns>Bank-local address of the selected presentation word.</returns>
    public static ushort PresentationWordAddress(int index) => MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Mechanics words in one stage: a duration for each frame plus its external-call command.</summary>
    internal const int WordsPerStage = MotherBrainHandBeamInstructionProgramDefinitions.FramesPerStage + 1;

    /// <summary>Total decoded mechanics entries across all stages and the final delete instruction.</summary>
    internal static int NativeWordCount => MotherBrainHandBeamInstructionProgramDefinitions.StageCount * WordsPerStage + 1;

    /// <summary>Decodes one timer, callback, or terminal-delete mechanics entry from the compiled sequence.</summary>
    /// <param name="index">Zero-based entry index across all stages and the terminal instruction.</param>
    /// <returns>The native instruction address and its translated operand value.</returns>
    internal static InstructionMechanicsWord NativeWord(int index)
    {
        if ((uint)index >= NativeWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == NativeWordCount - 1) return new(MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        int record = index % WordsPerStage;
        ushort address = (ushort)(MotherBrainHandBeamInstructionProgramDefinitions.StageStart(index / WordsPerStage) +
            (record == 1 ? MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes : MotherBrainHandBeamInstructionProgramDefinitions.FrameOffset(record == 0 ? 0 : record - 1)));
        return new(address, MotherBrainHandBeamInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Reports whether a cartridge byte address belongs to a translated mechanics word.</summary>
    /// <param name="address">Full cartridge address to check.</param>
    /// <returns><see langword="true"/> for bytes occupied by stage timers, callback commands, or the final delete.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int bankAddress = (ushort)address;
        if (bankAddress == MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete || bankAddress == MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete + 1) return true;
        if (bankAddress < MotherBrainHandBeamInstructionProgramDefinitions.Initial || bankAddress >= MotherBrainHandBeamInstructionProgramDefinitions.TerminalDelete) return false;
        int offset = (bankAddress - MotherBrainHandBeamInstructionProgramDefinitions.Initial) % MotherBrainHandBeamInstructionProgramDefinitions.StageBytes;
        if (offset < sizeof(ushort)) return true;
        if (offset < MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes) return false;
        if (offset < MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes + MotherBrainHandBeamInstructionProgramDefinitions.CallbackBytes) return true;
        return (offset - MotherBrainHandBeamInstructionProgramDefinitions.CallbackBytes) % MotherBrainHandBeamInstructionProgramDefinitions.FrameBytes < sizeof(ushort);
    }
}
