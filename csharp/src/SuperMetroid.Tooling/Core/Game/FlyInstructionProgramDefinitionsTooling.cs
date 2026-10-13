using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FlyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FlyInstructionProgramDefinitions))]
internal abstract class FlyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts Fly flight-program words compiled as mechanics.</summary>
    public static int MechanicsWordCount => FlyInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a compiled Fly mechanics word and its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => FlyInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts visual operands, one for each Fly flight frame.</summary>
    public static int PresentationWordCount => FlyInstructionProgramDefinitions.FrameCount;
    /// <summary>Locates the visual word after an indexed Fly flight-frame control word.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(FlyInstructionProgramDefinitions.Flight + 2 + 4 * index);
    }
    /// <summary>Checks both bytes of compiled Fly mechanics words in bank $A2.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < FlyInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = FlyInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
