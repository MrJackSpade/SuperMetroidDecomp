using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The spans, places and entries the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The spans of two entries of each kind.</summary>
    internal static int Spans => ListKind.Span(2) + WideListKind.Span(2);

    /// <summary>The end and last entry of a span of three entries from the second.</summary>
    internal static int Ends => new ListSpan(2, 3).End() + new ListSpan(2, 3).Last;

    /// <summary>A cursor's entry after it advances and rewinds.</summary>
    internal static int Rewound
    {
        get
        {
            ListCursor cursor = new();
            cursor.Advance();
            cursor.Rewind();
            return cursor.Index;
        }
    }

    /// <summary>Four times three.</summary>
    internal static int Quadrupled => ListArithmetic.Quadruple(3);
}
