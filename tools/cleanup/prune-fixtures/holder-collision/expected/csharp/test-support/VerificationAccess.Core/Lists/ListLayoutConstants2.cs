namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Cartridge values and helpers of <see cref="ListLayout"/> that only verification reads.</summary>
internal static class ListLayoutConstants2
{
    /// <summary>The number of lists a layout of <paramref name="entries"/> holds.</summary>
    internal static int Count(int[] entries) => entries.Length;
}
