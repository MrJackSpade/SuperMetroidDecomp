namespace SuperMetroid.Core.Game;

/// <summary>Physical pose extents, independent of editable artwork and its display offsets.</summary>
internal static class SamusPoseCollisionDefinitions
{
    /// <summary>$91:B629 PoseDefinitions byte six for poses $00..$FC plus the exact adjacent-code observations for $FD..$FF. Consumed by $90:EC22 Samus_SetRadius.</summary>
    private static ReadOnlySpan<byte> VerticalRadii =>
    [
        24, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
        21, 21, 21, 19, 19, 19, 19, 10, 10, 12, 12, 12, 12, 7, 7, 7,
        7, 7, 7, 7, 7, 21, 21, 16, 16, 19, 19, 19, 19, 10, 10, 19,
        19, 7, 7, 7, 7, 16, 16, 7, 7, 7, 7, 21, 21, 16, 16, 7,
        7, 7, 7, 16, 16, 21, 21, 21, 21, 21, 21, 19, 19, 19, 19, 19,
        19, 19, 19, 21, 21, 19, 19, 19, 19, 19, 19, 16, 16, 16, 16, 16,
        16, 16, 16, 12, 12, 12, 12, 19, 19, 19, 19, 19, 19, 19, 19, 19,
        19, 16, 16, 16, 16, 21, 21, 21, 21, 7, 7, 7, 7, 7, 7, 7,
        7, 12, 12, 19, 19, 16, 16, 19, 19, 21, 21, 21, 21, 21, 21, 19,
        19, 19, 19, 19, 19, 19, 19, 16, 16, 16, 16, 24, 21, 21, 19, 19,
        19, 19, 16, 16, 21, 21, 21, 21, 21, 21, 21, 21, 19, 19, 10, 10,
        19, 19, 17, 17, 16, 16, 16, 16, 16, 16, 21, 21, 21, 21, 21, 21,
        21, 21, 21, 21, 21, 7, 21, 19, 19, 19, 19, 19, 19, 19, 19, 21,
        21, 21, 21, 21, 21, 21, 21, 21, 21, 16, 16, 7, 7, 21, 21, 7,
        21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
        21, 16, 16, 16, 16, 16, 16, 21, 21, 21, 21, 21, 21, 138, 24, 16,
    ];

    internal static byte ReadVerticalRadius(byte pose) => VerticalRadii[pose];
}
