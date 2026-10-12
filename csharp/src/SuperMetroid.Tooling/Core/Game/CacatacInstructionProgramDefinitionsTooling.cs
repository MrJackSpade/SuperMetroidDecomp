using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CacatacInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CacatacInstructionProgramDefinitions))]
internal abstract class CacatacInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 56;
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => 24;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int start = CacatacInstructionProgramDefinitions.UpsideUpIdle + index / 28 * 80;
        int step = index % 28;
        int offset = step switch
        {
            0 => 0,
            < 9 => 2 + (step - 1) * 4,
            < 11 => 34 + (step - 9) * 2,
            < 15 => 38 + (step - 11) * 4,
            _ => 54 + (step - 15) * 2,
        };
        ushort address = (ushort)(start + offset);
        return new(address, CacatacInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int pose = index % 12;
        return (ushort)(CacatacInstructionProgramDefinitions.UpsideUpIdle + index / 12 * 80 + (pose < 8 ? 4 + pose * 4 : 40 + (pose - 8) * 4));
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == 0xa20000 && CacatacInstructionProgramDefinitions.TryRead((ushort)(address & ~1), out _);
}
