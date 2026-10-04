namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct SbugInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>Compiled mechanics words from Sbug's eight directional animation loops.</summary>
/// <remarks>
/// Each list's frame durations and terminal goto are immutable simulation control. The
/// interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal static class SbugInstructionProgramDefinitions
{
    /// <summary><c>$A3:A071</c>, right-facing animation loop.</summary>
    internal const ushort Right = 0xa071;

    /// <summary><c>$A3:A085</c>, up-right-facing animation loop.</summary>
    internal const ushort UpRight = 0xa085;

    /// <summary><c>$A3:A099</c>, up-facing animation loop.</summary>
    internal const ushort Up = 0xa099;

    /// <summary><c>$A3:A0AD</c>, up-left-facing animation loop.</summary>
    internal const ushort UpLeft = 0xa0ad;

    /// <summary><c>$A3:A0C1</c>, left-facing animation loop.</summary>
    internal const ushort Left = 0xa0c1;

    /// <summary><c>$A3:A0D5</c>, down-left-facing animation loop.</summary>
    internal const ushort DownLeft = 0xa0d5;

    /// <summary><c>$A3:A0E9</c>, down-facing animation loop.</summary>
    internal const ushort Down = 0xa0e9;

    /// <summary><c>$A3:A0FD</c>, down-right-facing animation loop.</summary>
    internal const ushort DownRight = 0xa0fd;

    internal static int MechanicsWordCount => 48;
    internal static int PresentationWordCount => 32;

    /// <summary>Eight directional programs each display four five-tick frames and branch back to their entry.</summary>
    internal static SbugInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort start = (ushort)(Right + 20 * (index / 6));
        int word = index % 6;
        return word < 4 ? new((ushort)(start + 4 * word), 5)
            : new((ushort)(start + 16 + 2 * (word - 4)), word == 4 ? CommonEnemyInstructionCodes.Goto : start);
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Right + 20 * (index / 4) + 2 + 4 * (index % 4));
    }
    /// <summary>Returns one fixed control word or rejects pointers outside all eight loops.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SbugInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Sbug instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
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
