namespace SuperMetroid.Core.Audio;

/// <summary>Resource limits for building a finite PCM representation of a BRR loop.</summary>
internal static class BrrExtractionLimits
{
    /// <summary>
    /// Maximum decoded PCM frames while searching for a repeated decoder-history state.
    /// This is a host resource bound, not a cartridge address or an approximation: a valid
    /// loop that exceeds it fails extraction rather than silently repeating an inexact wave.
    /// </summary>
    internal const int MaximumDecodedLoopSamples = 4 * 1024 * 1024;
}
