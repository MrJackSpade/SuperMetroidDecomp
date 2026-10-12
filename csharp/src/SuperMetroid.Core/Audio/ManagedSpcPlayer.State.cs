using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Audio;

/// <summary>Per-music-voice state from Super Metroid's SPC driver work RAM.</summary>
internal sealed class ManagedSpcMusicChannel
{
    /// <summary>Stores pattern order pointer.</summary>
    internal ushort PatternOrderPointer;
    /// <summary>Stores index.</summary>
    internal byte Index;
    /// <summary>Stores note ticks left.</summary>
    internal byte NoteTicksLeft;
    /// <summary>Stores note key off ticks left.</summary>
    internal byte NoteKeyOffTicksLeft;
    /// <summary>Stores subroutine loops.</summary>
    internal byte SubroutineLoops;
    /// <summary>Stores volume fade ticks.</summary>
    internal byte VolumeFadeTicks;
    /// <summary>Stores pan ticks.</summary>
    internal byte PanTicks;
    /// <summary>Stores pitch slide length.</summary>
    internal byte PitchSlideLength;
    /// <summary>Stores pitch slide delay left.</summary>
    internal byte PitchSlideDelayLeft;
    /// <summary>Stores vibrato hold count.</summary>
    internal byte VibratoHoldCount;
    /// <summary>Stores vibrato depth.</summary>
    internal byte VibratoDepth;
    /// <summary>Stores tremolo hold count.</summary>
    internal byte TremoloHoldCount;
    /// <summary>Stores tremolo depth.</summary>
    internal byte TremoloDepth;
    /// <summary>Stores vibrato change count.</summary>
    internal byte VibratoChangeCount;
    /// <summary>Stores note length.</summary>
    internal byte NoteLength;
    /// <summary>Stores note gate off fixed point.</summary>
    internal byte NoteGateOffFixedPoint;
    /// <summary>Stores channel volume master.</summary>
    internal byte ChannelVolumeMaster;
    /// <summary>Stores instrument id.</summary>
    internal byte InstrumentId;
    /// <summary>Stores instrument pitch base.</summary>
    internal ushort InstrumentPitchBase;
    /// <summary>Stores saved pattern pointer.</summary>
    internal ushort SavedPatternPointer;
    /// <summary>Stores pattern start pointer.</summary>
    internal ushort PatternStartPointer;
    /// <summary>Stores pitch envelope ticks.</summary>
    internal byte PitchEnvelopeTicks;
    /// <summary>Stores pitch envelope delay.</summary>
    internal byte PitchEnvelopeDelay;
    /// <summary>Stores pitch envelope direction.</summary>
    internal byte PitchEnvelopeDirection;
    /// <summary>Stores pitch envelope slide.</summary>
    internal byte PitchEnvelopeSlide;
    /// <summary>Stores vibrato count.</summary>
    internal byte VibratoCount;
    /// <summary>Stores vibrato rate.</summary>
    internal byte VibratoRate;
    /// <summary>Stores vibrato delay ticks.</summary>
    internal byte VibratoDelayTicks;
    /// <summary>Stores vibrato fade ticks.</summary>
    internal byte VibratoFadeTicks;
    /// <summary>Stores vibrato fade add per tick.</summary>
    internal byte VibratoFadeAddPerTick;
    /// <summary>Stores vibrato depth target.</summary>
    internal byte VibratoDepthTarget;
    /// <summary>Stores tremolo count.</summary>
    internal byte TremoloCount;
    /// <summary>Stores tremolo rate.</summary>
    internal byte TremoloRate;
    /// <summary>Stores tremolo delay ticks.</summary>
    internal byte TremoloDelayTicks;
    /// <summary>Stores channel transposition.</summary>
    internal byte ChannelTransposition;
    /// <summary>Stores channel volume.</summary>
    internal ushort ChannelVolume;
    /// <summary>Stores volume fade add per tick.</summary>
    internal ushort VolumeFadeAddPerTick;
    /// <summary>Stores volume fade target.</summary>
    internal byte VolumeFadeTarget;
    /// <summary>Stores final volume.</summary>
    internal byte FinalVolume;
    /// <summary>Stores pan value.</summary>
    internal ushort PanValue;
    /// <summary>Stores pan add per tick.</summary>
    internal ushort PanAddPerTick;
    /// <summary>Stores pan target.</summary>
    internal byte PanTarget;
    /// <summary>Stores pan flags.</summary>
    internal byte PanFlags;
    /// <summary>Stores pitch.</summary>
    internal ushort Pitch;
    /// <summary>Stores pitch add per tick.</summary>
    internal ushort PitchAddPerTick;
    /// <summary>Stores pitch target.</summary>
    internal byte PitchTarget;
    /// <summary>Stores fine tune.</summary>
    internal byte FineTune;
    /// <summary>Stores cut key.</summary>
    internal byte CutKey;
}

/// <summary>Shared allocation and command state for one of the three SFX libraries.</summary>
internal sealed class ManagedSpcSoundLibrary
{
    /// <summary>Stores priority.</summary>
    internal byte Priority;
    /// <summary>Stores channel index times two.</summary>
    internal byte ChannelIndexTimesTwo;
    /// <summary>Stores channel voice bitset pointer.</summary>
    internal ushort ChannelVoiceBitsetPointer;
    /// <summary>Stores current sound.</summary>
    internal byte CurrentSound;
    /// <summary>Stores channel voice mask pointer.</summary>
    internal ushort ChannelVoiceMaskPointer;
    /// <summary>Stores channel voice index pointer.</summary>
    internal ushort ChannelVoiceIndexPointer;
    /// <summary>Stores mode.</summary>
    internal byte Mode;
    /// <summary>Stores current pointer.</summary>
    internal ushort CurrentPointer;
    /// <summary>Stores enabled voices.</summary>
    internal byte EnabledVoices;
    /// <summary>Stores voices to setup.</summary>
    internal byte VoicesToSetup;
    /// <summary>Stores channel index.</summary>
    internal byte ChannelIndex;
    /// <summary>Stores voices remaining.</summary>
    internal byte VoicesRemaining;
    /// <summary>Stores current sound index.</summary>
    internal byte CurrentSoundIndex;
    /// <summary>Stores voice id.</summary>
    internal byte VoiceId;
    /// <summary>Stores initialization flag.</summary>
    internal byte InitializationFlag;
    /// <summary>Stores voice index.</summary>
    internal byte VoiceIndex;
}

/// <summary>One logical SFX stream temporarily borrowing a hardware music voice.</summary>
internal sealed class ManagedSpcSoundChannel
{
    /// <summary>Stores voice bitset.</summary>
    internal byte VoiceBitset;
    /// <summary>Stores instruction timer.</summary>
    internal byte InstructionTimer;
    /// <summary>Stores subnote.</summary>
    internal byte Subnote;
    /// <summary>Stores repeat point.</summary>
    internal byte RepeatPoint;
    /// <summary>Stores note.</summary>
    internal byte Note;
    /// <summary>Stores pointer index.</summary>
    internal byte PointerIndex;
    /// <summary>Stores voice index.</summary>
    internal byte VoiceIndex;
    /// <summary>Stores voice index times eight.</summary>
    internal byte VoiceIndexTimesEight;
    /// <summary>Stores release timer.</summary>
    internal byte ReleaseTimer;
    /// <summary>Stores volume.</summary>
    internal byte Volume;
    /// <summary>Stores target note.</summary>
    internal byte TargetNote;
    /// <summary>Stores phase invert.</summary>
    internal byte PhaseInvert;
    /// <summary>Stores pointer.</summary>
    internal ushort Pointer;
    /// <summary>Stores repeat counter.</summary>
    internal byte RepeatCounter;
    /// <summary>Stores enable pitch slide.</summary>
    internal byte EnablePitchSlide;
    /// <summary>Stores release flag.</summary>
    internal byte ReleaseFlag;
    /// <summary>Stores enable pitch slide legato.</summary>
    internal byte EnablePitchSlideLegato;
    /// <summary>Stores adsr settings.</summary>
    internal ushort AdsrSettings;
    /// <summary>Stores channel mask.</summary>
    internal byte ChannelMask;
    /// <summary>Stores legato.</summary>
    internal byte Legato;
    /// <summary>Stores disabled.</summary>
    internal byte Disabled;
    /// <summary>Stores subnote delta.</summary>
    internal byte SubnoteDelta;
    /// <summary>Stores update adsr.</summary>
    internal byte UpdateAdsr;
}
