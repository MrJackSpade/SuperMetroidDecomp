namespace SuperMetroid.Core.Game;

/// <summary>
/// One native corpse rot table: four-byte entries of a signed pixel-row word followed by a
/// delay word, consumed by <c>ProcessCorpseRotting</c> at <c>$A9:DB12</c> (#629).
/// </summary>
public readonly struct CorpseRotTable
{
    /// <summary>Bytes in one (signed Y, delay) entry.</summary>
    public const int EntryByteCount = 4;

    /// <summary>Byte-addressed view of the shared corpse-rotting heap.</summary>
    private readonly WorkRamByteTable bytes;
    /// <summary>Start of this enemy's rot entries within the heap view.</summary>
    private readonly int firstByte;

    /// <summary>Creates a nonempty table view at the caller's validated heap offset.</summary>
    internal CorpseRotTable(WorkRamByteTable bytes, int firstByte, ushort entryCount)
    {
        if (entryCount == 0)
            throw new ArgumentOutOfRangeException(nameof(entryCount));
        this.bytes = bytes;
        this.firstByte = firstByte;
        EntryCount = entryCount;
    }

    /// <summary>The number of entries processed in ascending index order.</summary>
    public ushort EntryCount { get; }

    /// <summary>Entry <paramref name="entry"/>'s signed pixel-row word.</summary>
    public ushort YOffset(int entry) => bytes.ReadWord(EntryOffset(entry));

    /// <summary>Stores entry <paramref name="entry"/>'s signed pixel-row word, low byte first.</summary>
    public void SetYOffset(int entry, ushort value) => bytes.WriteWord(EntryOffset(entry), value);

    /// <summary>Entry <paramref name="entry"/>'s delay word.</summary>
    public ushort Delay(int entry) => bytes.ReadWord(EntryOffset(entry) + 2);

    /// <summary>Stores entry <paramref name="entry"/>'s delay word, low byte first.</summary>
    public void SetDelay(int entry, ushort value) => bytes.WriteWord(EntryOffset(entry) + 2, value);

    /// <summary>Bounds-checks an entry and locates its four-byte row-and-delay pair.</summary>
    private int EntryOffset(int entry)
    {
        if ((uint)entry >= EntryCount)
            throw new ArgumentOutOfRangeException(nameof(entry), entry, $"Rot table holds {EntryCount} entries.");
        return firstByte + entry * EntryByteCount;
    }
}
