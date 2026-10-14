using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Development-tool members of <see cref="ShaktoolProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShaktoolProjectileInstructionProgramDefinitions))]
internal abstract class ShaktoolProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The unused Shaktool attack-circle programs contain eight retained visual selections.</summary>
    public static int PresentationWordCount => ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a Shaktool circle spritemap operand within the front, middle, or back program.</summary>
    public static ushort PresentationWordAddress(int index) => ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>The three attack-circle programs contribute 18 control words after visual operands are excluded.</summary>
    public static int MechanicsWordCount => 18;
    /// <summary>Walks the bank-$86 circle programs in address order, skipping spritemap operands to find the indexed control word.</summary>
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
    /// <summary>Recognizes control-word bytes within the Shaktool circle range while excluding presentation-word addresses.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort word = (ushort)(address & 0xfffe);
        return word >= ShaktoolProjectileInstructionProgramDefinitions.Front && word < ShaktoolProjectileInstructionProgramDefinitions.End && ShaktoolProjectileInstructionProgramDefinitions.ProgramWord(word) != ShaktoolProjectileInstructionProgramDefinitions.PresentationOperand;
    }
}
