using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BrinstarPipeBugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BrinstarPipeBugInstructionProgramDefinitions))]
internal abstract class BrinstarPipeBugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of fixed timing and control words across the eight pipe-bug programs.</summary>
    public static int MechanicsWordCount => 60;

    /// <summary>Number of sprite-selector words supplied as presentation data across all eight programs.</summary>
    public static int PresentationWordCount => 44;

    /// <summary>Maps a flattened mechanics slot to its program address and fixed operand.</summary>
    /// <param name="index">The zero-based slot across all eight programs in catalog order.</param>
    /// <returns>The address and decoded mechanics value for that slot.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        for (int program = 0; program < 8; program++)
        {
            int frames = BrinstarPipeBugInstructionProgramDefinitions.Frames(program);
            if (index < frames + 2)
            {
                ushort address = (ushort)(BrinstarPipeBugInstructionProgramDefinitions.Start(program) + (index < frames ? index * 4 : frames * 4 + (index - frames) * 2));
                return new(address, BrinstarPipeBugInstructionProgramDefinitions.ReadMechanicsWord(address));
            }
            index -= frames + 2;
        }
        throw new IndexOutOfRangeException();
    }
    /// <summary>Maps a flattened presentation slot to its sprite-selector word address.</summary>
    /// <param name="index">The zero-based selector slot across all eight programs in catalog order.</param>
    /// <returns>The bank-$B3 address containing the selected presentation word.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        for (int program = 0; program < 8; program++)
        {
            int frames = BrinstarPipeBugInstructionProgramDefinitions.Frames(program);
            if (index < frames)
                return (ushort)(BrinstarPipeBugInstructionProgramDefinitions.Start(program) + index * 4 + 2);
            index -= frames;
        }
        throw new IndexOutOfRangeException();
    }
    /// <summary>Checks whether an address is either byte of a compiled mechanics operand.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the byte belongs to a compiled bank-$B3 mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xb30000 &&
        (BrinstarPipeBugInstructionProgramDefinitions.TryRead((ushort)address, out _) || BrinstarPipeBugInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
