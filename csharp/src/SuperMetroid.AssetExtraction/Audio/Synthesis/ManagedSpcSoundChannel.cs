namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One logical SFX stream temporarily borrowing a hardware music voice.</summary>
internal sealed class ManagedSpcSoundChannel
{
    /// <summary>Bit selecting the hardware voice temporarily owned by this SFX stream.</summary>
    internal int VoiceBitset;
    /// <summary>Ticks until this stream reads its next sound instruction.</summary>
    internal int FrameHold;
    /// <summary>Fractional pitch byte advanced during a sound effect slide.</summary>
    internal int Subnote;
    /// <summary>Instruction index saved at a sound effect repeat-start command.</summary>
    internal int RepeatPoint;
    /// <summary>Current whole-number pitch byte of the active SFX note.</summary>
    internal int Note;
    /// <summary>Byte offset into the current sound effect instruction stream.</summary>
    internal int PointerIndex;
    /// <summary>Twice the borrowed hardware voice number, matching SPC DSP addressing.</summary>
    internal int VoiceIndex;
    /// <summary>Eight times the borrowed voice number for per-voice ADSR register writes.</summary>
    internal int VoiceIndexTimesEight;
    /// <summary>Short key-off delay used before a non-legato stream releases its note.</summary>
    internal int ReleaseTimer;
    /// <summary>Saved music-channel volume restored when this SFX releases its voice.</summary>
    internal int Volume;
    /// <summary>Whole-number pitch destination of the current SFX slide.</summary>
    internal int TargetNote;
    /// <summary>Saved music-channel pan and phase flags restored after SFX playback.</summary>
    internal int PhaseInvert;
    /// <summary>Address of the current sound effect byte stream.</summary>
    internal int Pointer;
    /// <summary>Remaining repetitions after an SFX repeat-start instruction.</summary>
    internal int RepeatCounter;
    /// <summary>How far the channel's pitch slide has gone: off, sliding, or sliding legato (#2560).</summary>
    internal SoundEffectSlide Slide;
    /// <summary>Whether the release key-off delay has begun for this note.</summary>
    internal bool Releasing;
    /// <summary>The form the last slide instruction took, retained until the next one (#2560).</summary>
    internal SoundEffectSlideForm SlideForm;
    /// <summary>Two ADSR bytes supplied by an inline SFX envelope override.</summary>
    internal int AdsrSettings;
    /// <summary>Inverse borrowed-voice bit mask used to clear that voice from shared DSP masks.</summary>
    internal int ChannelMask;
    /// <summary>Whether the channel is unallocated or finished, so its stream does not run.</summary>
    internal bool Disabled;
    /// <summary>Fractional pitch step added or subtracted during each slide tick.</summary>
    internal int SubnoteDelta;
    /// <summary>Whether each note writes the inline ADSR override to the borrowed voice.</summary>
    internal bool UpdateAdsr;

    /// <summary>Appends every field of this sound-effect channel, in declaration order, to a driver-state snapshot (#1305).</summary>
    internal void WriteState(List<byte> state)
    {
        SpcDriverSnapshot.AddBytes(state, [VoiceBitset, FrameHold, Subnote, RepeatPoint, Note, PointerIndex, VoiceIndex, VoiceIndexTimesEight, ReleaseTimer, Volume, TargetNote, PhaseInvert, RepeatCounter,
            SpcDriverSnapshot.Mark(Slide != SoundEffectSlide.Off), SpcDriverSnapshot.Mark(Releasing), SpcDriverSnapshot.Byte(SlideForm), ChannelMask,
            SpcDriverSnapshot.Mark(Slide == SoundEffectSlide.SlidingLegato), SpcDriverSnapshot.Mark(Disabled), SubnoteDelta, SpcDriverSnapshot.Mark(UpdateAdsr)]);
        SpcDriverSnapshot.AddWords(state, [Pointer, AdsrSettings]);
    }
}
