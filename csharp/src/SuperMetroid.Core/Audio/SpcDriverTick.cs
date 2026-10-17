namespace SuperMetroid.Core.Audio;

/// <summary>One driver timer tick within a frame, with the sound-effect polls it ran.</summary>
/// <param name="DspCycleOffset">DSP cycles from the frame start at which the tick runs.</param>
/// <param name="Ticks">Elapsed timer periods the tick accounts for: a count, at most one pending period plus the whole periods since it.</param>
/// <param name="SoundEffectsPolled">Whether the sound-effect timer carried and polled all three libraries.</param>
/// <param name="Library1">Library 1's poll when <paramref name="SoundEffectsPolled"/> is set.</param>
/// <param name="Library2">Library 2's poll when <paramref name="SoundEffectsPolled"/> is set.</param>
/// <param name="Library3">Library 3's poll when <paramref name="SoundEffectsPolled"/> is set.</param>
public readonly record struct SpcDriverTick(
    int DspCycleOffset,
    int Ticks,
    bool SoundEffectsPolled,
    SpcSoundEffectPoll Library1,
    SpcSoundEffectPoll Library2,
    SpcSoundEffectPoll Library3);
