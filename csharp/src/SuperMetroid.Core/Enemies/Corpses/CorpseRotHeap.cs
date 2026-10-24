namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The rot tables of the rotting heap (#1406), by the heap byte each starts at. A corpse asking for the table at the same
/// place and length as an earlier one shares it, as corpses sharing heap bytes do on the cartridge; one placed over part of
/// earlier tables replaces them, whose corpses are gone, since every corpse initializes its table before rotting.
/// </summary>
internal sealed class CorpseRotHeap(int byteCount)
{
    /// <summary>The live tables by the heap byte each starts at.</summary>
    private readonly SortedDictionary<int, CorpseRotTable> tables = [];

    /// <summary>The table of <paramref name="entryCount"/> entries <paramref name="firstByte"/> bytes into the heap.</summary>
    /// <exception cref="InvalidDataException">The table leaves the heap.</exception>
    internal CorpseRotTable Table(int firstByte, int entryCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryCount);
        int end = firstByte + entryCount * CorpseRotTable.EntryByteCount;
        if (firstByte < 0 || end > byteCount)
            throw new InvalidDataException($"Corpse rot table at heap byte {firstByte} with {entryCount} entries lies outside the rotting heap.");
        if (tables.TryGetValue(firstByte, out CorpseRotTable? existing) && existing.EntryCount == entryCount)
            return existing;
        foreach (int start in tables.Where(entry => entry.Key < end && firstByte < entry.Key + entry.Value.EntryCount * CorpseRotTable.EntryByteCount)
                     .Select(entry => entry.Key).ToArray())
            tables.Remove(start);
        var table = new CorpseRotTable(entryCount);
        tables.Add(firstByte, table);
        return table;
    }
}
