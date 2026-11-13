namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Verification access to <see cref="ListGauge"/> members production does not use.</summary>
internal static class ListGaugeAccess
{
    /// <summary>Exposes <see cref="ListGauge"/> state that only verification reads.</summary>
    extension(ListGauge self)
    {
        /// <summary>The level before scaling, which only checks read; <c>ListGauge.Scaled</c> doubles it.</summary>
        internal int Raw => self.Level;
    }
}
