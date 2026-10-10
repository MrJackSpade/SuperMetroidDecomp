namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Waver's steady and spinning programs.</summary>
/// <remarks>
/// Durations, spin completion, and terminal sleeps are immutable simulation data. The
/// ten interleaved spritemap pointers remain live cartridge presentation data.
/// </remarks>
internal abstract class WaverInstructionProgramDefinitions
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

    /// <summary>Two steady frame/sleep programs followed by two four-frame spin/completion/sleep programs.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 4)
        {
            int local = index % 2;
            return new((ushort)(SteadyFacingLeft + 6 * (index / 2) + 4 * local),
                local == 0 ? (ushort)1 : (ushort)CommonEnemyInstruction.Sleep);
        }
        int spin = (index - 4) / 6;
        int word = (index - 4) % 6;
        ushort start = (ushort)(SpinningFacingLeft + 20 * spin);
        return word switch
        {
            < 4 => new((ushort)(start + 4 * word), 8),
            4 => new((ushort)(start + 16), (ushort)WaverInstruction.SetSpinFinishedFlag),
            _ => new((ushort)(start + 18), (ushort)CommonEnemyInstruction.Sleep),
        };
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
}
