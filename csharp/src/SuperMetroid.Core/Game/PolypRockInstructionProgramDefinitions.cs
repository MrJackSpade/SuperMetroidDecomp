namespace SuperMetroid.Core.Game;

/// <summary>One compiled Polyp-rock mechanics word at its bank-$86 address.</summary>
internal readonly record struct PolypRockInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Polyp's single-frame lava-rock animation.
/// Its sprite operand selects installed presentation artwork.
/// </summary>
internal static class PolypRockInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_NorfairLavaquakeRocks</c> at $86:BBD5.</summary>
    internal const ushort Initial = 0xbbd5;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> holding the lava-rock frame at $86:BBD9.
    /// </summary>
    internal const ushort Sleep = 0xbbd9;

    /// <summary>Spritemap operand at $86:BBD7.</summary>
    internal const ushort PresentationWord = 0xbbd7;

    /// <summary>EnemyProjSpritemaps_LavaquakeRocks at $8D:9340, selected by $86:BBD7.</summary>
    internal const ushort Spritemap = 0x9340;

    internal static ushort FrameAt(ushort operandAddress) => operandAddress == PresentationWord
        ? Spritemap
        : throw new InvalidDataException($"Unknown Polyp-rock visual operand $86:{operandAddress:X4}.");

    internal static int MechanicsWordCount => 2;
    /// <summary>$86:BBD5-BBD9 installs one static rock pose then sleeps.</summary>
    internal static PolypRockInstructionMechanicsWord MechanicsWord(int index) => index switch
    {
        0 => new(Initial, 1),
        1 => new(Sleep, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        _ => throw new IndexOutOfRangeException(),
    };

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PolypRockInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Polyp-rock instruction mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
