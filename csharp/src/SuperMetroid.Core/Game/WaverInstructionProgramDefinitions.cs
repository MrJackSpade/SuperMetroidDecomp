namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Waver's steady and spinning programs.</summary>
/// <remarks>
/// Durations, spin completion, and terminal sleeps are immutable simulation data. The
/// ten interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal abstract class WaverInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>$A3:86A7</c>, steady animation facing left.</summary>
    internal const ushort SteadyFacingLeft = 0x86a7;

    /// <summary><c>$A3:86AD</c>, steady animation facing right.</summary>
    internal const ushort SteadyFacingRight = 0x86ad;

    /// <summary><c>$A3:86B3</c>, four-frame spin facing left.</summary>
    internal const ushort SpinningFacingLeft = 0x86b3;

    /// <summary><c>$A3:86C7</c>, four-frame spin facing right.</summary>
    internal const ushort SpinningFacingRight = 0x86c7;

    public static int MechanicsWordCount => 16;
    public static int PresentationWordCount => 10;

    /// <summary>Two steady frame/sleep programs followed by two four-frame spin/completion/sleep programs.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 4)
        {
            int local = index % 2;
            return new((ushort)(SteadyFacingLeft + 6 * (index / 2) + 4 * local),
                local == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
        }
        int spin = (index - 4) / 6;
        int word = (index - 4) % 6;
        ushort start = (ushort)(SpinningFacingLeft + 20 * spin);
        return word switch
        {
            < 4 => new((ushort)(start + 4 * word), 8),
            4 => new((ushort)(start + 16), EnemyInstructionCodePointers.Instruction_Waver_SetSpinFinishedFlag),
            _ => new((ushort)(start + 18), CommonEnemyInstructionCodes.Sleep),
        };
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return index < 2 ? (ushort)(SteadyFacingLeft + 6 * index + 2)
            : (ushort)(SpinningFacingLeft + 20 * ((index - 2) / 4) + 2 + 4 * ((index - 2) % 4));
    }
    /// <summary>Returns fixed Waver control or rejects pointers outside all four programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Waver instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
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
