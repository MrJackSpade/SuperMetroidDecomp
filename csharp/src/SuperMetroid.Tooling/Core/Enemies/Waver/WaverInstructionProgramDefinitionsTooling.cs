using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Development-tool members of <see cref="WaverInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WaverInstructionProgramDefinitions))]
internal abstract class WaverInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Compiled timing and control words in the steady and spinning Waver lists.</summary>
    public static int MechanicsWordCount => WaverInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads a mechanics word from a Waver facing or spin program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => WaverInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Two steady poses and four spinning poses per facing direction.</summary>
    public static int PresentationWordCount => 10;
    /// <summary>Locates the steady or spinning pose operand within its directional list.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return index < 2 ? (ushort)(WaverInstructionProgramDefinitions.SteadyFacingLeft + 6 * index + 2)
            : (ushort)(WaverInstructionProgramDefinitions.SpinningFacingLeft + 20 * ((index - 2) / 4) + 2 + 4 * ((index - 2) % 4));
    }
    /// <summary>Matches either byte of a compiled Waver word in bank $A3.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < WaverInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = WaverInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
