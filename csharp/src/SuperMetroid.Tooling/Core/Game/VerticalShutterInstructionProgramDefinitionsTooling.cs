using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="VerticalShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(VerticalShutterInstructionProgramDefinitions))]
internal abstract class VerticalShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 8;
    public static int PresentationWordCount => 5;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int frame = index - 2;
        ushort address = (ushort)(index < 2 ? VerticalShutterInstructionProgramDefinitions.Plain + 4 * index
            : VerticalShutterInstructionProgramDefinitions.KamerPlatform + (frame < 4 ? 4 * frame : 16 + 2 * (frame - 4)));
        return new(address, VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index == 0 ? VerticalShutterInstructionProgramDefinitions.Plain + 2 : VerticalShutterInstructionProgramDefinitions.KamerPlatform + 2 + 4 * (index - 1));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int bankAddress = unchecked((ushort)address);
        int plainOffset = bankAddress - VerticalShutterInstructionProgramDefinitions.Plain;
        if ((uint)plainOffset < 6) return plainOffset is < 2 or >= 4;
        int kamerOffset = bankAddress - VerticalShutterInstructionProgramDefinitions.KamerPlatform;
        return (uint)kamerOffset < 20 && (kamerOffset >= 16 || kamerOffset % 4 < 2);
    }
}
