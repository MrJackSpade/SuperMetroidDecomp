namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One corpse rot table (#629, #1406): entries of a signed pixel-row word and a delay word, consumed by
/// <c>ProcessCorpseRotting</c> at <c>$A9:DB12</c>.
/// </summary>
public sealed class CorpseRotTable
{
    /// <summary>The heap bytes one (signed Y, delay) entry occupies, which place tables in the rotting heap.</summary>
    public const int EntryByteCount = 4;

    /// <summary>Each entry's signed pixel-row word.</summary>
    private readonly ushort[] yOffsets;

    /// <summary>Each entry's delay word.</summary>
    private readonly ushort[] delays;

    /// <summary>Creates a table of <paramref name="entryCount"/> entries, all zero until initialized.</summary>
    internal CorpseRotTable(int entryCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
        yOffsets = new ushort[entryCount];
        delays = new ushort[entryCount];
    }

    /// <summary>
    /// The number of entries processed in ascending index order: the corpse's height in pixel rows, an authored
    /// count that is always positive and never changes after the table is located.
    /// </summary>
    public int EntryCount => yOffsets.Length;

    /// <summary>Entry <paramref name="entry"/>'s signed pixel-row word.</summary>
    public ushort YOffset(int entry) => yOffsets[Checked(entry)];

    /// <summary>Stores entry <paramref name="entry"/>'s signed pixel-row word.</summary>
    public void SetYOffset(int entry, ushort value) => yOffsets[Checked(entry)] = value;

    /// <summary>Entry <paramref name="entry"/>'s delay word.</summary>
    public ushort Delay(int entry) => delays[Checked(entry)];

    /// <summary>Stores entry <paramref name="entry"/>'s delay word.</summary>
    public void SetDelay(int entry, ushort value) => delays[Checked(entry)] = value;

    /// <summary>Bounds-checks an entry index.</summary>
    private int Checked(int entry) => (uint)entry < (uint)EntryCount ? entry
        : throw new ArgumentOutOfRangeException(nameof(entry), entry, $"Rot table holds {EntryCount} entries.");
}
