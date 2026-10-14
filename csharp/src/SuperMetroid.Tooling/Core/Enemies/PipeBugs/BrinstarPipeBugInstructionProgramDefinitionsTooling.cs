using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Development-tool members of <see cref="BrinstarPipeBugInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BrinstarPipeBugInstructionProgramDefinitions))]
internal abstract class BrinstarPipeBugInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Accounts for timed frames and two trailer words across eight pipe-bug programs.</summary>
    public static int MechanicsWordCount => 60;
    /// <summary>Accounts for the 44 frames interleaved across eight pipe-bug programs.</summary>
    public static int PresentationWordCount => 44;
    /// <summary>Walks each program's frame durations before its two control words.</summary>
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
    /// <summary>Locates the sprite word following a frame duration in one of eight programs.</summary>
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
    /// <summary>Recognizes compiled mechanic bytes in bank B3, including second bytes of words.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == 0xb30000 &&
        (BrinstarPipeBugInstructionProgramDefinitions.TryRead((ushort)address, out _) || BrinstarPipeBugInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
