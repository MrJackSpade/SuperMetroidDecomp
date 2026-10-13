using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidRockProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidRockProjectileInstructionProgramDefinitions))]
internal abstract class KraidRockProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts compiled mechanics words in Kraid's rock projectile program.</summary>
    public static int MechanicsWordCount => KraidRockProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a rock-projectile mechanics word with its cartridge address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidRockProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Kraid rock presentation operands retained for artwork.</summary>
    public static int PresentationWordCount => KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Kraid rock presentation word.</summary>
    public static ushort PresentationWordAddress(int index) => KraidRockProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of compiled Kraid rock words in the enemy-projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidRockProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidRockProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
