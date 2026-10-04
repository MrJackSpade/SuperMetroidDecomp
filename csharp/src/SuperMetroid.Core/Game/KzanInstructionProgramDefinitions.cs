namespace SuperMetroid.Core.Game;

internal readonly record struct KzanInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the Kzan spike-platform animation program.
/// Its interleaved spritemap operand remains live cartridge presentation data.
/// </summary>
internal static class KzanInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kzan</c> at $A6:8B29.</summary>
    internal const ushort Idle = 0x8b29;

    /// <summary>Address of the live <c>Spritemap_Kzan</c> operand at $A6:8B2B.</summary>
    internal const ushort PresentationWord = 0x8b2b;

    internal static int MechanicsWordCount => 2;
    internal static KzanInstructionMechanicsWord MechanicsWord(int index) => index switch
    {
        0 => new(Idle, 1),
        1 => new(Idle + 4, CommonEnemyInstructionCodes.Sleep),
        _ => throw new IndexOutOfRangeException(),
    };

    internal static ushort ReadMechanicsWord(ushort address) => address switch
    {
        Idle => 1,
        Idle + 4 => CommonEnemyInstructionCodes.Sleep,
        _ => throw new InvalidDataException(
            $"Kzan instruction mechanics pointer $A6:{address:X4} is not compiled."),
    };

    internal static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa60000 &&
        ((ushort)address - Idle is 0 or 1 or 4 or 5);
}