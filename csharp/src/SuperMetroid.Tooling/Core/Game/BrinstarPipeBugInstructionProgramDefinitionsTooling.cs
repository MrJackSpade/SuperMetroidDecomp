using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BrinstarPipeBugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BrinstarPipeBugInstructionProgramDefinitions))]
internal abstract class BrinstarPipeBugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 60;
    public static int PresentationWordCount => 44;
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
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xb30000 &&
        (BrinstarPipeBugInstructionProgramDefinitions.TryRead((ushort)address, out _) || BrinstarPipeBugInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
