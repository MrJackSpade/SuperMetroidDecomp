using SuperMetroid.Core.Hardware;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Shared allocation and command state for one of the three SFX libraries.</summary>
internal sealed class ManagedSpcSoundLibrary
{
    /// <summary>Preemption level selected by the incoming sound command.</summary>
    internal byte Priority;
    /// <summary>Twice the next logical SFX channel index during hardware voice assignment.</summary>
    internal byte ChannelIndexTimesTwo;
    /// <summary>SPC work-RAM address of the current channel's allocated hardware-voice bit.</summary>
    internal ushort ChannelVoiceBitsetPointer;
    /// <summary>Sound effect currently playing in this library.</summary>
    internal byte CurrentSound;
    /// <summary>SPC work-RAM address of the current channel's inverse voice mask.</summary>
    internal ushort ChannelVoiceMaskPointer;
    /// <summary>SPC work-RAM address of the current channel's hardware voice index.</summary>
    internal ushort ChannelVoiceIndexPointer;
    /// <summary>Library-three low-health or cancellation mode selected by its command.</summary>
    internal byte Mode;
    /// <summary>Pointer to the selected sound's table of per-channel instruction streams.</summary>
    internal ushort CurrentPointer;
    /// <summary>Bit mask of hardware voices currently borrowed by this library.</summary>
    internal byte EnabledVoices;
    /// <summary>Number of hardware voices still requested by the new sound's configuration.</summary>
    internal byte VoicesToSetup;
    /// <summary>Next logical channel to receive a free hardware voice.</summary>
    internal byte ChannelIndex;
    /// <summary>Shifted occupancy mask used while scanning hardware voices from high to low.</summary>
    internal byte VoicesRemaining;
    /// <summary>Selected sound's index within the library's pointer table.</summary>
    internal byte CurrentSoundIndex;
    /// <summary>Descending hardware voice number examined during allocation.</summary>
    internal byte VoiceId;
    /// <summary>Marks that the current sound's channels and borrowed voices have been initialized.</summary>
    internal byte InitializationFlag;
    /// <summary>DSP register-pair index assigned to the currently allocated voice.</summary>
    internal byte VoiceIndex;

    /// <summary>Appends every field of this sound-effect library, in declaration order, to a driver-state snapshot (#1305).</summary>
    internal void WriteState(List<byte> state)
    {
        state.AddRange([Priority, ChannelIndexTimesTwo, CurrentSound, Mode, EnabledVoices, VoicesToSetup, ChannelIndex, VoicesRemaining, CurrentSoundIndex, VoiceId, InitializationFlag, VoiceIndex]);
        state.Add(SnesLittleEndian.LowByte(ChannelVoiceBitsetPointer));
        state.Add(SnesLittleEndian.HighByte(ChannelVoiceBitsetPointer));
        state.Add(SnesLittleEndian.LowByte(ChannelVoiceMaskPointer));
        state.Add(SnesLittleEndian.HighByte(ChannelVoiceMaskPointer));
        state.Add(SnesLittleEndian.LowByte(ChannelVoiceIndexPointer));
        state.Add(SnesLittleEndian.HighByte(ChannelVoiceIndexPointer));
        state.Add(SnesLittleEndian.LowByte(CurrentPointer));
        state.Add(SnesLittleEndian.HighByte(CurrentPointer));
    }
}
