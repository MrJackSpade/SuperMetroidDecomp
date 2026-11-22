namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Where in its movie a replayed frame comes from.</summary>
/// <param name="Update">The converted update whose input the frame runs; a message box's frames share their update.</param>
/// <param name="SourceFrame">The original movie's frame whose controller input the frame runs.</param>
internal readonly record struct MovieReplayPosition(int Update, int SourceFrame);
