namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Calculated geometry and bounded native overreads for Kraid's fingernail contour.</summary>
/// <remarks>
/// The contour has four ledges; a nail below all of them reads two more records from the code that follows the table
/// ($A7:BF2D..BF34). Those records are gameplay: they decide where a low nail is pushed. Their values are stated here
/// as the offsets and bounds the contour reads (#1413); the ROM oracle confirms they are the cartridge's words.
/// </remarks>
internal static class KraidNailContourDefinitions
{
    /// <summary>The contour's records: four ledges followed by the two the contour reads past them.</summary>
    internal const int RecordCount = 6;
    /// <summary>The fifth record's left offset, read past the ledges.</summary>
    private const short FirstOverreadLeft = 1312;
    /// <summary>The fifth record's upper bound, read past the ledges.</summary>
    private const short FirstOverreadTop = -21056;
    /// <summary>The sixth record's left offset, read past the ledges.</summary>
    private const short SecondOverreadLeft = 4050;
    /// <summary>The sixth record's upper bound, read past the ledges.</summary>
    private const short SecondOverreadTop = 14281;

    /// <summary>Returns a left offset for one of the six reachable contour records.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against pinned bank_A7.asm and original NTSC
    /// words. The four geometry ledges advance 24 pixels from -64 to8. The next two
    /// records are the overreads past the ledges. Preserve their unsigned words.
    /// </remarks>
    internal static ushort LeftAt(int record)
    {
        if ((uint)record >= RecordCount) throw new ArgumentOutOfRangeException(nameof(record));
        if (record < 4) return unchecked((ushort)(-64 + 24 * record));
        return unchecked((ushort)(record == 4 ? FirstOverreadLeft : SecondOverreadLeft));
    }

    /// <summary>Returns the comparison boundary paired with one reachable left offset.</summary>
    /// <remarks>
    /// The first three ledges are 56 pixels apart, starting at16. The final authored
    /// head boundary is -128. The two overread bounds follow; they are bounded overreads,
    /// not extrapolated contour geometry.
    /// </remarks>
    internal static ushort TopAt(int record)
    {
        if ((uint)record >= RecordCount) throw new ArgumentOutOfRangeException(nameof(record));
        if (record < 3) return unchecked((ushort)(16 - 56 * record));
        return record switch
        {
            3 => unchecked((ushort)-128),
            4 => unchecked((ushort)FirstOverreadTop),
            _ => unchecked((ushort)SecondOverreadTop),
        };
    }

    /// <summary>Returns the native selected left offset for a wrapped nail-minus-body Y word.</summary>
    public static ushort LeftOffset(ushort relativeY)
    {
        for (int record = 0; record < RecordCount; record++)
            if (unchecked((short)(TopAt(record) - relativeY)) < 0)
                return LeftAt(record);
        throw new InvalidOperationException("Kraid contour definitions failed their exhaustive signed-word coverage invariant.");
    }
}
