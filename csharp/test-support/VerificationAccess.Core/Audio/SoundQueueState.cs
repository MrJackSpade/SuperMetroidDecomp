namespace SuperMetroid.Core.Audio;

/// <summary>A sound queue's start, next and state bytes.</summary>
/// <param name="Start">The start.</param>
/// <param name="Next">The next.</param>
/// <param name="State">The state.</param>
internal readonly record struct SoundQueueState(byte Start, byte Next, byte State);
