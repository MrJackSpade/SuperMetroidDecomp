namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Cartridge values and helpers of <see cref="ListSlots"/> that only verification reads.</summary>
internal static class ListSlotsConstants
{
    /// <summary>How many slots <paramref name="slots"/> holds, which only checks read.</summary>
    /// <param name="slots">The slots, if any.</param>
    internal static int HeldBy(ListSlots? slots) => slots?.places.Held ?? 0;
}
