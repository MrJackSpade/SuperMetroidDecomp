namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the ordinary elevator's two-frame animation loop.
/// Its two spritemap operands resolve through compiled presentation selectors.
/// </summary>
internal abstract class ElevatorInstructionProgramDefinitions
{
    /// <summary><c>InstList_Elevator</c> at $A3:94D6.</summary>
    internal const ushort Loop = 0x94d6;
    /// <summary>Identifies either of the two spritemap operand words in the elevator's two-frame loop.</summary>
    /// <param name="address">The bank-local address to test.</param>
    /// <returns><see langword="true"/> when the address selects a compiled presentation frame.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Loop + 2);
        return (uint)offset < 8 && offset % 4 == 0;
    }
    /// <summary>Returns the compiled duration, branch command, or loop target stored at an elevator instruction word.</summary>
    /// <param name="address">The bank-local instruction-word address.</param>
    /// <returns>The mechanics value encoded at the address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of the elevator loop's compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Loop;
        if (offset is 0 or 4) return 2;
        if (offset == 8) return CommonEnemyInstructionCodes.Goto;
        if (offset == 10) return Loop;
        throw new InvalidDataException(
            $"Elevator instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
