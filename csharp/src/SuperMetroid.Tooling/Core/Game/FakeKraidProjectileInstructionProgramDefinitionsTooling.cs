using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FakeKraidProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FakeKraidProjectileInstructionProgramDefinitions))]
internal abstract class FakeKraidProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    // Each pose occupies six bytes: duration, visual operand, terminal sleep.
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 6;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort address = (ushort)(FakeKraidProjectileInstructionProgramDefinitions.Spit + 6 * (index / 2) + 4 * (index % 2));
        return new(address, FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = (ushort)address - FakeKraidProjectileInstructionProgramDefinitions.Spit;
        return (uint)offset < 18 && offset % 6 is 0 or 1 or 4 or 5;
    }
}
