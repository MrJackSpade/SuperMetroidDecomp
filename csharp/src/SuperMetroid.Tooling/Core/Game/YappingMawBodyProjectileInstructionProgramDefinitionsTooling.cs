using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="YappingMawBodyProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(YappingMawBodyProjectileInstructionProgramDefinitions))]
internal abstract class YappingMawBodyProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Compiled timing and control words in the Yapping Maw body projectile lists.</summary>
    public static int MechanicsWordCount => YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads a word from the extracted body projectile mechanics catalog.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands in the Yapping Maw body projectile lists.</summary>
    public static int PresentationWordCount => YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a body projectile drawing operand by catalog index.</summary>
    public static ushort PresentationWordAddress(int index) => YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a body projectile mechanics word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = YappingMawBodyProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
