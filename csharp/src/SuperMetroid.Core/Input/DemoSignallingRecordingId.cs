namespace SuperMetroid.Core.Input;

/// <summary>
/// The intro's recordings that signal their owner as they end (#2553): the scene that loads one gives it the routine its
/// signal runs. The title's demonstrations play only <see cref="DemoInputRecordingId"/> recordings, which never signal.
/// </summary>
public enum DemoSignallingRecordingId
{
    /// <summary>The input that ends the intro's discovery of the Baby, once Samus has looked at it.</summary>
    IntroBabyDiscoveryEnd,
    /// <summary>The input Samus fires with in the intro's flashback of her fight with Mother Brain.</summary>
    IntroMotherBrainFlashback,
}
