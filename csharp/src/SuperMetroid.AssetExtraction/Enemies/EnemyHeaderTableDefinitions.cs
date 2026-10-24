namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Where the bank-$B4 tables an enemy header's words select begin.</summary>
internal static class EnemyHeaderTableDefinitions
{
    /// <summary><c>EnemyDropChances</c> at $B4:F1F4, the first six-byte item-drop table.</summary>
    internal const ushort FirstDropChances = 0xf1f4;

    /// <summary><c>EnemyVulnerabilities_Default</c> at $B4:EC1C, the first 22-byte vulnerability table.</summary>
    internal const ushort FirstVulnerability = 0xec1c;
}
