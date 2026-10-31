namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The one part of Torizo's spawn parameter words gameplay keeps: the width of parameter 1's independent low bits,
/// beside the posture latches above them. Where each latch sits is the checks' knowledge.
/// </summary>
[EncodingOwner("Torizo spawn parameter 1's low bits")]
internal static class TorizoParameterFlagCodec
{
    /// <summary>The bits of parameter 1 below the posture bits.</summary>
    internal const int PostureLowBitsMask = (1 << 13) - 1;
}
