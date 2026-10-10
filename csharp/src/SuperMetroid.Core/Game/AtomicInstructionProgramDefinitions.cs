namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Atomic's four directional animation loops.</summary>
/// <remarks>
/// The six durations and terminal goto in each list are immutable simulation control. The
/// interleaved visual selectors are compiled in
/// <see cref="Assets.EnemySpritemapDefinitions"/>.
/// </remarks>
internal abstract class AtomicInstructionProgramDefinitions
{
    /// <summary><c>$A8:E310</c>, spinning up-right.</summary>
    internal const ushort UpRight = 0xe310;

    /// <summary><c>$A8:E32C</c>, spinning up-left.</summary>
    internal const ushort UpLeft = 0xe32c;

    /// <summary><c>$A8:E348</c>, spinning down-left.</summary>
    internal const ushort DownLeft = 0xe348;

    /// <summary><c>$A8:E364</c>, spinning down-right.</summary>
    internal const ushort DownRight = 0xe364;

    /// <summary>Identifies an interleaved presentation selector in one of Atomic's four animation loops.</summary>
    /// <param name="address">Bank-$A8 address to classify.</param>
    /// <returns><see langword="true"/> for a visual operand, excluding each loop's timing and control words.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (UpRight + 2);
        return (uint)offset < 4 * 28 && offset % 28 <= 20 && offset % 4 == 0;
    }

    /// <summary>Evaluates the six eight-frame durations, goto and loop target at
    /// $A8:E310..E37F. Interleaved visual operands remain outside this decoder.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - UpRight;
        if ((uint)offset < 4 * 28 && (offset & 1) == 0)
        {
            int stage = offset % 28;
            if (stage < 24 && stage % 4 == 0) return 8;
            if (stage == 24) return CommonEnemyInstructionCodes.Goto;
            if (stage == 26) return (ushort)(address - 26);
        }
        throw new InvalidDataException(
            $"Atomic instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }
}
