namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadMonsterRottingDefinitions"/>; never linked by player hosts.</summary>
internal static class DeadMonsterRottingDefinitionsTooling
{
    /// <summary>Enumerates compiled bank-$A9 corpse-rotation transfer descriptors in Sidehopper, Zoomer, Ripper, then Skree table order.</summary>
    /// <returns>Every transfer row from the supported variant tables, preserving each table's native row order.</returns>
    internal static IEnumerable<DeadMonsterVramTransferDefinition> AllTransfers
    {
        get
        {
            for (int index = 0; index < 10; index++)
            {
                ushort table = index < 2 ? (ushort)(DeadMonsterRottingDefinitions.SidehopperTransfers + index * 42)
                    : index < 5 ? (ushort)(DeadMonsterRottingDefinitions.ZoomerTransfers + (index - 2) * 18)
                    : index < 7 ? (ushort)(DeadMonsterRottingDefinitions.RipperTransfers + (index - 5) * 18)
                    : (ushort)(DeadMonsterRottingDefinitions.SkreeTransfers + (index - 7) * 34);
                foreach (var row in DeadMonsterRottingDefinitions.ForTransferTable(table)) yield return row;
            }
        }
    }
}
