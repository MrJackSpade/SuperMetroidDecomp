using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Decodes an enemy header's bank-$B4 table words to the drop and vulnerability tables they select.</summary>
internal static class EnemyHeaderTables
{
    /// <summary>The item-drop table a header word selects, or null for a zero word, which drops nothing.</summary>
    internal static EnemyDropTable? DropChances(int word) => word == 0
        ? null
        : new EnemyDropTable(Record(word, EnemyHeaderTableDefinitions.FirstDropChances, EnemyDropChanceDefinitions.RecordSize,
            EnemyDropChanceDefinitions.RecordCount, "item-drop"));

    /// <summary>The vulnerability table a header word selects; the engine reads a zero word as the default table.</summary>
    internal static EnemyVulnerabilityTable Vulnerability(int word) => new(word == 0
        ? 0
        : Record(word, EnemyHeaderTableDefinitions.FirstVulnerability, EnemyVulnerabilityDefinitions.RecordSize,
            EnemyVulnerabilityDefinitions.RecordCount, "vulnerability"));

    /// <summary>The position of the table at <paramref name="word"/> among <paramref name="count"/> tables.</summary>
    private static int Record(int word, int first, int size, int count, string kind)
    {
        int offset = word - first;
        if (offset < 0 || offset % size != 0 || offset / size >= count)
            throw new InvalidDataException($"Enemy {kind} word ${word:X4} is not one of the {count} aligned tables.");
        return offset / size;
    }
}
