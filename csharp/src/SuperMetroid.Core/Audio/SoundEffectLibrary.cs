namespace SuperMetroid.Core.Audio;

/// <summary>
/// The three sound libraries, each with its own request queue and its own sound playing at a time.
/// A sound belongs to exactly one library; the values are not combinable flags.
/// </summary>
public enum SoundEffectLibrary
{
    /// <summary>The first library: queue index zero.</summary>
    Library1 = 1,
    /// <summary>The second library: queue index one.</summary>
    Library2 = 2,
    /// <summary>The third library, which also holds the low-health beep: queue index two.</summary>
    Library3 = 3,
}
