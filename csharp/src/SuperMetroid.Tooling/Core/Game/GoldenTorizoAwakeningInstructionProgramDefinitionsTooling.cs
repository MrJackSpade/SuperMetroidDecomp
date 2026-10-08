using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoAwakeningInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoAwakeningInstructionProgramDefinitions))]
internal abstract class GoldenTorizoAwakeningInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 69;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return GoldenTorizoAwakeningInstructionProgramDefinitions.Select(index, visual: false);
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
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
