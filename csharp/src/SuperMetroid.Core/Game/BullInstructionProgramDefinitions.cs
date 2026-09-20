namespace SuperMetroid.Core.Game;

internal readonly record struct BullInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for Bull's ordinary and immune-shot animation programs.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class BullInstructionProgramDefinitions
{
    /// <summary><c>InstList_Bull_Normal</c> at $A8:D841.</summary>
    internal const ushort Normal = 0xd841;
    /// <summary><c>InstList_Bull_Shot_0</c> at $A8:D855.</summary>
    internal const ushort Shot = 0xd855;
    /// <summary><c>InstList_Bull_Shot_1</c> at $A8:D859.</summary>
    internal const ushort ShotLoop = 0xd859;

    private static readonly BullInstructionMechanicsWord[] Words =
    [
        new(0xd841, 10), new(0xd845, 10), new(0xd849, 10), new(0xd84d, 10),
        new(0xd851, CommonEnemyInstructionCodes.Goto), new(0xd853, Normal),

        new(0xd855, CommonEnemyInstructionCodes.SetTimer), new(0xd857, 5),
        new(0xd859, 3), new(0xd85d, 3), new(0xd861, 3), new(0xd865, 3),
        new(0xd869, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xd86b, ShotLoop),
        new(0xd86d, CommonEnemyInstructionCodes.Goto), new(0xd86f, Normal),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xd843, 0xd847, 0xd84b, 0xd84f,
        0xd85b, 0xd85f, 0xd863, 0xd867,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static BullInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Bull instruction mechanics pointer $A8:{address:X4} is not compiled.");
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
