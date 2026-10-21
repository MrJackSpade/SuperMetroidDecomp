namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>The first two loop points heard on a channel and the voice that played them.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Voice">The voice.</param>
internal readonly record struct ChannelLoop(long First, long Second, int Voice);
