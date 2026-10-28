namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Cartridge values and helpers of <see cref="SnesSpritemapXWord"/> that only verification reads.</summary>
internal static class SnesSpritemapXWordConstants
{
    /// <summary>The largest value the nine-bit X offset field holds, $1FF.</summary>
    public const int OffsetFieldMaximum = SnesSpritemapXWord.OffsetMask;
}
