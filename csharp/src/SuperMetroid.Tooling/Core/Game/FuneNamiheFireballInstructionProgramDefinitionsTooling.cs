using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FuneNamiheFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FuneNamiheFireballInstructionProgramDefinitions))]
internal abstract class FuneNamiheFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Three fireball drawing operands for each facing direction.</summary>
    public static int PresentationWordCount => FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates the drawing operand paired with a five-tick fireball frame.</summary>
    public static ushort PresentationWordAddress(int index) => FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Each facing has three five-tick frames followed by a jump to its start.</summary>
    public static int MechanicsWordCount => 10;
    /// <summary>Reads a frame duration or closing jump from the selected facing list.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int word = index % 5;
        ushort address = (ushort)(FuneNamiheFireballInstructionProgramDefinitions.Left + 16 * (index / 5) +
            (word < 3 ? 4 * word : 12 + 2 * (word - 3)));
        return new(address, FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Recognizes duration and jump bytes in the two bank-$86 fireball lists.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = unchecked((ushort)address) - FuneNamiheFireballInstructionProgramDefinitions.Left;
        return (uint)offset < 32 && (offset % 16 >= 12 || offset % 4 < 2);
    }
}
