namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool's orbit and orientation encodings, as its bank-$AA segment routines apply them.</summary>
internal static class ShaktoolOrbitDefinitions
{
    /// <summary>One angle-table entry of a 16-bit angle: the straightening step and its extra centre step.</summary>
    internal const ushort TableStep = 0x0100;

    /// <summary>The direction buckets a middle segment's heading is rounded to, one for each equal arc of the turn.</summary>
    internal const int OrientationBuckets = 8;

    /// <summary>The table units a midpoint's heading is advanced by before its bucket is chosen.</summary>
    internal const int OrientationBucketLead = 8;

    /// <summary>The curl progress installed when the centre reaches its target angle: 120 angle-table steps.</summary>
    internal const ushort StraighteningStart = 120 * TableStep;

    /// <summary>Once the curl progress reaches 240 angle-table steps, the chain starts straightening.</summary>
    internal const ushort StraighteningThreshold = 240 * TableStep;

    /// <summary>The subpixels in one pixel.</summary>
    private const int SubpixelsPerPixel = 65536;

    /// <summary>The fraction middle segments reset to on an endpoint swap: half a pixel.</summary>
    internal const ushort SwapFraction = SubpixelsPerPixel / 2;
}
