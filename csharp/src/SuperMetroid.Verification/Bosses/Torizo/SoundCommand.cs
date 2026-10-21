namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The library and maximum queued of one sound command.</summary>
/// <param name="Library">The library.</param>
/// <param name="MaximumQueued">The maximum queued.</param>
internal readonly record struct SoundCommand(SoundEffectLibrary Library, byte MaximumQueued);
