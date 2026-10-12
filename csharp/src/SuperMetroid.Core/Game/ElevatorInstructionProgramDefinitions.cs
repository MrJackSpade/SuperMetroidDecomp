namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the ordinary elevator's two-frame animation loop.
/// Its two spritemap operands resolve through compiled presentation selectors.
/// </summary>
internal abstract class ElevatorInstructionProgramDefinitions
{
    /// <summary><c>InstList_Elevator</c> at $A3:94D6.</summary>
    internal const ushort Loop = 0x94d6;
    /// <summary>Identifies either of the two spritemap operands interleaved with the elevator loop's two-tick waits.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Loop + 2);
        return (uint)offset < 8 && offset % 4 == 0;
    }
    /// <summary>Returns the compiled waits and backward goto for the bank-$A3 loop, rejecting its visual operands and unknown addresses.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Loop;
        if (offset is 0 or 4) return 2;
        if (offset == 8) return (ushort)CommonEnemyInstruction.Goto;
        if (offset == 10) return Loop;
        throw new InvalidDataException(
            $"Elevator instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
