using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KagoBugProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KagoBugProjectileInstructionProgramDefinitions))]
internal abstract class KagoBugProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => KagoBugProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => KagoBugProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => KagoBugProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => KagoBugProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Indicates whether compiled mechanics byte.</summary>
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
