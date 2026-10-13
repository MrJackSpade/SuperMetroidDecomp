using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HopperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HopperInstructionProgramDefinitions))]
internal abstract class HopperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Hopper instruction words translated into compiled mechanics.</summary>
    public static int MechanicsWordCount => HopperInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Hopper mechanics word with its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => HopperInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts Hopper presentation words retained for cartridge artwork comparison.</summary>
    public static int PresentationWordCount => HopperInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Hopper presentation operand in its native program.</summary>
    public static ushort PresentationWordAddress(int index) => HopperInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each compiled Hopper word in bank $A3.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < HopperInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = HopperInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
