using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Audio;

/// <summary>Per-music-voice state from Super Metroid's SPC driver work RAM.</summary>
internal sealed class ManagedSpcMusicChannel
{
    internal ushort PatternOrderPointer;
    internal byte Index;
    internal byte NoteTicksLeft;
    internal byte NoteKeyOffTicksLeft;
    internal byte SubroutineLoops;
    internal byte VolumeFadeTicks;
    internal byte PanTicks;
    internal byte PitchSlideLength;
    internal byte PitchSlideDelayLeft;
    internal byte VibratoHoldCount;
    internal byte VibratoDepth;
    internal byte TremoloHoldCount;
    internal byte TremoloDepth;
    internal byte VibratoChangeCount;
    internal byte NoteLength;
    internal byte NoteGateOffFixedPoint;
    internal byte ChannelVolumeMaster;
    internal byte InstrumentId;
    internal ushort InstrumentPitchBase;
    internal ushort SavedPatternPointer;
    internal ushort PatternStartPointer;
    internal byte PitchEnvelopeTicks;
    internal byte PitchEnvelopeDelay;
    internal byte PitchEnvelopeDirection;
    internal byte PitchEnvelopeSlide;
    internal byte VibratoCount;
    internal byte VibratoRate;
    internal byte VibratoDelayTicks;
    internal byte VibratoFadeTicks;
    internal byte VibratoFadeAddPerTick;
    internal byte VibratoDepthTarget;
    internal byte TremoloCount;
    internal byte TremoloRate;
    internal byte TremoloDelayTicks;
    internal byte ChannelTransposition;
    internal ushort ChannelVolume;
    internal ushort VolumeFadeAddPerTick;
    internal byte VolumeFadeTarget;
    internal byte FinalVolume;
    internal ushort PanValue;
    internal ushort PanAddPerTick;
    internal byte PanTarget;
    internal byte PanFlags;
    internal ushort Pitch;
    internal ushort PitchAddPerTick;
    internal byte PitchTarget;
    internal byte FineTune;
    internal byte CutKey;
}

/// <summary>Shared allocation and command state for one of the three SFX libraries.</summary>
internal sealed class ManagedSpcSoundLibrary
{
    internal byte Priority;
    internal byte ChannelIndexTimesTwo;
    internal ushort ChannelVoiceBitsetPointer;
    internal byte CurrentSound;
    internal ushort ChannelVoiceMaskPointer;
    internal ushort ChannelVoiceIndexPointer;
    internal byte Mode;
    internal ushort CurrentPointer;
    internal byte EnabledVoices;
    internal byte VoicesToSetup;
    internal byte ChannelIndex;
    internal byte VoicesRemaining;
    internal byte CurrentSoundIndex;
    internal byte VoiceId;
    internal byte InitializationFlag;
    internal byte VoiceIndex;
}

/// <summary>One logical SFX stream temporarily borrowing a hardware music voice.</summary>
internal sealed class ManagedSpcSoundChannel
{
    internal byte VoiceBitset;
    internal byte InstructionTimer;
    internal byte Subnote;
    internal byte RepeatPoint;
    internal byte Note;
    internal byte PointerIndex;
    internal byte VoiceIndex;
    internal byte VoiceIndexTimesEight;
    internal byte ReleaseTimer;
    internal byte Volume;
    internal byte TargetNote;
    internal byte PhaseInvert;
    internal ushort Pointer;
    internal byte RepeatCounter;
    internal byte EnablePitchSlide;
    internal byte ReleaseFlag;
    internal byte EnablePitchSlideLegato;
    internal ushort AdsrSettings;
    internal byte ChannelMask;
    internal byte Legato;
    internal byte Disabled;
    internal byte SubnoteDelta;
    internal byte UpdateAdsr;
}
