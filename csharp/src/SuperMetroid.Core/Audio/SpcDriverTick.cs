namespace SuperMetroid.Core.Audio;

/// <summary>One driver timer tick within a frame, with the sound-effect polls it ran.</summary>
/// <param name="DspCycleOffset">DSP cycles from the frame start at which the tick runs.</param>
/// <param name="Ticks">Elapsed timer periods the tick accounts for: a count, at most one pending period plus the whole periods since it.</param>
/// <param name="SoundEffectsPolled">Whether the sound-effect timer carried and polled all three libraries.</param>
/// <param name="LibraryPolls">Each library's poll when <paramref name="SoundEffectsPolled"/> is set, indexed by host queue index.</param>
[IndependentFlag("whether this driver tick polled the sound-effect ports, a fact of the tick's timing")]
public readonly record struct SpcDriverTick(
    int DspCycleOffset,
    int Ticks,
    bool SoundEffectsPolled,
    SoundLibraryPolls LibraryPolls);
