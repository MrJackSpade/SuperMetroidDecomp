namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Kzan spike-platform animation program.
/// Its interleaved spritemap operand is resolved by the compiled visual catalog.
/// </summary>
internal abstract class KzanInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kzan</c> at $A6:8B29.</summary>
    internal const ushort Idle = 0x8b29;

    /// <summary>Decodes only the idle frame's one-tick delay and its following sleep command; other addresses are not compiled.</summary>
    internal static ushort ReadMechanicsWord(ushort address) => address switch
    {
        Idle => 1,
        Idle + 4 => (ushort)CommonEnemyInstruction.Sleep,
        _ => throw new InvalidDataException(
            $"Kzan instruction mechanics pointer $A6:{address:X4} is not compiled."),
    };
}
