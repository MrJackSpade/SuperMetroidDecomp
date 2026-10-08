using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ShaktoolProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShaktoolProjectileInstructionProgramDefinitions))]
internal abstract class ShaktoolProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 18;
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
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort word = (ushort)(address & 0xfffe);
        return word >= ShaktoolProjectileInstructionProgramDefinitions.Front && word < ShaktoolProjectileInstructionProgramDefinitions.End && ShaktoolProjectileInstructionProgramDefinitions.ProgramWord(word) != ShaktoolProjectileInstructionProgramDefinitions.PresentationOperand;
    }
}
