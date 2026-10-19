namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One call to one of the cartridge's three <c>QueueSfx</c> entry points.
/// </summary>
/// <param name="SoundEffect">The library-qualified sound identity supplied by the original 65816 caller.</param>
/// <param name="MaximumQueued">
/// The exact queue-cap variant selected by that caller (for example Max3 or Max6).
/// </param>
/// <param name="SoundSuppressed">Native queue guard captured at the request, not deferred publication.</param>
public readonly record struct EnemySoundRequest(
    SoundEffectId SoundEffect,
    int MaximumQueued,
    bool SoundSuppressed = false);
