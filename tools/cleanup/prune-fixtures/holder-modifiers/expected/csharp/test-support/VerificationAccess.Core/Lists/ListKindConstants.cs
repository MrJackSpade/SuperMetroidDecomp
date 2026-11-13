namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Cartridge values and helpers of <see cref="ListKind"/> that only verification reads.</summary>
internal static class ListKindConstants
{
    /// <summary>The span of <paramref name="entries"/> entries of any kind, which only checks compute.</summary>
    internal static int Span(int entries) => entries * 2;
}
