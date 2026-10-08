using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FuneNamiheFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FuneNamiheFireballInstructionProgramDefinitions))]
internal abstract class FuneNamiheFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress(index);
    // Each facing has three five-tick frames followed by a jump to its start.
    public static int MechanicsWordCount => 10;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int word = index % 5;
        ushort address = (ushort)(FuneNamiheFireballInstructionProgramDefinitions.Left + 16 * (index / 5) +
            (word < 3 ? 4 * word : 12 + 2 * (word - 3)));
        return new(address, FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = unchecked((ushort)address) - FuneNamiheFireballInstructionProgramDefinitions.Left;
        return (uint)offset < 32 && (offset % 16 >= 12 || offset % 4 < 2);
    }
}
