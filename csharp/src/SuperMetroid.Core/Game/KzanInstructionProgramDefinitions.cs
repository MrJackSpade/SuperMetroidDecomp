namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Kzan spike-platform animation program.
/// Its interleaved spritemap operand is resolved by the compiled visual catalog.
/// </summary>
internal abstract class KzanInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kzan</c> at $A6:8B29.</summary>
    internal const ushort Idle = 0x8b29;

    /// <summary>Resolves the one-frame duration and sleep opcode used by Kzan's idle instruction list.</summary>
    /// <param name="address">Bank-$A6 word address requested by the enemy instruction interpreter.</param>
    /// <returns>The compiled duration or common sleep instruction at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of the idle program's compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address) => address switch
    {
        Idle => 1,
        Idle + 4 => CommonEnemyInstructionCodes.Sleep,
        _ => throw new InvalidDataException(
            $"Kzan instruction mechanics pointer $A6:{address:X4} is not compiled."),
    };
}
