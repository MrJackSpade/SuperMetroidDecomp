namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Immutable physical level copied by the SR388 discovery setup. This 32-by-12
/// source is distinct from the visible BG page; the final four 32-block rows of
/// the declared room retain their native zero initialization.
/// </summary>
internal static class IntroBabyDiscoveryCollisionDefinitions
{
    /// <summary>Thirty-two physical foreground words per source row.</summary>
    internal const int Columns = 32;
    /// <summary>Twelve ROM-authored rows; the room itself is sixteen rows tall.</summary>
    internal const int SourceRows = 12;
    /// <summary>Ten zero rows precede the two authored nonzero rows.</summary>
    internal const int ZeroRows = 10;
    /// <summary>The eleventh source row contains word $1000 in every column.</summary>
    internal const ushort PenultimateRowWord = 0x1000;
    /// <summary>The final source row contains word $8000 in every column.</summary>
    internal const ushort FinalRowWord = 0x8000;

    /// <summary>Native room height, including four zero-initialized rows beyond the imported region.</summary>
    internal const int RoomRows = 16;

    /// <summary>Construct mutable room state directly from its two horizontal collision strips.</summary>
    internal static ushort[] CreateForeground()
    {
        var foreground = new ushort[Columns * RoomRows];
        foreground.AsSpan(ZeroRows * Columns, Columns).Fill(PenultimateRowWord);
        foreground.AsSpan((SourceRows - 1) * Columns, Columns).Fill(FinalRowWord);
        return foreground;
    }
}