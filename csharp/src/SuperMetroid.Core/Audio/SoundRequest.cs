namespace SuperMetroid.Core.Audio;

/// <summary>One request for the sound queue, whichever system made it.</summary>
/// <param name="SoundEffect">The library-qualified sound identity supplied by the original caller.</param>
/// <param name="Priority">How full the sound's queue may be for the request to join it, as that caller chose.</param>
/// <param name="SoundSuppressed">Whether sounds were suppressed when the request was made, captured then rather than when it is published.</param>
public readonly record struct SoundRequest(SoundEffectId SoundEffect, SoundRequestPriority Priority, bool SoundSuppressed = false);
