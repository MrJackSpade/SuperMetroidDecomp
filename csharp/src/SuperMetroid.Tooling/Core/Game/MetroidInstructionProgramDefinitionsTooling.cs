using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MetroidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MetroidInstructionProgramDefinitions))]
internal abstract class MetroidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts timed control and terminal words across Metroid chasing and draining programs.</summary>
    public static int MechanicsWordCount => 31;
    /// <summary>Counts twenty chase visuals and five drain visuals.</summary>
    public static int PresentationWordCount => 25;
    /// <summary>Maps a mechanics index through the chase or drain frame words and terminal controls.</summary>
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
    /// <summary>Locates the visual word following a chase or drain frame duration.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 20 ? MetroidInstructionProgramDefinitions.ChasingSamus + index * 4 + 2 : MetroidInstructionProgramDefinitions.DrainingSamus + (index - 20) * 4 + 2);
    }
    /// <summary>Accepts timed control words and terminal controls, excluding Metroid visual operands in bank $A3.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
        int pointer = (ushort)address;
        bool chasing = pointer < MetroidInstructionProgramDefinitions.DrainingSamus;
        int offset = pointer - (chasing ? MetroidInstructionProgramDefinitions.ChasingSamus : MetroidInstructionProgramDefinitions.DrainingSamus);
        int timedBytes = chasing ? 80 : 20;
        return (uint)offset < timedBytes + 6 && (offset >= timedBytes || offset % 4 < 2);
    }
}
