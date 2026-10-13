using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SparkInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SparkInstructionProgramDefinitions))]
internal abstract class SparkInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The Wrecked Ship spark's activation, active, fade, and emitter lists expose their immutable control words.</summary>
    public static int MechanicsWordCount => SparkInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes a spark callback, duration, terminal instruction, or branch target for native-data comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => SparkInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts visual selections across the four spark animations while excluding their timing.</summary>
    public static int PresentationWordCount => SparkInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a spark spritemap operand interleaved with its compiled animation cadence.</summary>
    public static ushort PresentationWordAddress(int index) => SparkInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Recognizes bank-$A8 spark mechanics bytes, leaving visual operands to the artwork catalog.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SparkInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SparkInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
