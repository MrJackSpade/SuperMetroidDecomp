using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CrocomireTongueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CrocomireTongueInstructionProgramDefinitions))]
internal abstract class CrocomireTongueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 14;
    public static int PresentationWordCount => 9;
    /// <summary>Enumerates four fight durations and their loop, terminal sleep,
    /// then five melting durations and their loop, in native address order.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index == 6) return new(CrocomireTongueInstructionProgramDefinitions.Sleep, (ushort)CommonEnemyInstruction.Sleep);
        int start = index < 6 ? CrocomireTongueInstructionProgramDefinitions.Fight : CrocomireTongueInstructionProgramDefinitions.Melting;
        int frameCount = index < 6 ? 4 : 5;
        int field = index < 6 ? index : index - 7;
        int offset = field < frameCount ? 4 * field : 4 * frameCount + 2 * (field - frameCount);
        ushort address = (ushort)(start + offset);
        return new(address, CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Spritemap operand positions in the four-frame fight and five-frame
    /// melting loops: start + 2 + 4*frame. These positions do not own artwork.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 4 ? CrocomireTongueInstructionProgramDefinitions.Fight + 2 + 4 * index : CrocomireTongueInstructionProgramDefinitions.Melting + 2 + 4 * (index - 4));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa40000) return false;
        int bankAddress = address & 0xffff;
        if (bankAddress is CrocomireTongueInstructionProgramDefinitions.Sleep or (CrocomireTongueInstructionProgramDefinitions.Sleep + 1)) return true;
        int offset = bankAddress < CrocomireTongueInstructionProgramDefinitions.Melting ? bankAddress - CrocomireTongueInstructionProgramDefinitions.Fight : bankAddress - CrocomireTongueInstructionProgramDefinitions.Melting;
        int frameCount = bankAddress < CrocomireTongueInstructionProgramDefinitions.Melting ? 4 : 5;
        return offset >= 0 && offset < frameCount * 4 + 4 &&
            (offset >= frameCount * 4 || offset % 4 < 2);
    }
}
