namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Zero's four production-selected surface loops. The
/// twenty-four spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class ZeroInstructionProgramDefinitions
{
    /// <summary><c>InstList_Zero_UpsideRight_FacingDown_0</c> at $A3:984B.</summary>
    internal const ushort UpsideRight = 0x984b;
    /// <summary><c>InstList_Zero_UpsideLeft_FacingUp_0</c> at $A3:988B.</summary>
    internal const ushort UpsideLeft = 0x988b;
    /// <summary><c>InstList_Zero_UpsideDown_FacingLeft_0</c> at $A3:98AB.</summary>
    internal const ushort UpsideDown = 0x98ab;
    /// <summary><c>UNUSED_InstList_Zero_UpsideUp_FacingRight_A3990B</c> at $A3:990B.</summary>
    internal const ushort UpsideUp = 0x990b;

    public static int MechanicsWordCount => 40;
    public static int PresentationWordCount => 24;

    private static ushort Entry(CrawlerSurfaceOrientation surface) => surface switch
    {
        CrawlerSurfaceOrientation.UpsideRight => UpsideRight,
        CrawlerSurfaceOrientation.UpsideLeft => UpsideLeft,
        CrawlerSurfaceOrientation.UpsideDown => UpsideDown,
        CrawlerSurfaceOrientation.UpsideUp => UpsideUp,
        _ => throw new ArgumentOutOfRangeException(nameof(surface)),
    };

    /// <summary>Each selected surface sets its axis, displays six four-tick frames and loops without repeating setup.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int surface = index / 10;
        int word = index % 10;
        ushort start = Entry((CrawlerSurfaceOrientation)surface);
        return word switch
        {
            0 => new(start, EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY),
            1 => new((ushort)(start + 2), (ushort)(surface < 2
                ? CrawlerEnemyFunction.CrawlingVertically : CrawlerEnemyFunction.CrawlingHorizontally)),
            < 8 => new((ushort)(start + 4 + 4 * (word - 2)), 4),
            8 => new((ushort)(start + 28), CommonEnemyInstructionCodes.Goto),
            _ => new((ushort)(start + 30), (ushort)(start + 4)),
        };
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Entry((CrawlerSurfaceOrientation)(index / 6)) + 6 + 4 * (index % 6));
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
            $"Zero instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
