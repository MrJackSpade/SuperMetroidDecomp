using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ShaktoolProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShaktoolProjectileInstructionProgramDefinitions))]
internal abstract class ShaktoolProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 18;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = ShaktoolProjectileInstructionProgramDefinitions.Front; address < ShaktoolProjectileInstructionProgramDefinitions.End; address += 2)
        {
            int value = ShaktoolProjectileInstructionProgramDefinitions.ProgramWord((ushort)address);
            if (value != ShaktoolProjectileInstructionProgramDefinitions.PresentationOperand && index-- == 0) return new((ushort)address, (ushort)value);
        }
        throw new InvalidOperationException("Shaktool projectile mechanics-word index is inconsistent.");
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort word = (ushort)(address & 0xfffe);
        return word >= ShaktoolProjectileInstructionProgramDefinitions.Front && word < ShaktoolProjectileInstructionProgramDefinitions.End && ShaktoolProjectileInstructionProgramDefinitions.ProgramWord(word) != ShaktoolProjectileInstructionProgramDefinitions.PresentationOperand;
    }
}
