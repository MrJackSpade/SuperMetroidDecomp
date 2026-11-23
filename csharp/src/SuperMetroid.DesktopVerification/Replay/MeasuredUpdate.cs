namespace SuperMetroid.DesktopVerification.Replay;

/// <summary>One update of a measured replay (#2653): where the movie stood and how long the host took to run it.</summary>
/// <param name="Update">The update's number, from one.</param>
/// <param name="SourceFrame">The movie's source frame.</param>
/// <param name="Phase">What the game was doing.</param>
/// <param name="Room">The active room, or none.</param>
/// <param name="RoomState">The active room state, or none.</param>
/// <param name="UpdateMilliseconds">The host's time running the update, from its input to its published frame.</param>
/// <param name="Sequence">The publication sequence of the frame it published.</param>
internal sealed record MeasuredUpdate(int Update, long? SourceFrame, string Phase, string? Room, string? RoomState, double UpdateMilliseconds, long Sequence);
