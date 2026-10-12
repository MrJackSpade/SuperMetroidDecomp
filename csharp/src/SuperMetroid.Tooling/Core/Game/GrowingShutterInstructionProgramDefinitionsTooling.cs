using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GrowingShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GrowingShutterInstructionProgramDefinitions))]
internal abstract class GrowingShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 8;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => 4;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(GrowingShutterInstructionProgramDefinitions.TenPixels + 6 * (index / 2) + 4 * (index % 2));
        return new(address, GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(GrowingShutterInstructionProgramDefinitions.TenPixels + 6 * index + 2);
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - GrowingShutterInstructionProgramDefinitions.TenPixels;
        return (uint)offset < 24 && (offset % 6 < 2 || offset % 6 >= 4);
    }
}
