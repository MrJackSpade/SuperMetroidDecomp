using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MetroidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MetroidInstructionProgramDefinitions))]
internal abstract class MetroidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of timing and control-flow words in the chasing and draining programs.</summary>
    public static int MechanicsWordCount => 31;

    /// <summary>Gets the number of live spritemap operands interleaved with those programs.</summary>
    public static int PresentationWordCount => 25;

    /// <summary>Returns one compiled timing, sound-callback, or loop-control word.</summary>
    /// <param name="index">Zero-based index through the chasing program followed by the draining program.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 31 compiled words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool chasing = index < 23;
        int local = chasing ? index : index - 23;
        int frames = chasing ? 20 : 5;
        ushort start = chasing ? MetroidInstructionProgramDefinitions.ChasingSamus : MetroidInstructionProgramDefinitions.DrainingSamus;
        ushort address = (ushort)(start + (local < frames ? local * 4 : frames * 4 + (local - frames) * 2));
        return new(address, MetroidInstructionProgramDefinitions.ReadMechanicsWord(address));
    }

    /// <summary>Returns the bank-$A3 address of a live spritemap operand in either program.</summary>
    /// <param name="index">Zero-based operand index through the chasing and draining programs.</param>
    /// <returns>Address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 25 presentation operands.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 20 ? MetroidInstructionProgramDefinitions.ChasingSamus + index * 4 + 2 : MetroidInstructionProgramDefinitions.DrainingSamus + (index - 20) * 4 + 2);
    }

    /// <summary>Checks whether a bank-$A3 byte is compiled timing or control data, excluding spritemap operands.</summary>
    /// <param name="address">24-bit cartridge address to classify.</param>
    /// <returns><see langword="true"/> for a compiled mechanics-word byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int pointer = (ushort)address;
        bool chasing = pointer < MetroidInstructionProgramDefinitions.DrainingSamus;
        int offset = pointer - (chasing ? MetroidInstructionProgramDefinitions.ChasingSamus : MetroidInstructionProgramDefinitions.DrainingSamus);
        int timedBytes = chasing ? 80 : 20;
        return (uint)offset < timedBytes + 6 && (offset >= timedBytes || offset % 4 < 2);
    }
}
