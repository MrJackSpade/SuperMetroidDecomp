namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Observable call to one of the cartridge's three queued-sound libraries.</summary>
public readonly record struct PlmSoundRequest(
    SoundEffectId SoundEffect,
    int MaximumQueued,
    bool SoundSuppressed = false);
