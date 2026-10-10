namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the horizontal shutter's stationary program.
/// Its interleaved spritemap operand selects separately installed presentation data.
/// </summary>
internal abstract class HorizontalShutterInstructionProgramDefinitions
{
    /// <summary><c>InstList_ShutterHorizontal</c> at $A2:E9D4.</summary>
    internal const ushort Stationary = 0xe9d4;

    internal const ushort PresentationWord = 0xe9d6;

    /// <summary>True only for the stationary horizontal shutter's visual operand.</summary>
    internal static bool IsPresentationWord(ushort address) => address == PresentationWord;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == Stationary) return 1;
        if (address == Stationary + 4) return (ushort)CommonEnemyInstruction.Sleep;
        throw new InvalidDataException(
            $"Horizontal-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}
