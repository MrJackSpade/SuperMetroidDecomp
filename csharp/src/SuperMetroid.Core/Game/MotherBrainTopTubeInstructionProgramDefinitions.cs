namespace SuperMetroid.Core.Game;

/// <summary>One compiled Mother Brain ceiling-tube mechanics word at its bank-$86 address.</summary>
internal readonly record struct MotherBrainTopTubeInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the four falling ceiling-tube poses in Mother Brain's fake-death
/// sequence. Their four spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MotherBrainTopTubeInstructionProgramDefinitions
{
    /// <summary>Top-right ceiling tube instruction list at $86:CC43.</summary>
    internal const ushort TopRight = 0xcc43;

    /// <summary>Top-left ceiling tube instruction list at $86:CC49.</summary>
    internal const ushort TopLeft = 0xcc49;

    /// <summary>Top-middle-left ceiling tube instruction list at $86:CC4F.</summary>
    internal const ushort TopMiddleLeft = 0xcc4f;

    /// <summary>Top-middle-right ceiling tube instruction list at $86:CC55.</summary>
    internal const ushort TopMiddleRight = 0xcc55;

    private static readonly MotherBrainTopTubeInstructionMechanicsWord[] Words =
    [
        new(TopRight, 0x0001),
        new(0xcc47, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(TopLeft, 0x0001),
        new(0xcc4d, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(TopMiddleLeft, 0x0001),
        new(0xcc53, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(TopMiddleRight, 0x0001),
        new(0xcc59, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
    ];

    private static readonly ushort[] PresentationWords = [0xcc45, 0xcc4b, 0xcc51, 0xcc57];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MotherBrainTopTubeInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainTopRightTube or
        RoomEnemyProjectileKind.MotherBrainTopLeftTube or
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube or
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MotherBrainTopTubeInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Mother Brain ceiling-tube mechanics pointer $86:{address:X4} is not compiled.");
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
