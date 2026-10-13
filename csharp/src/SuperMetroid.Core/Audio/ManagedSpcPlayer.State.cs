using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Audio;

/// <summary>Per-music-voice state from Super Metroid's SPC driver work RAM.</summary>
internal sealed class ManagedSpcMusicChannel
{
    /// <summary>Current byte cursor in this voice's SPC pattern instruction stream.</summary>
    internal ushort PatternOrderPointer;
    /// <summary>Hardware voice index used when selecting the channel's DSP registers.</summary>
    internal byte Index;
    /// <summary>Remaining duration of the current note before reading another pattern command.</summary>
    internal byte NoteTicksLeft;
    /// <summary>Remaining gated portion of the note before the voice receives key off.</summary>
    internal byte NoteKeyOffTicksLeft;
    /// <summary>Repeat count remaining in the active pattern subroutine.</summary>
    internal byte SubroutineLoops;
    /// <summary>Music ticks left before channel volume reaches its fade target.</summary>
    internal byte VolumeFadeTicks;
    /// <summary>Music ticks left before pan reaches its fade target.</summary>
    internal byte PanTicks;
    /// <summary>Music ticks remaining in the current pitch slide after its delay.</summary>
    internal byte PitchSlideLength;
    /// <summary>Ticks to wait before applying the current pitch slide.</summary>
    internal byte PitchSlideDelayLeft;
    /// <summary>Ticks elapsed before vibrato is permitted to modulate pitch.</summary>
    internal byte VibratoHoldCount;
    /// <summary>Current vibrato amplitude, which may ramp toward the requested depth.</summary>
    internal byte VibratoDepth;
    /// <summary>Ticks elapsed before tremolo is permitted to modulate volume.</summary>
    internal byte TremoloHoldCount;
    /// <summary>Amplitude of the channel's periodic volume modulation.</summary>
    internal byte TremoloDepth;
    /// <summary>Vibrato fade steps already applied to the current note.</summary>
    internal byte VibratoChangeCount;
    /// <summary>Pattern-selected duration for subsequent notes until another duration command.</summary>
    internal byte NoteLength;
    /// <summary>Table-derived gate percentage scaled against each note's duration.</summary>
    internal byte NoteGateOffFixedPoint;
    /// <summary>Pattern-selected channel volume used in voice output mixing.</summary>
    internal byte ChannelVolumeMaster;
    /// <summary>Selected instrument restored when an SFX releases this hardware voice.</summary>
    internal byte InstrumentId;
    /// <summary>Instrument-specific base used to convert pattern note numbers to DSP pitch.</summary>
    internal ushort InstrumentPitchBase;
    /// <summary>Instruction cursor restored after a pattern subroutine finishes.</summary>
    internal ushort SavedPatternPointer;
    /// <summary>Start address revisited while a pattern subroutine still has loops remaining.</summary>
    internal ushort PatternStartPointer;
    /// <summary>Duration of a note-on pitch envelope before settling on its target note.</summary>
    internal byte PitchEnvelopeTicks;
    /// <summary>Delay before a note-on pitch envelope starts sliding.</summary>
    internal byte PitchEnvelopeDelay;
    /// <summary>Selects whether the envelope approaches or departs from the note pitch.</summary>
    internal byte PitchEnvelopeDirection;
    /// <summary>Semitone offset applied by a note-on pitch envelope.</summary>
    internal byte PitchEnvelopeSlide;
    /// <summary>Current phase of the vibrato oscillator.</summary>
    internal byte VibratoCount;
    /// <summary>Phase step applied to vibrato on each music tick.</summary>
    internal byte VibratoRate;
    /// <summary>Required note age before vibrato begins.</summary>
    internal byte VibratoDelayTicks;
    /// <summary>Number of steps over which vibrato depth fades in.</summary>
    internal byte VibratoFadeTicks;
    /// <summary>Depth increment used on each vibrato fade step.</summary>
    internal byte VibratoFadeAddPerTick;
    /// <summary>Final vibrato depth selected by the enable effect.</summary>
    internal byte VibratoDepthTarget;
    /// <summary>Current phase of the tremolo oscillator.</summary>
    internal byte TremoloCount;
    /// <summary>Phase step applied to tremolo on each music tick.</summary>
    internal byte TremoloRate;
    /// <summary>Required note age before tremolo begins.</summary>
    internal byte TremoloDelayTicks;
    /// <summary>Per-voice note offset added to the global transposition.</summary>
    internal byte ChannelTransposition;
    /// <summary>Current fixed-point channel volume during a fade.</summary>
    internal ushort ChannelVolume;
    /// <summary>Signed fixed-point volume change applied on each fade tick.</summary>
    internal ushort VolumeFadeAddPerTick;
    /// <summary>Whole-number channel volume requested at fade completion.</summary>
    internal byte VolumeFadeTarget;
    /// <summary>Volume after channel, tremolo, and master factors are combined.</summary>
    internal byte FinalVolume;
    /// <summary>Current fixed-point left-to-right pan position.</summary>
    internal ushort PanValue;
    /// <summary>Signed fixed-point pan change applied on each fade tick.</summary>
    internal ushort PanAddPerTick;
    /// <summary>Whole-number pan position requested at fade completion.</summary>
    internal byte PanTarget;
    /// <summary>Pan effect bits retained when an SFX temporarily borrows this voice.</summary>
    internal byte PanFlags;
    /// <summary>Current fixed-point note pitch before vibrato and DSP scaling.</summary>
    internal ushort Pitch;
    /// <summary>Signed fixed-point pitch increment applied during a slide.</summary>
    internal ushort PitchAddPerTick;
    /// <summary>Whole-number note pitch selected for the end of a slide.</summary>
    internal byte PitchTarget;
    /// <summary>Fractional pitch byte restored when a slide reaches its target.</summary>
    internal byte FineTune;
    /// <summary>Pattern effect that suppresses normal key-off handling for this voice.</summary>
    internal byte CutKey;
}

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
}

/// <summary>One logical SFX stream temporarily borrowing a hardware music voice.</summary>
internal sealed class ManagedSpcSoundChannel
{
    /// <summary>Bit selecting the hardware voice temporarily owned by this SFX stream.</summary>
    internal byte VoiceBitset;
    /// <summary>Ticks until this stream reads its next sound instruction.</summary>
    internal byte InstructionTimer;
    /// <summary>Fractional pitch byte advanced during a sound effect slide.</summary>
    internal byte Subnote;
    /// <summary>Instruction index saved at a sound effect repeat-start command.</summary>
    internal byte RepeatPoint;
    /// <summary>Current whole-number pitch byte of the active SFX note.</summary>
    internal byte Note;
    /// <summary>Byte offset into the current sound effect instruction stream.</summary>
    internal byte PointerIndex;
    /// <summary>Twice the borrowed hardware voice number, matching SPC DSP addressing.</summary>
    internal byte VoiceIndex;
    /// <summary>Eight times the borrowed voice number for per-voice ADSR register writes.</summary>
    internal byte VoiceIndexTimesEight;
    /// <summary>Short key-off delay used before a non-legato stream releases its note.</summary>
    internal byte ReleaseTimer;
    /// <summary>Saved music-channel volume restored when this SFX releases its voice.</summary>
    internal byte Volume;
    /// <summary>Whole-number pitch destination of the current SFX slide.</summary>
    internal byte TargetNote;
    /// <summary>Saved music-channel pan and phase flags restored after SFX playback.</summary>
    internal byte PhaseInvert;
    /// <summary>Address of the current sound effect byte stream.</summary>
    internal ushort Pointer;
    /// <summary>Remaining repetitions after an SFX repeat-start instruction.</summary>
    internal byte RepeatCounter;
    /// <summary>Marks that each SFX tick must advance toward the target note.</summary>
    internal byte EnablePitchSlide;
    /// <summary>Tracks whether the release key-off delay has begun for this note.</summary>
    internal byte ReleaseFlag;
    /// <summary>Retains the slide opcode's legato form until the next pitch update.</summary>
    internal byte EnablePitchSlideLegato;
    /// <summary>Two ADSR bytes supplied by an inline SFX envelope override.</summary>
    internal ushort AdsrSettings;
    /// <summary>Inverse borrowed-voice bit mask used to clear that voice from shared DSP masks.</summary>
    internal byte ChannelMask;
    /// <summary>Lets a sliding note continue without retriggering its hardware voice.</summary>
    internal byte Legato;
    /// <summary>Marks an unallocated or finished SFX channel so its stream does not run.</summary>
    internal byte Disabled;
    /// <summary>Fractional pitch step added or subtracted during each slide tick.</summary>
    internal byte SubnoteDelta;
    /// <summary>Requests writing the inline ADSR override to the borrowed voice.</summary>
    internal byte UpdateAdsr;
}
