namespace SuperMetroid.IntegrationVerification;

/// <summary>The pixel and PCM counts and frame after advancing an Android session.</summary>
/// <param name="Pixels">The pixels.</param>
/// <param name="Pcm">The pcm.</param>
/// <param name="Frame">The frame.</param>
internal readonly record struct SessionAdvance(uint Pixels, uint Pcm, int Frame);
