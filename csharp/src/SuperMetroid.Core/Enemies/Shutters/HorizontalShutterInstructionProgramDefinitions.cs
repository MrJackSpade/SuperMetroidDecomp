namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// Compiled engine-control words for the horizontal shutter's stationary program.
/// Its interleaved spritemap operand selects separately installed presentation data.
/// </summary>
internal abstract class HorizontalShutterInstructionProgramDefinitions
{
    /// <summary><c>InstList_ShutterHorizontal</c> at $A2:E9D4.</summary>
    internal const ushort Stationary = 0xe9d4;

    /// <summary>The stationary program's visual operand at $A2:E9D6, between its delay and sleep words.</summary>
    internal const ushort PresentationWord = 0xe9d6;

    /// <summary>True only for the stationary horizontal shutter's visual operand.</summary>
    internal static bool IsPresentationWord(ushort address) => address == PresentationWord;

    /// <summary>Decodes the one-tick hold and following sleep command; rejects other bank-relative addresses.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == Stationary) return 1;
        if (address == Stationary + 4) return (ushort)CommonEnemyInstruction.Sleep;
        throw new InvalidDataException(
            $"Horizontal-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}
