namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Verification access to <see cref="ListSlots"/> members production does not use.</summary>
internal static class ListSlotsAccess
{
    /// <summary>Exposes <see cref="ListSlots"/> state that only verification reads.</summary>
    extension(ListSlots self)
    {
        /// <summary>How many slots are free, which only checks read: <see cref="ListSlots.places"/> less those held.</summary>
        internal int Free => self.places.Capacity - self.Count;
    }
}
