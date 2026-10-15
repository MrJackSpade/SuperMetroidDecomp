using SuperMetroid.Core.Hardware;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

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

    /// <summary>Appends every field of this music voice, in declaration order, to a driver-state snapshot (#1305).</summary>
    internal void WriteState(List<byte> state)
    {
        state.AddRange([Index, NoteTicksLeft, NoteKeyOffTicksLeft, SubroutineLoops, VolumeFadeTicks, PanTicks, PitchSlideLength, PitchSlideDelayLeft, VibratoHoldCount, VibratoDepth, TremoloHoldCount, TremoloDepth, VibratoChangeCount, NoteLength, NoteGateOffFixedPoint, ChannelVolumeMaster, InstrumentId, PitchEnvelopeTicks, PitchEnvelopeDelay, PitchEnvelopeDirection, PitchEnvelopeSlide, VibratoCount, VibratoRate, VibratoDelayTicks, VibratoFadeTicks, VibratoFadeAddPerTick, VibratoDepthTarget, TremoloCount, TremoloRate, TremoloDelayTicks, ChannelTransposition, VolumeFadeTarget, FinalVolume, PanTarget, PanFlags, PitchTarget, FineTune, CutKey]);
        state.Add(SnesLittleEndian.LowByte(PatternOrderPointer));
        state.Add(SnesLittleEndian.HighByte(PatternOrderPointer));
        state.Add(SnesLittleEndian.LowByte(InstrumentPitchBase));
        state.Add(SnesLittleEndian.HighByte(InstrumentPitchBase));
        state.Add(SnesLittleEndian.LowByte(SavedPatternPointer));
        state.Add(SnesLittleEndian.HighByte(SavedPatternPointer));
        state.Add(SnesLittleEndian.LowByte(PatternStartPointer));
        state.Add(SnesLittleEndian.HighByte(PatternStartPointer));
        state.Add(SnesLittleEndian.LowByte(ChannelVolume));
        state.Add(SnesLittleEndian.HighByte(ChannelVolume));
        state.Add(SnesLittleEndian.LowByte(VolumeFadeAddPerTick));
        state.Add(SnesLittleEndian.HighByte(VolumeFadeAddPerTick));
        state.Add(SnesLittleEndian.LowByte(PanValue));
        state.Add(SnesLittleEndian.HighByte(PanValue));
        state.Add(SnesLittleEndian.LowByte(PanAddPerTick));
        state.Add(SnesLittleEndian.HighByte(PanAddPerTick));
        state.Add(SnesLittleEndian.LowByte(Pitch));
        state.Add(SnesLittleEndian.HighByte(Pitch));
        state.Add(SnesLittleEndian.LowByte(PitchAddPerTick));
        state.Add(SnesLittleEndian.HighByte(PitchAddPerTick));
    }

    /// <summary>Copies every field except the hardware voice <see cref="Index"/> from <paramref name="donor"/>, for entry-state analysis (#1305).</summary>
    internal void CopyFrom(ManagedSpcMusicChannel donor)
    {
        PatternOrderPointer = donor.PatternOrderPointer;
        NoteTicksLeft = donor.NoteTicksLeft;
        NoteKeyOffTicksLeft = donor.NoteKeyOffTicksLeft;
        SubroutineLoops = donor.SubroutineLoops;
        VolumeFadeTicks = donor.VolumeFadeTicks;
        PanTicks = donor.PanTicks;
        PitchSlideLength = donor.PitchSlideLength;
        PitchSlideDelayLeft = donor.PitchSlideDelayLeft;
        VibratoHoldCount = donor.VibratoHoldCount;
        VibratoDepth = donor.VibratoDepth;
        TremoloHoldCount = donor.TremoloHoldCount;
        TremoloDepth = donor.TremoloDepth;
        VibratoChangeCount = donor.VibratoChangeCount;
        NoteLength = donor.NoteLength;
        NoteGateOffFixedPoint = donor.NoteGateOffFixedPoint;
        ChannelVolumeMaster = donor.ChannelVolumeMaster;
        InstrumentId = donor.InstrumentId;
        InstrumentPitchBase = donor.InstrumentPitchBase;
        SavedPatternPointer = donor.SavedPatternPointer;
        PatternStartPointer = donor.PatternStartPointer;
        PitchEnvelopeTicks = donor.PitchEnvelopeTicks;
        PitchEnvelopeDelay = donor.PitchEnvelopeDelay;
        PitchEnvelopeDirection = donor.PitchEnvelopeDirection;
        PitchEnvelopeSlide = donor.PitchEnvelopeSlide;
        VibratoCount = donor.VibratoCount;
        VibratoRate = donor.VibratoRate;
        VibratoDelayTicks = donor.VibratoDelayTicks;
        VibratoFadeTicks = donor.VibratoFadeTicks;
        VibratoFadeAddPerTick = donor.VibratoFadeAddPerTick;
        VibratoDepthTarget = donor.VibratoDepthTarget;
        TremoloCount = donor.TremoloCount;
        TremoloRate = donor.TremoloRate;
        TremoloDelayTicks = donor.TremoloDelayTicks;
        ChannelTransposition = donor.ChannelTransposition;
        ChannelVolume = donor.ChannelVolume;
        VolumeFadeAddPerTick = donor.VolumeFadeAddPerTick;
        VolumeFadeTarget = donor.VolumeFadeTarget;
        FinalVolume = donor.FinalVolume;
        PanValue = donor.PanValue;
        PanAddPerTick = donor.PanAddPerTick;
        PanTarget = donor.PanTarget;
        PanFlags = donor.PanFlags;
        Pitch = donor.Pitch;
        PitchAddPerTick = donor.PitchAddPerTick;
        PitchTarget = donor.PitchTarget;
        FineTune = donor.FineTune;
        CutKey = donor.CutKey;
    }
}
