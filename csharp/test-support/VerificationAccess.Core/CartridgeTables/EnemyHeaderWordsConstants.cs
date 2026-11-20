namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge values and helpers of <see cref="EnemyHeaderWords"/> that only verification reads.</summary>
internal static class EnemyHeaderWordsConstants
{
    /// <summary>
    /// The header word the cartridge leaves in a killed enemy's place while it waits to respawn (#1851): the port holds
    /// that place as awaiting respawn, with no enemy and no definition, so only native comparisons need the word.
    /// </summary>
    internal const ushort AwaitingRespawnHeader = 0xdaff;

    /// <summary>The header word of an enemy slot that holds no enemy.</summary>
    internal const ushort FreeSlotHeader = 0;
}
