using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="AlcoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(AlcoonInstructionProgramDefinitions))]
internal abstract class AlcoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    // Each facing occupies112 bytes: walking28, three-shot volley72, two sleeping poses12.
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 68;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => 44;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 34;
        int offset;
        if (word < 10)
            offset = word < 8 ? 6 * (word / 2) + 2 * (word % 2) : 24 + 2 * (word - 8);
        else if (word < 28)
        {
            int volleyWord = word - 10;
            int stage = volleyWord % 6;
            offset = 28 + 22 * (volleyWord / 6) + (stage < 5 ? 4 * stage : 18);
        }
        else if (word < 30) offset = 94 + 2 * (word - 28);
        else offset = 100 + 6 * ((word - 30) / 2) + 4 * ((word - 30) % 2);
        ushort address = (ushort)(AlcoonInstructionProgramDefinitions.WalkingLeft + 112 * (index / 34) + offset);
        return new(address, AlcoonInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int frame = index % 22;
        int offset;
        if (frame < 4) offset = 4 + 6 * frame;
        else if (frame < 19)
        {
            int volleyFrame = frame - 4;
            int stage = volleyFrame % 5;
            offset = 28 + 22 * (volleyFrame / 5) + 2 + 4 * stage + (stage == 4 ? 2 : 0);
        }
        else offset = frame == 19 ? 98 : 102 + 6 * (frame - 20);
        return (ushort)(AlcoonInstructionProgramDefinitions.WalkingLeft + 112 * (index / 22) + offset);
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa80000) return false;
        int offset = (ushort)address - AlcoonInstructionProgramDefinitions.WalkingLeft;
        return (uint)offset < 224 &&
            AlcoonInstructionProgramDefinitions.TryReadMechanicsWord((ushort)(AlcoonInstructionProgramDefinitions.WalkingLeft + (offset & ~1)), out _);
    }
}
