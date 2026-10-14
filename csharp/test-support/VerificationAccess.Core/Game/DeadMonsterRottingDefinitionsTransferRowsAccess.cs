using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="DeadMonsterRottingDefinitions.TransferRows"/> members production does not use.</summary>
internal static class DeadMonsterRottingDefinitionsTransferRowsAccess
{
    /// <summary>Exposes the number of transfer rows in a dead-monster rotting phase.</summary>
    extension(DeadMonsterRottingDefinitions.TransferRows self)
    {
        /// <summary>Number of native dead-monster transfer rows in this view.</summary>
        internal int Length => self.Count;
    }
}
