namespace SuperMetroid.Core.Assets;

/// <summary>
/// Fixed wing and articulated-tail visual identities selected by Ridley's
/// $A6:DADE/$A6:DB2A hooks. Pose cadence and segment motion remain gameplay state.
/// </summary>
internal static class RidleySupplementalVisualDefinitions
{
    /// <summary>$A6, the native bank containing Ridley's supplemental OAM maps.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>
    /// The twenty animation-table words at $A6:DB02-$DB28. The first ten are
    /// left-facing; the second ten are right-facing. Repeated poses are retained
    /// because the native wing timer selects by index, not by distinct artwork.
    /// </summary>
    private static ReadOnlySpan<ushort> WingPointers =>
    [
        0xdd4a, 0xdd6a, 0xdd85, 0xdd96, 0xdda7,
        0xddc2, 0xdda7, 0xdd96, 0xdd85, 0xdd6a,
        0xdde2, 0xde02, 0xde1d, 0xde2e, 0xde3f,
        0xde5a, 0xde3f, 0xde2e, 0xde1d, 0xde02,
    ];

    /// <summary>
    /// The sixteen clockwise tail-tip directions at $A6:DCBA-$DCD8.
    /// The order is the native masked-angle index order.
    /// </summary>
    private static ReadOnlySpan<ushort> TailTipPointers =>
    [
        0xdd2e, 0xdd27, 0xdd20, 0xdd19,
        0xdd12, 0xdd0b, 0xdd04, 0xdcfd,
        0xdcf6, 0xdcef, 0xdce8, 0xdce1,
        0xdcda, 0xdd43, 0xdd3c, 0xdd35,
    ];

    /// <summary>$A6:DC90, the large base-segment OAM map.</summary>
    internal const ushort LargeSegment = 0xdc90;

    /// <summary>$A6:DC97, the medium middle-segment OAM map.</summary>
    internal const ushort MediumSegment = 0xdc97;

    /// <summary>$A6:DC9E, the small tip-side segment OAM map.</summary>
    internal const ushort SmallSegment = 0xdc9e;

    internal static int WingPointerCount => WingPointers.Length;
    internal static int TailTipPointerCount => TailTipPointers.Length;

    internal static ushort WingFrameAt(int index) =>
        (uint)index < WingPointers.Length
            ? WingPointers[index]
            : throw new InvalidDataException($"Ridley wing frame {index} is outside the native table.");

    internal static ushort TailTipFrameAt(int index) =>
        (uint)index < TailTipPointers.Length
            ? TailTipPointers[index]
            : throw new InvalidDataException($"Ridley tail-tip direction {index} is outside the native table.");

    internal static ushort SegmentFrameAt(int index) => index switch
    {
        0 or 1 => LargeSegment,
        2 or 3 => MediumSegment,
        4 or 5 => SmallSegment,
        _ => throw new InvalidDataException($"Ridley tail segment {index} is outside six drawn links."),
    };

    internal static EnemySpritemapDefinition[] Frames()
    {
        var pointers = new SortedSet<ushort>
        {
            LargeSegment, MediumSegment, SmallSegment,
        };
        foreach (ushort pointer in TailTipPointers)
            pointers.Add(pointer);
        foreach (ushort pointer in WingPointers)
            pointers.Add(pointer);
        return [.. pointers.Select(pointer => new EnemySpritemapDefinition(
            Bank, pointer, $"ridley_supplement_a6_{pointer:x4}"))];
    }
}
