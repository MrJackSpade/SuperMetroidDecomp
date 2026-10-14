using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="GoldenTorizoAwakeningInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoAwakeningInstructionProgramDefinitions))]
internal abstract class GoldenTorizoAwakeningInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds visual operands in Golden Torizo's awakening sequence.</summary>
    public static int PresentationWordCount => GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an awakening visual operand for native source comparison.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Covers the 69 compiled mechanic words selected from the awakening sequence.</summary>
    public static int MechanicsWordCount => 69;
    /// <summary>Selects a mechanic entry while excluding visual entries from the sequence.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return GoldenTorizoAwakeningInstructionProgramDefinitions.Select(index, visual: false);
    }
    /// <summary>Matches either byte of a selected mechanic word within bank AA.</summary>
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
