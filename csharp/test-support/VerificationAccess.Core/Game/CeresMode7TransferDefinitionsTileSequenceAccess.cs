using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="CeresMode7TransferDefinitions.TileSequence"/> members production does not use.</summary>
internal static class CeresMode7TransferDefinitionsTileSequenceAccess
{
    /// <summary>Exposes the number of Mode 7 tile transfers in a Ceres sequence.</summary>
    extension(CeresMode7TransferDefinitions.TileSequence self)
    {
        /// <summary>Number of compiled Mode 7 tile transfers in this sequence.</summary>
        internal int Length => self.Count;
    }
}
