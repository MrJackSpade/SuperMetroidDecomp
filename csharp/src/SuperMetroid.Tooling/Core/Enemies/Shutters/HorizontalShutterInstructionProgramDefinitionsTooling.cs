using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Development-tool members of <see cref="HorizontalShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HorizontalShutterInstructionProgramDefinitions))]
internal abstract class HorizontalShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The stationary shutter's frame duration and terminating command.</summary>
    public static int MechanicsWordCount => 2;
    /// <summary>The drawing operand between the stationary duration and command.</summary>
    public static int PresentationWordCount => 1;
    /// <summary>Reads the duration or closing command from the stationary shutter list.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(HorizontalShutterInstructionProgramDefinitions.Stationary + 4 * index);
        return new(address, HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Locates the sole shutter drawing operand and rejects other indices.</summary>
    public static ushort PresentationWordAddress(int index) => index == 0
        ? HorizontalShutterInstructionProgramDefinitions.PresentationWord
        : throw new ArgumentOutOfRangeException(nameof(index));
    /// <summary>Recognizes the two bank-$A2 words around the presentation operand.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - HorizontalShutterInstructionProgramDefinitions.Stationary;
        return (uint)offset < 6 && (offset < 2 || offset >= 4);
    }
}
