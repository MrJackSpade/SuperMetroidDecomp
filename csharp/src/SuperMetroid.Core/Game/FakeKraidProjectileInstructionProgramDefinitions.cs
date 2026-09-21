namespace SuperMetroid.Core.Game;

/// <summary>One compiled Fake Kraid projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct FakeKraidProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Fake Kraid's spit and left/right spike projectile poses.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class FakeKraidProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpit</c> at $86:9DDA.</summary>
    internal const ushort Spit = 0x9dda;

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpikes_Left</c> at $86:9DE0.</summary>
    internal const ushort SpikeLeft = 0x9de0;

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpikes_Right</c> at $86:9DE6.</summary>
    internal const ushort SpikeRight = 0x9de6;

    /// <summary><c>Instruction_EnemyProjectile_Sleep</c> in the spit list at $86:9DDE.</summary>
    internal const ushort SpitSleep = 0x9dde;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> in the left-spike list at $86:9DE4.
    /// </summary>
    internal const ushort SpikeLeftSleep = 0x9de4;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> in the right-spike list at $86:9DEA.
    /// </summary>
    internal const ushort SpikeRightSleep = 0x9dea;

    private static readonly FakeKraidProjectileInstructionMechanicsWord[] Words =
    [
        new(Spit, 0x7fff),
        new(SpitSleep, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(SpikeLeft, 0x7fff),
        new(SpikeLeftSleep, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(SpikeRight, 0x7fff),
        new(SpikeRightSleep, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
    ];

    private static readonly ushort[] PresentationWords = [0x9ddc, 0x9de2, 0x9de8];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static FakeKraidProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.FakeKraidSpit or
        RoomEnemyProjectileKind.FakeKraidSpikeLeft or
        RoomEnemyProjectileKind.FakeKraidSpikeRight;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            FakeKraidProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Fake Kraid projectile instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
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
