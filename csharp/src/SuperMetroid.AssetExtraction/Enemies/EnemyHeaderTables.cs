using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Decodes an enemy header's bank-$B4 table words to the drop and vulnerability tables they select.</summary>
internal static class EnemyHeaderTables
{
    /// <summary><c>EnemyDropChances</c> at $B4:F1F4, the first six-byte item-drop table.</summary>
    private const ushort FirstDropChances = 0xf1f4;

    /// <summary><c>EnemyVulnerabilities_Default</c> at $B4:EC1C, the first 22-byte vulnerability table.</summary>
    private const ushort FirstVulnerability = 0xec1c;

    /// <summary>The item-drop table a header word selects, or null for a zero word, which drops nothing.</summary>
    internal static EnemyDropTable? DropChances(ushort word) => word == 0
        ? null
        : new EnemyDropTable(Record(word, FirstDropChances, EnemyDropChanceDefinitions.RecordSize,
            EnemyDropChanceDefinitions.RecordCount, "item-drop"));

    /// <summary>The vulnerability table a header word selects; the engine reads a zero word as the default table.</summary>
    internal static EnemyVulnerabilityTable Vulnerability(ushort word) => new(word == 0
        ? 0
        : Record(word, FirstVulnerability, EnemyVulnerabilityDefinitions.RecordSize,
            EnemyVulnerabilityDefinitions.RecordCount, "vulnerability"));

    /// <summary>The position of the table at <paramref name="word"/> among <paramref name="count"/> tables.</summary>
    private static int Record(ushort word, ushort first, int size, int count, string kind)
    {
        int offset = word - first;
        if (offset < 0 || offset % size != 0 || offset / size >= count)
            throw new InvalidDataException($"Enemy {kind} word ${word:X4} is not one of the {count} aligned tables.");
        return offset / size;
    }
}
