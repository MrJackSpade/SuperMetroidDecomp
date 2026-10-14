using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Development-tool members of <see cref="MagdolliteLavaInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MagdolliteLavaInstructionProgramDefinitions))]
internal abstract class MagdolliteLavaInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The left and right lava poses and hit response contribute seven compiled control words.</summary>
    public static int MechanicsWordCount => MagdolliteLavaInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes the lava-pose timing, sleep, or shot-response word used by the projectile interpreter.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => MagdolliteLavaInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Each facing of Magdollite's thrown lava selects one installed spritemap.</summary>
    public static int PresentationWordCount => MagdolliteLavaInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates the left or right lava spritemap operand between compiled control words.</summary>
    public static ushort PresentationWordAddress(int index) => MagdolliteLavaInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Recognizes Magdollite lava control bytes in bank $86 without claiming either artwork selector.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MagdolliteLavaInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = MagdolliteLavaInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
