using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Development-tool members of <see cref="EscapeDachoraInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EscapeDachoraInstructionProgramDefinitions))]
internal abstract class EscapeDachoraInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Controls and frame durations for low tide, high tide, and escape departure.</summary>
    public static int MechanicsWordCount => EscapeDachoraInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads one compiled word from the escape Dachora's pacing or departure lists.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => EscapeDachoraInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Drawing operands in the tide pacing and accelerating departure sequences.</summary>
    public static int PresentationWordCount => EscapeDachoraInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a Dachora frame operand in the extracted escape sequence.</summary>
    public static ushort PresentationWordAddress(int index) => EscapeDachoraInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled sequence word in bank $B3.</summary>
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
