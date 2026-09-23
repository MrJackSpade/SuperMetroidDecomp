namespace SuperMetroid.Core.Game;

/// <summary>One compiled Ceres falling-debris mechanics word at its bank-$86 address.</summary>
internal readonly record struct CeresFallingDebrisInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the light and dark Ceres falling-debris poses. Their spritemap
/// operands remain live cartridge presentation data.
/// </summary>
internal static class CeresFallingDebrisInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_CeresFallingTile_Light</c> at $86:9750,
    /// selected by definition $9734. Its pinned-ROM mechanics are one tick
    /// at $9750, then terminal $8159 sleep at $9754. The production interpreter
    /// reaches only those two private pointers before a separate shot reaction
    /// can enter the shared delete program. The $9752 spritemap is presentation.
    /// </summary>
    internal const ushort Light = 0x9750;

    /// <summary><c>InstList_EnemyProjectile_CeresFallingTile_Dark</c> at $86:9756.</summary>
    internal const ushort Dark = 0x9756;

    private static readonly CeresFallingDebrisInstructionMechanicsWord[] Words =
    [
        new(Light, 0x0001),
        new(0x9754, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(Dark, 0x0001),
        new(0x975a, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
    ];

    /// <summary>
    /// Live bank-$86 spritemap operands. The light program's sole operand at
    /// $9752 is $8ABF in the pinned NTSC J/U v1.0 ROM. Its bank-$8D target
    /// is a one-component authored pose with packed attribute $20EA. A
    /// one-pose pointer has no useful indexed algorithm; retain its identity
    /// and the live cartridge presentation read.
    /// </summary>
    private static readonly ushort[] PresentationWords = [0x9752, 0x9758];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CeresFallingDebrisInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.CeresFallingDebrisLight or
        RoomEnemyProjectileKind.CeresFallingDebrisDark;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CeresFallingDebrisInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ceres falling-debris mechanics pointer $86:{address:X4} is not compiled.");
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
