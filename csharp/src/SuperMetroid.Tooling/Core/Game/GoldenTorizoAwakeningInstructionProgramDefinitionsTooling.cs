using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoAwakeningInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoAwakeningInstructionProgramDefinitions))]
internal abstract class GoldenTorizoAwakeningInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 69;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return GoldenTorizoAwakeningInstructionProgramDefinitions.Select(index, visual: false);
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
