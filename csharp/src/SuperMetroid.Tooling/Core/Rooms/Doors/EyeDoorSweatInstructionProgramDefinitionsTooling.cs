using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Development-tool members of <see cref="EyeDoorSweatInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EyeDoorSweatInstructionProgramDefinitions))]
internal abstract class EyeDoorSweatInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Compiled timing and control words in the eye-door sweat initial and impact lists.</summary>
    public static int MechanicsWordCount => EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads a compiled word from the initial or impact animation.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>The four drawing operands used by the sweat animations.</summary>
    public static int PresentationWordCount => EyeDoorSweatInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a drawing operand in the initial or impact list.</summary>
    public static ushort PresentationWordAddress(int index) => EyeDoorSweatInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a cataloged sweat mechanics word in the projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EyeDoorSweatInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EyeDoorSweatInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
