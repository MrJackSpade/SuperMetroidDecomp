using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WaverInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WaverInstructionProgramDefinitions))]
internal abstract class WaverInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => WaverInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => WaverInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => 10;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return index < 2 ? (ushort)(WaverInstructionProgramDefinitions.SteadyFacingLeft + 6 * index + 2)
            : (ushort)(WaverInstructionProgramDefinitions.SpinningFacingLeft + 20 * ((index - 2) / 4) + 2 + 4 * ((index - 2) % 4));
    }
    /// <summary>Checks whether compiled mechanics byte.</summary>
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
