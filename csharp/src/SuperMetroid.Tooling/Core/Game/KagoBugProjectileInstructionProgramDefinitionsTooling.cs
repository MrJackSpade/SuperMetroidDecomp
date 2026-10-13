using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KagoBugProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KagoBugProjectileInstructionProgramDefinitions))]
internal abstract class KagoBugProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the Kago bug projectile's compiled mechanic words for source comparison.</summary>
    public static int MechanicsWordCount => KagoBugProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Supplies an addressed mechanic word from the Kago bug projectile program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KagoBugProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds visual operands separately from the Kago bug projectile mechanics.</summary>
    public static int PresentationWordCount => KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand in the compiled projectile program.</summary>
    public static ushort PresentationWordAddress(int index) => KagoBugProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each mechanic word within the enemy projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KagoBugProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KagoBugProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
