using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Development-tool members of <see cref="WreckedShipGhostInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WreckedShipGhostInstructionProgramDefinitions))]
internal abstract class WreckedShipGhostInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Wrecked Ship ghost instruction words translated into mechanics.</summary>
    public static int MechanicsWordCount => WreckedShipGhostInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled ghost mechanics word with its cartridge address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => WreckedShipGhostInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts ghost presentation operands retained outside compiled behavior.</summary>
    public static int PresentationWordCount => WreckedShipGhostInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed Wrecked Ship ghost presentation operand.</summary>
    public static ushort PresentationWordAddress(int index) => WreckedShipGhostInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of compiled ghost words in bank $A8.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < WreckedShipGhostInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = WreckedShipGhostInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
