namespace SuperMetroid.Core.Audio;

/// <summary>What starting a sound effect does to its library's priority (#1305).</summary>
public enum SoundEffectPriorityEffect
{
    /// <summary>The library becomes low priority: any later request may replace the sound.</summary>
    Low,

    /// <summary>The library becomes high priority: ordinary requests are rejected until the sound ends.</summary>
    High,

    /// <summary>The library keeps its current priority (library 3's low-health beep).</summary>
    Unchanged,
}
