namespace SuperMetroid.Core.Game;

/// <summary>One compiled Zero mechanics word at its native bank-$A3 address.</summary>
internal readonly record struct ZeroInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Zero's four production-selected surface loops. The
/// twenty-four spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal static class ZeroInstructionProgramDefinitions
{
    /// <summary><c>InstList_Zero_UpsideRight_FacingDown_0</c> at $A3:984B.</summary>
    internal const ushort UpsideRight = 0x984b;
    /// <summary><c>InstList_Zero_UpsideLeft_FacingUp_0</c> at $A3:988B.</summary>
    internal const ushort UpsideLeft = 0x988b;
    /// <summary><c>InstList_Zero_UpsideDown_FacingLeft_0</c> at $A3:98AB.</summary>
    internal const ushort UpsideDown = 0x98ab;
    /// <summary><c>UNUSED_InstList_Zero_UpsideUp_FacingRight_A3990B</c> at $A3:990B.</summary>
    internal const ushort UpsideUp = 0x990b;
    /// <summary>The retail-unused alternate upside-right program at $A3:982B.</summary>
    internal const ushort UnusedAlternateUpsideRight = 0x982b;

    internal static int MechanicsWordCount => 40;
    internal static int PresentationWordCount => 24;

    private static ushort Entry(CrawlerSurfaceOrientation surface) => surface switch
    {
        CrawlerSurfaceOrientation.UpsideRight => UpsideRight,
        CrawlerSurfaceOrientation.UpsideLeft => UpsideLeft,
        CrawlerSurfaceOrientation.UpsideDown => UpsideDown,
        CrawlerSurfaceOrientation.UpsideUp => UpsideUp,
        _ => throw new ArgumentOutOfRangeException(nameof(surface)),
    };

    /// <summary>Each selected surface sets its axis, displays six four-tick frames and loops without repeating setup.</summary>
    internal static ZeroInstructionMechanicsWord MechanicsWord(int index)
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

    internal static ushort PresentationWordAddress(int index)
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
            ZeroInstructionMechanicsWord candidate = MechanicsWord(middle);
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
