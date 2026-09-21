namespace SuperMetroid.Core.Game;

/// <summary>One compiled Powamp-spike mechanics word at its bank-$86 address.</summary>
internal readonly record struct PowampSpikeInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Powamp's looping spike animation and private delete list.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class PowampSpikeInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PowampSpike</c> at $86:D208.</summary>
    internal const ushort Initial = 0xd208;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the spike loop at $86:D214.
    /// </summary>
    internal const ushort LoopCommand = 0xd214;

    /// <summary><c>InstList_EnemyProjectile_PowampSpike_Delete</c> at $86:D218.</summary>
    internal const ushort Delete = 0xd218;

    private static readonly PowampSpikeInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0006),
        new(0xd20c, 0x0006),
        new(0xd210, 0x0006),
        new(LoopCommand, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xd216, Initial),
        new(Delete, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords = [0xd20a, 0xd20e, 0xd212];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static PowampSpikeInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PowampSpikeInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Powamp-spike instruction mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
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
