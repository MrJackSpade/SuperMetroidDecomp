using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Development-tool members of <see cref="EyeDoorProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EyeDoorProjectileInstructionProgramDefinitions))]
internal abstract class EyeDoorProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts compiled mechanics words in the eye-door projectile program.</summary>
    public static int MechanicsWordCount => EyeDoorProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns an eye-door projectile mechanics word and its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => EyeDoorProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts eye-door projectile presentation operands left as artwork data.</summary>
    public static int PresentationWordCount => EyeDoorProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed eye-door projectile presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => EyeDoorProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each compiled eye-door word in the enemy-projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EyeDoorProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EyeDoorProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1) return true;
        }
        return false;
    }
}
