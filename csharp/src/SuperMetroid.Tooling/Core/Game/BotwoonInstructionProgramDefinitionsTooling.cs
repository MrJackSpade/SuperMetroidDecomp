using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BotwoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BotwoonInstructionProgramDefinitions))]
internal abstract class BotwoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => BotwoonInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => BotwoonInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 74;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address;
        if (index < 24)
        {
            int word = index % 3;
            address = (ushort)((ushort)BotwoonMovementProgram.UpLeft + 8 * BotwoonInstructionProgramDefinitions.PhysicalDirection(index / 3) + (word == 0 ? 0 : word == 1 ? 2 : 6));
        }
        else if (index < 26) address = (ushort)(BotwoonInstructionProgramDefinitions.Hidden + 4 * (index - 24));
        else
        {
            int word = (index - 26) % 6;
            int offset = word == 0 ? 0 : word == 5 ? 14 : 2 + 2 * word;
            address = (ushort)(BotwoonInstructionProgramDefinitions.SpittingUpLeft + 16 * BotwoonInstructionProgramDefinitions.PhysicalDirection((index - 26) / 6) + offset);
        }
        return new(address, BotwoonInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    internal static bool IsMechanicsWord(ushort address) => address == BotwoonInstructionProgramDefinitions.Hidden || address == BotwoonInstructionProgramDefinitions.Hidden + 4 ||
        (BotwoonInstructionProgramDefinitions.TryDecodeDirectional(address, out bool spitting, out _, out int offset) &&
            (spitting ? offset is 0 or 4 or 6 or 8 or 10 or 14 : offset is 0 or 2 or 6));
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xb30000 &&
        (IsMechanicsWord(unchecked((ushort)address)) || IsMechanicsWord(unchecked((ushort)(address - 1))));
}
