using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where on the cartridge enemies read past their authored data: extraction's private knowledge, which it decodes once
/// into the enemy overread services (#1500).
/// </summary>
[EncodingOwner("cartridge regions enemies read past their authored data")]
internal static class EnemyOverreadSourceDefinitions
{
    /// <summary>
    /// $A8:AF55, Magdollite's nine-word arm-height threshold table. The maximum-rise branch at $A8:B259 reads the word at
    /// this table plus the arm's slot offset.
    /// </summary>
    internal const int MagdolliteArmHeightThresholds = 0xa8af55;

    /// <summary>The bytes between consecutive enemy slots' records, which the slot offset counts in.</summary>
    internal const int EnemyRecordBytes = 0x40;

    /// <summary>
    /// $86:F3D4, the falling spark's launch table: seven four-byte records of a whole and a fractional drift word, which
    /// random bits two through four select.
    /// </summary>
    internal const int FallingSparkLaunchTable = 0x86f3d4;

    /// <summary>The bytes of one falling-spark launch record: its whole word, then its fractional word.</summary>
    internal const int FallingSparkLaunchBytes = 4;
}
