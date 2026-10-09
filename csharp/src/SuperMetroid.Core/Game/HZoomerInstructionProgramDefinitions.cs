namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Wrecked Ship orange Zoomer's four surface loops.
/// The twenty interleaved spritemap operands select installed presentation art.
/// </summary>
internal abstract class HZoomerInstructionProgramDefinitions
{
    /// <summary><c>InstList_HZoomer_UpsideRight_0</c> at $A3:DFCB.</summary>
    internal const ushort UpsideRight = 0xdfcb;
    /// <summary><c>InstList_HZoomer_UpsideLeft_0</c> at $A3:DFE7.</summary>
    internal const ushort UpsideLeft = 0xdfe7;
    /// <summary><c>InstList_HZoomer_UpsideDown_0</c> at $A3:E003.</summary>
    internal const ushort UpsideDown = 0xe003;
    /// <summary><c>InstList_HZoomer_UpsideUp_0</c> at $A3:E01F.</summary>
    internal const ushort UpsideUp = 0xe01f;

    /// <summary>Number of mechanics words across the four compiled surface loops.</summary>
    public static int MechanicsWordCount => 36;

    /// <summary>Each surface initializes its axis, shows five three-tick frames, then loops to the first frame.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int surface = index / 9;
        int word = index % 9;
        ushort start = (ushort)(UpsideRight + 28 * surface);
        return word switch
        {
            0 => new(start, EnemyInstructionCodePointers.Instruction_HZoomer_FunctionInY),
            1 => new((ushort)(start + 2), (ushort)(surface < 2
                ? CrawlerEnemyFunction.HZoomerCrawlingVertically : CrawlerEnemyFunction.HZoomerCrawlingHorizontally)),
            < 7 => new((ushort)(start + 4 + 4 * (word - 2)), 3),
            7 => new((ushort)(start + 24), CommonEnemyInstructionCodes.Goto),
            _ => new((ushort)(start + 26), (ushort)(start + 4)),
        };
    }

    /// <summary>Determines whether a bank-relative address selects one of the interleaved live spritemap operands.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - UpsideRight;
        int local = offset % 28;
        return offset >= 0 && offset < 4 * 28 && local >= 6 && local <= 22 && (local - 6) % 4 == 0;
    }

    /// <summary>Looks up a compiled duration or control word by its bank-relative address.</summary>
    /// <param name="address">The instruction address to resolve within one of the four surface loops.</param>
    /// <returns>The mechanics word stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled mechanics-word position.</exception>
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
            $"HZoomer instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
