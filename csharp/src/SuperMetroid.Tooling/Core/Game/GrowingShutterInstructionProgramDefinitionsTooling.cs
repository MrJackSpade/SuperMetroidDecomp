using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GrowingShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GrowingShutterInstructionProgramDefinitions))]
internal abstract class GrowingShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 8;
    public static int PresentationWordCount => 4;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(GrowingShutterInstructionProgramDefinitions.TenPixels + 6 * (index / 2) + 4 * (index % 2));
        return new(address, GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(GrowingShutterInstructionProgramDefinitions.TenPixels + 6 * index + 2);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - GrowingShutterInstructionProgramDefinitions.TenPixels;
        return (uint)offset < 24 && (offset % 6 < 2 || offset % 6 >= 4);
    }
}
