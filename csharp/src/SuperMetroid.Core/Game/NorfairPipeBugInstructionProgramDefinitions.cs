namespace SuperMetroid.Core.Game;

/// <summary>Compiled control words for Norfair Pipe Bug rising and flight loops.</summary>
internal abstract class NorfairPipeBugInstructionProgramDefinitions
{
    /// <summary><c>$B3:8AE1</c>, rising while facing left.</summary>
    internal const ushort RisingLeft = 0x8ae1;
    /// <summary><c>$B3:8B05</c>, horizontal flight facing left.</summary>
    internal const ushort FlyingLeft = 0x8b05;
    /// <summary><c>$B3:8B21</c>, rising while facing right.</summary>
    internal const ushort RisingRight = 0x8b21;
    /// <summary><c>$B3:8B45</c>, horizontal flight facing right.</summary>
    internal const ushort FlyingRight = 0x8b45;

    public static int MechanicsWordCount => 36;

    /// <summary>
    /// $B3:8AE1-$8B60 contains two facing halves. Each half has eight two-tick rising
    /// frames and six one-tick flying frames, with a goto pair after each loop.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int facing = index / 18;
        int word = index % 18;
        bool flying = word >= 10;
        if (flying)
            word -= 10;
        int frames = flying ? 6 : 8;
        ushort start = (ushort)((flying ? FlyingLeft : RisingLeft) + 64 * facing);
        if (word < frames)
            return new((ushort)(start + 4 * word), (ushort)(flying ? 1 : 2));
        return new((ushort)(start + 4 * frames + 2 * (word - frames)),
            word == frames ? (ushort)CommonEnemyInstruction.Goto : start);
    }
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
            $"Norfair Pipe Bug instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }
}
