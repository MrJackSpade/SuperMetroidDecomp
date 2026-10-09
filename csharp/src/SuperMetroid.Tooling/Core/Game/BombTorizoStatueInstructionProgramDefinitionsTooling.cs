using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BombTorizoStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BombTorizoStatueInstructionProgramDefinitions))]
internal abstract class BombTorizoStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of presentation-owned spritemap-pointer operands across the compiled Bomb Torizo statue programs.</summary>
    public static int PresentationWordCount => BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps a presentation-operand index to its bank-local spritemap-pointer address.</summary>
    /// <param name="index">Zero-based position in the presentation operand sequence.</param>
    /// <returns>The bank-local address of the selected spritemap-pointer operand.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the presentation operand sequence.</exception>
    public static ushort PresentationWordAddress(int index) => BombTorizoStatueInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of fixed control words compiled from all Bomb Torizo statue instruction programs.</summary>
    public static int MechanicsWordCount => BombTorizoStatueInstructionProgramDefinitions.ProgramCount * 6;

    /// <summary>Returns one compiled control word in program order, skipping presentation-owned operands.</summary>
    /// <param name="index">Zero-based position in the flattened sequence, with six mechanics words per program.</param>
    /// <returns>The native address and value of the selected duration, sound, callback, or delete word.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int programIndex = index / 6;
        ushort program = BombTorizoStatueInstructionProgramDefinitions.Program(programIndex);
        int word = index % 6;
        // First timed frame, three-byte sound command, callback plus argument,
        // second timed frame, then delete. Reuse the direct reader for values.
        int offset = word switch
        {
            0 => 0,
            1 => 4,
            2 or 3 or 4 => 7 + 2 * (word - 2),
            _ => 15,
        };
        ushort address = (ushort)(program + offset);
        return new(address, BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Tests whether a full bank address identifies either byte of a compiled mechanics word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address is in the projectile-code bank and belongs to a mechanics word; otherwise <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        return IsMechanicsWordStart(bankAddress) ||
            IsMechanicsWordStart(unchecked((ushort)(bankAddress - 1)));
    }
    /// <summary>Recognizes the first byte of a compiled control word within any catalogued program.</summary>
    /// <param name="address">Bank-relative address to check.</param>
    /// <returns><see langword="true"/> when the address decodes to a compiled control-word offset.</returns>
    internal static bool IsMechanicsWordStart(ushort address) =>
        BombTorizoStatueInstructionProgramDefinitions.TryDecodeProgramOffset(address, out _, out int offset) &&
        offset is 0 or 4 or 7 or 9 or 11 or 15;
}
