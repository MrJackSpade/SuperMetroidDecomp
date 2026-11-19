namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Verification access to <see cref="ListRack"/> members production does not use.</summary>
internal static class ListRackAccess
{
    /// <summary>Exposes <see cref="ListRack"/> state that only verification reads.</summary>
    extension(ListRack self)
    {
        /// <summary>Twice the rack's places, which only checks read.</summary>
        internal int Doubled => self._places.Capacity * 2;
    }
}
