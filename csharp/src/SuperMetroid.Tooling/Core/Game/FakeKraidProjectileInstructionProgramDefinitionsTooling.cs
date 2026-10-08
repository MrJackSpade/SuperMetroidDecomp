using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FakeKraidProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FakeKraidProjectileInstructionProgramDefinitions))]
internal abstract class FakeKraidProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    // Each pose occupies six bytes: duration, visual operand, terminal sleep.
    public static int MechanicsWordCount => 6;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort address = (ushort)(FakeKraidProjectileInstructionProgramDefinitions.Spit + 6 * (index / 2) + 4 * (index % 2));
        return new(address, FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = (ushort)address - FakeKraidProjectileInstructionProgramDefinitions.Spit;
        return (uint)offset < 18 && offset % 6 is 0 or 1 or 4 or 5;
    }
}
