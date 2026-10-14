using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="MapArrowVisual"/> members production does not use.</summary>
internal static class MapArrowVisualAccess
{
    /// <summary>Inspects whether arrow timing edits are stored independently of stock values.</summary>
    extension(MapArrowVisual self)
    {
        /// <summary>Counts explicitly overridden arrow-frame durations.</summary>
        internal int StoredDurationCount => self.durationOverrides?.Count ?? 0;
    }
}
