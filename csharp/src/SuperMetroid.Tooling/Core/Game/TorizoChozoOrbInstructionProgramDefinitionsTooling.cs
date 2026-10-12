using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoChozoOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoChozoOrbInstructionProgramDefinitions))]
internal abstract class TorizoChozoOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts fixed motion and impact controls for Bomb and Golden Torizo orb programs.</summary>
    public static int MechanicsWordCount => TorizoChozoOrbInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads one compiled orb-program word at its native projectile-bank address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => TorizoChozoOrbInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Accounts for eighteen orb spritemap operands installed as presentation art.</summary>
    public static int PresentationWordCount => TorizoChozoOrbInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an orb-frame operand in the $86:AB15-AB89 program span.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoChozoOrbInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Tests compiled orb-control bytes in bank $86 without claiming sprite selectors.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TorizoChozoOrbInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = TorizoChozoOrbInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
