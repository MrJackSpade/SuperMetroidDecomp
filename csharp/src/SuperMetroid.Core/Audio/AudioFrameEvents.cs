namespace SuperMetroid.Core.Audio;

/// <summary>One 60-Hz update's audio instructions for an <see cref="IAudioBackend"/> (#1304).</summary>
/// <remarks>
/// <see cref="Events"/> is ordered: first the update's bank loads and music commands, in the
/// order the 65816 issued them, then each driver tick in time order, each immediately followed
/// by the sound effects its poll starts. All bank loads and music commands therefore take effect
/// before the update's <see cref="SpcCommandPorts.DspCyclesPerFrame"/> DSP cycles run.
/// </remarks>
/// <param name="Events">The update's ordered events.</param>
public readonly record struct AudioFrameEvents(IReadOnlyList<AudioEvent> Events);
