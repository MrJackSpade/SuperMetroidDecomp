namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool's orbit and orientation encodings, as its bank-$AA segment routines apply them.</summary>
internal static class ShaktoolOrbitDefinitions
{
    /// <summary>One angle-table entry of a 16-bit angle: the straightening step and its extra centre step.</summary>
    internal const ushort TableStep = 0x0100;

    /// <summary>Bit 15 of the centre segment's orientation word: the chain runs reversed, so its midpoint flips a half turn.</summary>
    internal const ushort ReversedOrientationBit = 0x8000;

    /// <summary>Rounds a midpoint's table angle to one of eight direction buckets.</summary>
    internal const int DirectionBucketMask = 0x00e0;

    /// <summary>The orientation word installed when the centre reaches its target angle.</summary>
    internal const ushort StraighteningStart = 0x7800;

    /// <summary>Once the orientation word reaches this, the chain starts straightening.</summary>
    internal const ushort StraighteningThreshold = 0xf000;

    /// <summary>The motion flags a second reversal keeps: it clears the reversed-once, straightening and bit-12 flags.</summary>
    internal const ShaktoolMotionFlags SecondReversalKeptFlags = (ShaktoolMotionFlags)0x8fff;

    /// <summary>The fraction middle segments reset to on an endpoint swap: half a pixel.</summary>
    internal const ushort SwapFraction = 0x8000;
}
