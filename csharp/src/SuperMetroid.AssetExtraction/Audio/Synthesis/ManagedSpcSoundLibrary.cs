namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Shared allocation and command state for one of the three SFX libraries.</summary>
internal sealed class ManagedSpcSoundLibrary
{
    /// <summary>Preemption level selected by the incoming sound command.</summary>
    internal int Priority;
    /// <summary>Twice the next logical SFX channel index during hardware voice assignment.</summary>
    internal int ChannelIndexTimesTwo;
    /// <summary>SPC work-RAM address of the current channel's allocated hardware-voice bit.</summary>
    internal int ChannelVoiceBitsetPointer;
    /// <summary>Sound effect currently playing in this library, by its command; the driver's silent command while none plays.</summary>
    internal int CurrentSound;

    /// <summary>The driver's command that asks for no sound, which the library also holds while it plays none.</summary>
    internal const int SilentCommand = 0;

    /// <summary>Whether a sound effect plays in this library.</summary>
    internal bool IsPlaying => CurrentSound != SilentCommand;
    /// <summary>SPC work-RAM address of the current channel's inverse voice mask.</summary>
    internal int ChannelVoiceMaskPointer;
    /// <summary>SPC work-RAM address of the current channel's hardware voice index.</summary>
    internal int ChannelVoiceIndexPointer;
    /// <summary>Library three's request mode, which its low-health and cancellation commands select (#2560).</summary>
    internal SoundLibraryMode Mode;
    /// <summary>Pointer to the selected sound's table of per-channel instruction streams.</summary>
    internal int CurrentPointer;
    /// <summary>Bit mask of hardware voices currently borrowed by this library.</summary>
    internal int EnabledVoices;
    /// <summary>Number of hardware voices still requested by the new sound's configuration.</summary>
    internal int VoicesToSetup;
    /// <summary>Next logical channel to receive a free hardware voice.</summary>
    internal int ChannelIndex;
    /// <summary>Shifted occupancy mask used while scanning hardware voices from high to low.</summary>
    internal int VoicesRemaining;
    /// <summary>Selected sound's index within the library's pointer table.</summary>
    internal int CurrentSoundIndex;
    /// <summary>Descending hardware voice number examined during allocation.</summary>
    internal int VoiceId;
    /// <summary>Whether the current sound's channels and borrowed voices have been initialized.</summary>
    internal bool Initialized;
    /// <summary>DSP register-pair index assigned to the currently allocated voice.</summary>
    internal int VoiceIndex;

    /// <summary>Appends every field of this sound-effect library, in declaration order, to a driver-state snapshot (#1305).</summary>
    internal void WriteState(List<byte> state)
    {
        SpcDriverSnapshot.AddBytes(state, [Priority, ChannelIndexTimesTwo, CurrentSound, SpcDriverSnapshot.Byte(Mode), EnabledVoices, VoicesToSetup, ChannelIndex, VoicesRemaining, CurrentSoundIndex, VoiceId, SpcDriverSnapshot.Mark(Initialized), VoiceIndex]);
        SpcDriverSnapshot.AddWords(state, [ChannelVoiceBitsetPointer, ChannelVoiceMaskPointer, ChannelVoiceIndexPointer, CurrentPointer]);
    }
}
