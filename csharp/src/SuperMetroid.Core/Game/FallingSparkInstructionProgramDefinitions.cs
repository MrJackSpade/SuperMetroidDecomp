namespace SuperMetroid.Core.Game;

/// <summary>One compiled Falling Spark mechanics word at its bank-$86 address.</summary>
internal readonly record struct FallingSparkInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Falling Spark's falling and floor-impact programs. Interleaved
/// spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class FallingSparkInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_FallingSpark_Falling</c> at $86:F353.</summary>
    internal const ushort Falling = 0xf353;

    /// <summary><c>InstList_EnemyProjectile_FallingSpark_HitFloor</c> at $86:F363.</summary>
    internal const ushort HitFloor = 0xf363;

    /// <summary>
    /// Terminal <c>Instruction_EnemyProjectile_Delete</c> word in
    /// <c>InstList_EnemyProjectile_FallingSpark_HitFloor</c> at $86:F38F.
    /// </summary>
    internal const ushort HitFloorTerminalDelete = 0xf38f;

    private static readonly FallingSparkInstructionMechanicsWord[] Words =
    [
        new(0xf353, 0x0003),
        new(0xf357, 0x0003),
        new(0xf35b, 0x0003),
        new(0xf35f, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xf361, Falling),
        new(0xf363, 0x0001),
        new(0xf367, 0x0001),
        new(0xf36b, 0x0001),
        new(0xf36f, 0x0001),
        new(0xf373, 0x0001),
        new(0xf377, 0x0001),
        new(0xf37b, 0x0001),
        new(0xf37f, 0x0001),
        new(0xf383, 0x0001),
        new(0xf387, 0x0001),
        new(0xf38b, 0x0001),
        new(HitFloorTerminalDelete,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xf355,
        0xf359,
        0xf35d,
        0xf365,
        0xf369,
        0xf36d,
        0xf371,
        0xf375,
        0xf379,
        0xf37d,
        0xf381,
        0xf385,
        0xf389,
        0xf38d,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static FallingSparkInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            FallingSparkInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Falling Spark instruction mechanics pointer $86:{address:X4} is not compiled.");
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
