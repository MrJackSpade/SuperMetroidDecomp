using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop.Timing;

/// <summary>How many frames a playback batch completed and the last one.</summary>
/// <param name="CompletedFrames">The completed frames.</param>
/// <param name="LastFrame">The last frame.</param>
internal readonly record struct PlaybackBatchResult(int CompletedFrames, FrontendFrame? LastFrame);
