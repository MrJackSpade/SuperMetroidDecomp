namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Powamp's looping spike animation and private delete list.
/// Interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class PowampSpikeInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PowampSpike</c> at $86:D208.</summary>
    internal const ushort Initial = 0xd208;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the spike loop at $86:D214.
    /// </summary>
    internal const ushort LoopCommand = 0xd214;

    /// <summary><c>InstList_EnemyProjectile_PowampSpike_Delete</c> at $86:D218.</summary>
    internal const ushort Delete = 0xd218;

    /// <summary>Number of spritemap operands interleaved in the three looping spike-frame records.</summary>
    public static int PresentationWordCount => 3;

    /// <summary>Gets the bank-local address of an interleaved spike-frame spritemap operand.</summary>
    /// <param name="index">Zero-based frame operand index from zero through <see cref="PresentationWordCount"/> minus one.</param>
    /// <returns>The instruction address of the frame's compiled presentation pointer.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + index * 4 + 2);
    }

    /// <summary>Reads a compiled spike-program timing or control word.</summary>
    /// <param name="address">Bank-$86 instruction address to resolve.</param>
    /// <returns>The compiled duration, loop command or delete command value.</returns>
    /// <exception cref="InvalidDataException">The address is not part of the compiled spike programs.</exception>
    internal static ushort ReadMechanicsWord(ushort address) => TryRead(address, out ushort value)
        ? value : throw new InvalidDataException($"Powamp-spike instruction mechanics pointer $86:{address:X4} is not compiled.");

    // Three six-frame drawings repeat until the producer chooses the separate delete program.
    /// <summary>Resolves a known mechanics position in the looping spike program or its separate delete list.</summary>
    /// <param name="address">Bank-local instruction address to inspect.</param>
    /// <param name="value">Receives the compiled word when found; zero when the address is unrecognized.</param>
    /// <returns><see langword="true"/> for a frame duration, loop control word, or delete command position.</returns>
    internal static bool TryRead(ushort address, out ushort value)
    {
        value = 0;
        if (address >= Initial && address < LoopCommand && (address - Initial) % 4 == 0)
        { value = 6; return true; }
        if (address == LoopCommand)
        { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY; return true; }
        if (address == LoopCommand + 2) { value = Initial; return true; }
        if (address == Delete)
        { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete; return true; }
        return false;
    }
}
