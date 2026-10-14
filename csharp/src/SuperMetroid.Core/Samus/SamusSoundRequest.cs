namespace SuperMetroid.Core.Samus;

/// <summary>One exact request to a native sound-library queue.</summary>
/// <param name="SoundEffect">Cartridge library and sound identifier.</param>
/// <param name="MaximumQueued">Native queue occupancy threshold.</param>
/// <param name="SoundSuppressed">Queue guard captured by the producer, before any later state changes.</param>
public readonly record struct SamusSoundRequest(
    SoundEffectId SoundEffect,
    byte MaximumQueued,
    bool SoundSuppressed = false);
