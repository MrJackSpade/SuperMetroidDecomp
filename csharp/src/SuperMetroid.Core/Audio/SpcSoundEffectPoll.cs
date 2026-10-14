namespace SuperMetroid.Core.Audio;

/// <summary>One sound-effect library's command as a driver poll acted on it.</summary>
/// <param name="Command">The value latched at the previous poll, now echoed to the CPU.</param>
/// <param name="Changed">Whether it differs from the value acted on at the previous poll; the driver starts a sound only for a changed nonzero command.</param>
public readonly record struct SpcSoundEffectPoll(byte Command, bool Changed);
