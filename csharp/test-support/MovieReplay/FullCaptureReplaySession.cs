namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// A session replaying a movie against its private full native capture up to a chosen update, with the timeline whose
/// native memory follows the replay; for diagnostics that need the complete WRAM of a replayed moment.
/// </summary>
/// <param name="Session">The session the host steps; disposing it closes the capture.</param>
/// <param name="Timeline">The session's timeline: <see cref="ConvertedTimelineReplay.Memory"/> is the native WRAM after the last checkpointed update.</param>
internal readonly record struct FullCaptureReplaySession(ConvertedMovieSession Session, ConvertedTimelineReplay Timeline);
