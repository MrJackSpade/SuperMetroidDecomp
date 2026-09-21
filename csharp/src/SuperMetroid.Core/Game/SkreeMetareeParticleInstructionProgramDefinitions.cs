namespace SuperMetroid.Core.Game;

/// <summary>One compiled Skree/Metaree debris mechanics word at its bank-$86 address.</summary>
internal readonly record struct SkreeMetareeParticleInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the visually distinct Skree and Metaree death-particle programs.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class SkreeMetareeParticleInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_MetalSkreeParticle</c> at $86:8ABD.</summary>
    internal const ushort Skree = 0x8abd;

    /// <summary><c>InstList_EnemyProjectile_MetareeParticle</c> at $86:8AC5.</summary>
    internal const ushort Metaree = 0x8ac5;

    private static readonly SkreeMetareeParticleInstructionMechanicsWord[] Words =
    [
        new(Skree, 0x0010),
        new(0x8ac1, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8ac3, Skree),
        new(Metaree, 0x0010),
        new(0x8ac9, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8acb, Metaree),
    ];

    private static readonly ushort[] PresentationWords = [0x8abf, 0x8ac7];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SkreeMetareeParticleInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.SkreeParticleDownRight or
        RoomEnemyProjectileKind.SkreeParticleUpRight or
        RoomEnemyProjectileKind.SkreeParticleDownLeft or
        RoomEnemyProjectileKind.SkreeParticleUpLeft or
        RoomEnemyProjectileKind.MetareeParticleDownRight or
        RoomEnemyProjectileKind.MetareeParticleUpRight or
        RoomEnemyProjectileKind.MetareeParticleDownLeft or
        RoomEnemyProjectileKind.MetareeParticleUpLeft;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SkreeMetareeParticleInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Skree/Metaree particle instruction mechanics pointer $86:{address:X4} " +
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
