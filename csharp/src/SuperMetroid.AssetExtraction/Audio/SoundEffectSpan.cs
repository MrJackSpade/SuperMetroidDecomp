namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>A rendered effect's lifetime, known once its logical end has come (#2554): an end follows the start it ends.</summary>
/// <param name="StartSample">Absolute sample of the tick at which the driver accepted the request.</param>
/// <param name="EndSample">Absolute sample of the tick at which the library's logical end came.</param>
internal sealed record SoundEffectSpan(long StartSample, long EndSample);
