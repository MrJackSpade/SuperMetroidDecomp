namespace SuperMetroid.Core.Game;

/// <summary>One compiled save-station electricity mechanics word at its bank-$86 address.</summary>
internal readonly record struct SaveStationElectricityInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the save station's twenty-cycle electricity animation. The eight
/// spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class SaveStationElectricityInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_SaveStationElectricity_0</c> at $86:E683.</summary>
    internal const ushort Initial = 0xe683;

    /// <summary><c>InstList_EnemyProjectile_SaveStationElectricity_1</c> at $86:E687.</summary>
    internal const ushort Loop = 0xe687;

    private static readonly SaveStationElectricityInstructionMechanicsWord[] Words =
    [
        new(Initial, EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY),
        new(0xe685, 0x0014),
        new(Loop, 0x0001),
        new(0xe68b, 0x0001),
        new(0xe68f, 0x0001),
        new(0xe693, 0x0001),
        new(0xe697, 0x0001),
        new(0xe69b, 0x0001),
        new(0xe69f, 0x0001),
        new(0xe6a3, 0x0001),
        new(0xe6a7,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
        new(0xe6a9, Loop),
        new(0xe6ab, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
        [0xe689, 0xe68d, 0xe691, 0xe695, 0xe699, 0xe69d, 0xe6a1, 0xe6a5];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static SaveStationElectricityInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SaveStationElectricityInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Save-station electricity mechanics pointer $86:{address:X4} is not compiled.");
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
