using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MetroidInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MetroidInstructionProgramDefinitions))]
internal abstract class MetroidInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 31;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => 25;
    /// <summary>Mechanics word.</summary>
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
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 20 ? MetroidInstructionProgramDefinitions.ChasingSamus + index * 4 + 2 : MetroidInstructionProgramDefinitions.DrainingSamus + (index - 20) * 4 + 2);
    }
    /// <summary>Checks whether an address identifies a compiled mechanics byte.</summary>
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
