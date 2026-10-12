using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidRockProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidRockProjectileInstructionProgramDefinitions))]
internal abstract class KraidRockProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => KraidRockProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidRockProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => KraidRockProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
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
