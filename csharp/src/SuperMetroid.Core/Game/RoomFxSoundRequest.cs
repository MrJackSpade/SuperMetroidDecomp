namespace SuperMetroid.Core.Game;

/// <summary>One bank-$88 room-FX request for the global cartridge sound queue.</summary>
public readonly record struct RoomFxSoundRequest(
    SoundEffectId SoundEffect,
    byte MaximumQueued,
    bool SoundSuppressed = false);
