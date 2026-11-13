namespace SuperMetroid.Core.Audio;

/// <summary>
/// One 60-Hz update's audio events, which an <see cref="ClipAudioBackend"/> renders (#1304) and every
/// <see cref="Frontend.FrontendFrame"/> publishes to its host.
/// </summary>
/// <remarks>
/// <see cref="Events"/> is ordered: first the update's music data set changes and track selections,
/// in the order the game made them, then each clock step in time order, each immediately followed
/// by the sound effects its request check starts. All data set changes and track selections
/// therefore take effect before the update's <see cref="SoundClockTiming.SamplesPerUpdate"/>
/// samples run.
/// </remarks>
/// <param name="Events">The update's ordered events.</param>
public readonly record struct AudioFrameEvents(IReadOnlyList<AudioEvent> Events);
