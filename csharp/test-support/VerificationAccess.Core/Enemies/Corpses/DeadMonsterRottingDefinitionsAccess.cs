namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Verification access to <see cref="DeadMonsterRottingDefinitions"/> members production does not use.</summary>
internal static class DeadMonsterRottingDefinitionsAccess
{
    /// <summary>Enumerates the ten authored corpse-rotting transfer tables
    /// for Sidehopper, Zoomer, Ripper, and Skree in native table order.</summary>
    internal static IEnumerable<DeadMonsterVramTransferDefinition> AllTransfers
    {
        get
        {
            foreach (CorpseTransferTable table in CorpseNativeAddresses.AllTransferTables)
                foreach (var row in DeadMonsterRottingDefinitions.ForTransferTable(table)) yield return row;
        }
    }
}
