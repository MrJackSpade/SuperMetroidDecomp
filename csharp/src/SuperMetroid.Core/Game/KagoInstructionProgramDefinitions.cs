namespace SuperMetroid.Core.Game;

internal readonly record struct KagoInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for Kago's slow and post-hit animation loops.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class KagoInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kago_Initial_SlowAnimation</c> at $A8:AB1E.</summary>
    internal const ushort Slow = 0xab1e;
    /// <summary><c>InstList_Kago_TakenHit_FastAnimation</c> at $A8:AB32.</summary>
    internal const ushort Fast = 0xab32;

    private static readonly KagoInstructionMechanicsWord[] Words =
    [
        new(0xab1e, 10), new(0xab22, 10), new(0xab26, 10), new(0xab2a, 10),
        new(0xab2e, CommonEnemyInstructionCodes.Goto), new(0xab30, Slow),
        new(0xab32, 3), new(0xab36, 3), new(0xab3a, 3), new(0xab3e, 3),
        new(0xab42, CommonEnemyInstructionCodes.Goto), new(0xab44, Fast),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xab20, 0xab24, 0xab28, 0xab2c,
        0xab34, 0xab38, 0xab3c, 0xab40,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KagoInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Kago instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
