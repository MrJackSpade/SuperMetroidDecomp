using SuperMetroid.Core.Hardware;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Per-music-voice state from Super Metroid's SPC driver work RAM.</summary>
internal sealed class ManagedSpcMusicChannel
{
    /// <summary>Current byte cursor in this voice's SPC pattern instruction stream.</summary>
    internal int PatternOrderPointer;
    /// <summary>Hardware voice index used when selecting the channel's DSP registers.</summary>
    internal int Index;
    /// <summary>Remaining duration of the current note before reading another pattern command.</summary>
    internal int NoteTicksLeft;
    /// <summary>Remaining gated portion of the note before the voice receives key off.</summary>
    internal int NoteKeyOffTicksLeft;
    /// <summary>Repeat count remaining in the active pattern subroutine.</summary>
    internal int SubroutineLoops;
    /// <summary>Music ticks left before channel volume reaches its fade target.</summary>
    internal int VolumeFadeTicks;
    /// <summary>Music ticks left before pan reaches its fade target.</summary>
    internal int PanTicks;
    /// <summary>Music ticks remaining in the current pitch slide after its delay.</summary>
    internal int PitchSlideLength;
    /// <summary>Ticks to wait before applying the current pitch slide.</summary>
    internal int PitchSlideDelayLeft;
    /// <summary>Ticks elapsed before vibrato is permitted to modulate pitch.</summary>
    internal int VibratoHoldCount;
    /// <summary>Current vibrato amplitude, which may ramp toward the requested depth.</summary>
    internal int VibratoDepth;
    /// <summary>Ticks elapsed before tremolo is permitted to modulate volume.</summary>
    internal int TremoloHoldCount;
    /// <summary>Amplitude of the channel's periodic volume modulation.</summary>
    internal int TremoloDepth;
    /// <summary>Vibrato fade steps already applied to the current note.</summary>
    internal int VibratoChangeCount;
    /// <summary>Pattern-selected duration for subsequent notes until another duration command.</summary>
    internal int NoteLength;
    /// <summary>Table-derived gate percentage scaled against each note's duration.</summary>
    internal int NoteGateOffFixedPoint;
    /// <summary>Pattern-selected channel volume used in voice output mixing.</summary>
    internal int ChannelVolumeMaster;
    /// <summary>Selected instrument restored when an SFX releases this hardware voice.</summary>
    internal int InstrumentId;
    /// <summary>Instrument-specific base used to convert pattern note numbers to DSP pitch.</summary>
    internal int InstrumentPitchBase;
    /// <summary>Instruction cursor restored after a pattern subroutine finishes.</summary>
    internal int SavedPatternPointer;
    /// <summary>Start address revisited while a pattern subroutine still has loops remaining.</summary>
    internal int PatternStartPointer;
    /// <summary>Duration of a note-on pitch envelope before settling on its target note.</summary>
    internal int PitchEnvelopeTicks;
    /// <summary>Delay before a note-on pitch envelope starts sliding.</summary>
    internal int PitchEnvelopeDelay;
    /// <summary>Selects whether the envelope approaches or departs from the note pitch.</summary>
    internal int PitchEnvelopeDirection;
    /// <summary>Semitone offset applied by a note-on pitch envelope.</summary>
    internal int PitchEnvelopeSlide;
    /// <summary>Current phase of the vibrato oscillator.</summary>
    internal int VibratoCount;
    /// <summary>Phase step applied to vibrato on each music tick.</summary>
    internal int VibratoRate;
    /// <summary>Required note age before vibrato begins.</summary>
    internal int VibratoDelayTicks;
    /// <summary>Number of steps over which vibrato depth fades in.</summary>
    internal int VibratoFadeTicks;
    /// <summary>Depth increment used on each vibrato fade step.</summary>
    internal int VibratoFadeAddPerTick;
    /// <summary>Final vibrato depth selected by the enable effect.</summary>
    internal int VibratoDepthTarget;
    /// <summary>Current phase of the tremolo oscillator.</summary>
    internal int TremoloCount;
    /// <summary>Phase step applied to tremolo on each music tick.</summary>
    internal int TremoloRate;
    /// <summary>Required note age before tremolo begins.</summary>
    internal int TremoloDelayTicks;
    /// <summary>Per-voice note offset added to the global transposition.</summary>
    internal int ChannelTransposition;
    /// <summary>Current fixed-point channel volume during a fade.</summary>
    internal int ChannelVolume;
    /// <summary>Signed fixed-point volume change applied on each fade tick.</summary>
    internal int VolumeFadeAddPerTick;
    /// <summary>Whole-number channel volume requested at fade completion.</summary>
    internal int VolumeFadeTarget;
    /// <summary>Volume after channel, tremolo, and master factors are combined.</summary>
    internal int FinalVolume;
    /// <summary>Current fixed-point left-to-right pan position.</summary>
    internal int PanValue;
    /// <summary>Signed fixed-point pan change applied on each fade tick.</summary>
    internal int PanAddPerTick;
    /// <summary>Whole-number pan position requested at fade completion.</summary>
    internal int PanTarget;
    /// <summary>Pan effect bits retained when an SFX temporarily borrows this voice.</summary>
    internal int PanFlags;
    /// <summary>Current fixed-point note pitch before vibrato and DSP scaling.</summary>
    internal int Pitch;
    /// <summary>Signed fixed-point pitch increment applied during a slide.</summary>
    internal int PitchAddPerTick;
    /// <summary>Whole-number note pitch selected for the end of a slide.</summary>
    internal int PitchTarget;
    /// <summary>Fractional pitch byte restored when a slide reaches its target.</summary>
    internal int FineTune;
    /// <summary>Pattern effect that suppresses normal key-off handling for this voice.</summary>
    internal int CutKey;

    /// <summary>Appends every field of this music voice, in declaration order, to a driver-state snapshot (#1305).</summary>
    internal void WriteState(List<byte> state)
    {
        SpcDriverSnapshot.AddBytes(state, [Index, NoteTicksLeft, NoteKeyOffTicksLeft, SubroutineLoops, VolumeFadeTicks, PanTicks, PitchSlideLength, PitchSlideDelayLeft, VibratoHoldCount, VibratoDepth, TremoloHoldCount, TremoloDepth, VibratoChangeCount, NoteLength, NoteGateOffFixedPoint, ChannelVolumeMaster, InstrumentId, PitchEnvelopeTicks, PitchEnvelopeDelay, PitchEnvelopeDirection, PitchEnvelopeSlide, VibratoCount, VibratoRate, VibratoDelayTicks, VibratoFadeTicks, VibratoFadeAddPerTick, VibratoDepthTarget, TremoloCount, TremoloRate, TremoloDelayTicks, ChannelTransposition, VolumeFadeTarget, FinalVolume, PanTarget, PanFlags, PitchTarget, FineTune, CutKey]);
        SpcDriverSnapshot.AddWords(state, [PatternOrderPointer, InstrumentPitchBase, SavedPatternPointer, PatternStartPointer, ChannelVolume, VolumeFadeAddPerTick, PanValue, PanAddPerTick, Pitch, PitchAddPerTick]);
    }
}
