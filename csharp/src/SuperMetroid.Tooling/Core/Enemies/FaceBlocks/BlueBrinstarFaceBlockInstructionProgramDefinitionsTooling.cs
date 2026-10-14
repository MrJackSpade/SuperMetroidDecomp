using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>Development-tool members of <see cref="BlueBrinstarFaceBlockInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BlueBrinstarFaceBlockInstructionProgramDefinitions))]
internal abstract class BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Covers the left and right Samus reactions and the block's initial program.</summary>
    public static int MechanicsWordCount => 10;
    /// <summary>Covers three visual operands per Samus reaction plus one initial-frame operand.</summary>
    public static int PresentationWordCount => 7;
    /// <summary>Reads four mechanic words from each reaction and two from initialization.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 8 ? (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft + 14 * (index / 4) + 4 * (index % 4))
            : (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial + 4 * (index - 8));
        return new(address, BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates visual operands interleaved with the two reaction instruction lists.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index < 6 ? (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft + 14 * (index / 3) + 4 * (index % 3) + 2)
            : (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial + 2);
    }
    /// <summary>Classifies mechanic byte positions in bank A8 while excluding interleaved visuals.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000) return false;
        int offset = unchecked((ushort)address) - BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft;
        if ((uint)offset < 28) return offset % 14 % 4 < 2;
        int initialOffset = unchecked((ushort)address) - BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial;
        return (uint)initialOffset < 6 && initialOffset % 4 < 2;
    }
}
