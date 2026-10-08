using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BlueBrinstarFaceBlockInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BlueBrinstarFaceBlockInstructionProgramDefinitions))]
internal abstract class BlueBrinstarFaceBlockInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 10;
    public static int PresentationWordCount => 7;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 8 ? (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft + 14 * (index / 4) + 4 * (index % 4))
            : (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial + 4 * (index - 8));
        return new(address, BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return index < 6 ? (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft + 14 * (index / 3) + 4 * (index % 3) + 2)
            : (ushort)(BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        int offset = unchecked((ushort)address) - BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft;
        if ((uint)offset < 28) return offset % 14 % 4 < 2;
        int initialOffset = unchecked((ushort)address) - BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial;
        return (uint)initialOffset < 6 && initialOffset % 4 < 2;
    }
}
