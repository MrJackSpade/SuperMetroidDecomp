namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the sixteen Bomb Torizo statue-fragment programs at
/// $86:A4C3-$A5D3. Their thirty-two interleaved spritemap operands use extracted
/// presentation art. The packed sound-ID byte has its own typed sound decoder.
/// </summary>
internal abstract class BombTorizoStatueInstructionProgramDefinitions
{
    /// <summary>First program, <c>InstList_EnemyProjectile_BombTorizoChozoBreaking_Index0</c>, at $86:A4C3.</summary>
    internal const ushort FirstProgram = 0xa4c3;
    /// <summary>Byte distance between consecutive fragment programs.</summary>
    internal const ushort ProgramStride = 0x0011;
    /// <summary>Number of authored fragment programs selected by even parameters $00-$1E.</summary>
    internal const int ProgramCount = 16;

    /// <summary>$86:A4C7 and each subsequent fragment program's +4 command: library 2, sound $0C, Max6, immediately before falling starts.</summary>
    internal static EnemySoundRequest ReleaseSound(ushort commandAddress)
    {
        if (!TryDecodeProgramOffset(commandAddress, out _, out int offset) || offset != 4)
            throw new InvalidDataException($"Invalid statue-fragment sound command $86:{commandAddress:X4}.");
        return new(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0c), 6);
    }

    // Fragment release delays decrease by eight ticks, with a sixty-four-tick floor.
    /// <summary>Computes a fragment's initial delay, decreasing by eight ticks per program to a 64-tick minimum.</summary>
    /// <param name="programIndex">Zero-based fragment program index.</param>
    /// <returns>The delay encoded by that program's first instruction word.</returns>
    private static ushort InitialDuration(int programIndex) => (ushort)Math.Max(64, 128 - 8 * programIndex);
    /// <summary>Number of presentation operands stored across all fragment programs.</summary>
    public static int PresentationWordCount => ProgramCount * 2;

    /// <summary>Returns the bank-local start pointer for one fragment's compiled instruction program.</summary>
    /// <param name="index">Zero-based program index in the sixteen-entry fragment sequence.</param>
    /// <returns>The program's address within bank $86.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the compiled program range.</exception>
    internal static ushort Program(int index)
    {
        if ((uint)index >= ProgramCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return unchecked((ushort)(FirstProgram + index * ProgramStride));
    }
    /// <summary>Maps an interleaved presentation-word index to its operand address in the fragment programs.</summary>
    /// <param name="index">Index from zero through <see cref="PresentationWordCount"/> minus one.</param>
    /// <returns>The bank-local address of the selected spritemap pointer operand.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        ushort program = Program(index / 2);
        return unchecked((ushort)(program + ((index & 1) == 0 ? 2 : 13)));
    }

    /// <summary>Reports whether an address points to one of the two visual spritemap operands in a program.</summary>
    /// <param name="address">Bank-local address to classify.</param>
    /// <returns><see langword="true"/> when the address has operand offset two or thirteen in a compiled program.</returns>
    internal static bool IsPresentationWord(ushort address) =>
        TryDecodeProgramOffset(address, out _, out int offset) && offset is 2 or 13;

    /// <summary>Reads a compiled non-presentation word such as a delay, instruction pointer, or pre-instruction operand.</summary>
    /// <param name="address">Bank-local address of the mechanics word.</param>
    /// <returns>The value used by the interpreted fragment program at that address.</returns>
    /// <exception cref="InvalidDataException">The address is outside the compiled program block or is not a mechanics operand.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (!TryDecodeProgramOffset(address, out int programIndex, out int offset))
        {
            throw new InvalidDataException(
                $"Bomb Torizo statue mechanics pointer $86:{address:X4} is not compiled.");
        }

        return offset switch
        {
            0 => InitialDuration(programIndex),
            4 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6,
            7 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY,
            9 => EnemyProjectileCodePointers.PreInst_EnemyProjectile_BombTorizoChozoBreaking_Falling,
            11 => 0x0070,
            15 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
            _ => throw new InvalidDataException(
                $"Bomb Torizo statue mechanics pointer $86:{address:X4} is not compiled."),
        };
    }

    /// <summary>Splits an address in the compiled block into its fragment index and byte offset.</summary>
    /// <param name="address">Bank-local address to decode.</param>
    /// <param name="programIndex">Receives the containing program index, or zero when the address is outside the block.</param>
    /// <param name="offset">Receives the byte offset within that program, or zero when the address is outside the block.</param>
    /// <returns><see langword="true"/> when the address lies within the sixteen consecutive program records.</returns>
    internal static bool TryDecodeProgramOffset(
        ushort address,
        out int programIndex,
        out int offset)
    {
        int relative = address - FirstProgram;
        if ((uint)relative >= ProgramCount * ProgramStride)
        {
            programIndex = 0;
            offset = 0;
            return false;
        }

        programIndex = relative / ProgramStride;
        offset = relative % ProgramStride;
        return true;
    }
}
