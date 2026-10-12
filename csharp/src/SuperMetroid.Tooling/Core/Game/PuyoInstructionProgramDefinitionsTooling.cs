using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PuyoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PuyoInstructionProgramDefinitions))]
internal abstract class PuyoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 28;
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => 17;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int pointer;
        if (index < 18)
        {
            int local = index % 6;
            pointer = PuyoInstructionProgramDefinitions.GroundedFast + index / 6 * 20 + (local < 5 ? local * 4 : 18);
        }
        else pointer = PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4 + (index - 18) / 2 * 6 + (index % 2) * 4;
        return new((ushort)pointer, PuyoInstructionProgramDefinitions.ReadMechanicsWord((ushort)pointer));
    }
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 12 ? PuyoInstructionProgramDefinitions.GroundedFast + index / 4 * 20 + index % 4 * 4 + 2 :
            PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4 + (index - 12) * 6 + 2);
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000) return false;
        int offset = (ushort)address - PuyoInstructionProgramDefinitions.GroundedFast;
        if ((uint)offset < 60) return offset % 20 >= 16 || offset % 20 % 4 < 2;
        offset = (ushort)address - PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4;
        return (uint)offset < 30 && offset % 6 is not (2 or 3);
    }
}
