using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PlatformInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PlatformInstructionProgramDefinitions))]
internal abstract class PlatformInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 56;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => 32;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int word = index % 7;
        int offset = word == 0 ? 0 : word < 5 ? 2 + 4 * (word - 1) : 18 + 2 * (word - 5);
        ushort address = (ushort)(PlatformInstructionProgramDefinitions.KamerMovingLeft + 22 * (index / 7) + offset);
        return new(address, PlatformInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(PlatformInstructionProgramDefinitions.KamerMovingLeft + 22 * (index / 4) + 4 + 4 * (index % 4));
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
        int offset = unchecked((ushort)address) - PlatformInstructionProgramDefinitions.KamerMovingLeft;
        if ((uint)offset >= 176) return false;
        int local = offset % 22;
        return local < 2 || local >= 18 || (local - 2) % 4 < 2;
    }
}
