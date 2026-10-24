namespace SuperMetroid.Core.Audio;

/// <summary>One request for the sound queue, whichever system made it.</summary>
/// <param name="SoundEffect">The library-qualified sound identity supplied by the original caller.</param>
/// <param name="MaximumQueued">How full the sound's queue may be for the request to join it, as that caller chose.</param>
/// <param name="SoundSuppressed">Native queue guard captured at the request, not deferred publication.</param>
public readonly record struct SoundRequest(SoundEffectId SoundEffect, SoundQueueLimit MaximumQueued, bool SoundSuppressed = false);
