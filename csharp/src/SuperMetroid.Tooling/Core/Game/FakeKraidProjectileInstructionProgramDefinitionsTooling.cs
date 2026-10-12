using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FakeKraidProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FakeKraidProjectileInstructionProgramDefinitions))]
internal abstract class FakeKraidProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Each of Fake Kraid's spit and two spike poses has a live spritemap operand.</summary>
    public static int PresentationWordCount => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates the artwork selection interleaved between a pose duration and terminal sleep.</summary>
    public static ushort PresentationWordAddress(int index) => FakeKraidProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>The three six-byte projectile poses each contribute a duration and terminal sleep word to compiled control.</summary>
    public static int MechanicsWordCount => 6;
    /// <summary>Reads a Fake Kraid pose's duration or sleep word, skipping its middle visual operand.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort address = (ushort)(FakeKraidProjectileInstructionProgramDefinitions.Spit + 6 * (index / 2) + 4 * (index % 2));
        return new(address, FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Matches the first and last two bytes of each six-byte pose in bank $86, leaving the spritemap operand editable.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = (ushort)address - FakeKraidProjectileInstructionProgramDefinitions.Spit;
        return (uint)offset < 18 && offset % 6 is 0 or 1 or 4 or 5;
    }
}
