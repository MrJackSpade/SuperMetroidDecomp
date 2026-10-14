using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Development-tool members of <see cref="DownwardGateProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DownwardGateProjectileInstructionProgramDefinitions))]
internal abstract class DownwardGateProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the gate projectile's compiled mechanic words for source comparison.</summary>
    public static int MechanicsWordCount => DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Supplies an addressed mechanic word from the gate projectile program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Bounds presentation operands separately from the gate projectile mechanics.</summary>
    public static int PresentationWordCount => DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an operand used only for projectile presentation.</summary>
    public static ushort PresentationWordAddress(int index) => DownwardGateProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a mechanic word within the enemy projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
