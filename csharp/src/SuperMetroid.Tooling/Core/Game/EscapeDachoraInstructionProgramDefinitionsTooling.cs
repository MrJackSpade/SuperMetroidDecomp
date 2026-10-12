using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EscapeDachoraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EscapeDachoraInstructionProgramDefinitions))]
internal abstract class EscapeDachoraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => EscapeDachoraInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
