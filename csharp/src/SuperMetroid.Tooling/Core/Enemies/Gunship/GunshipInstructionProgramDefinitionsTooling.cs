using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Development-tool members of <see cref="GunshipInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GunshipInstructionProgramDefinitions))]
internal abstract class GunshipInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The hull and entrance-pad sequences contribute 28 immutable timing and branch words.</summary>
    public static int MechanicsWordCount => GunshipInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes a compiled gunship control word from the opening, closing, or hull programs.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => GunshipInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>The gunship sequences select 22 installed OAM frames outside their compiled mechanics.</summary>
    public static int PresentationWordCount => GunshipInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an interleaved gunship visual selector for artwork extraction checks.</summary>
    public static ushort PresentationWordAddress(int index) => GunshipInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled gunship control word in bank $A2; visual selectors remain excluded.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GunshipInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = GunshipInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
