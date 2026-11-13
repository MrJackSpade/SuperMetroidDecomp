namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Cartridge values and helpers of <see cref="WideListKind"/> that only verification reads.</summary>
internal static class WideListKindConstants
{
    /// <summary>The span of <paramref name="entries"/> wide entries, which only checks compute.</summary>
    internal static int Span(int entries) => entries * 3;
}
