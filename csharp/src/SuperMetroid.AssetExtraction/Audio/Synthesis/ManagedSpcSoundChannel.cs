using SuperMetroid.Core.Hardware;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One logical SFX stream temporarily borrowing a hardware music voice.</summary>
internal sealed class ManagedSpcSoundChannel
{
    /// <summary>Bit selecting the hardware voice temporarily owned by this SFX stream.</summary>
    internal int VoiceBitset;
    /// <summary>Ticks until this stream reads its next sound instruction.</summary>
    internal int InstructionTimer;
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
    /// <summary>Marks that each SFX tick must advance toward the target note.</summary>
    internal int EnablePitchSlide;
    /// <summary>Tracks whether the release key-off delay has begun for this note.</summary>
    internal int ReleaseFlag;
    /// <summary>Retains the slide opcode's legato form until the next pitch update.</summary>
    internal int EnablePitchSlideLegato;
    /// <summary>Two ADSR bytes supplied by an inline SFX envelope override.</summary>
    internal int AdsrSettings;
    /// <summary>Inverse borrowed-voice bit mask used to clear that voice from shared DSP masks.</summary>
    internal int ChannelMask;
    /// <summary>Lets a sliding note continue without retriggering its hardware voice.</summary>
    internal int Legato;
    /// <summary>Marks an unallocated or finished SFX channel so its stream does not run.</summary>
    internal int Disabled;
    /// <summary>Fractional pitch step added or subtracted during each slide tick.</summary>
    internal int SubnoteDelta;
    /// <summary>Requests writing the inline ADSR override to the borrowed voice.</summary>
    internal int UpdateAdsr;

    /// <summary>Appends every field of this sound-effect channel, in declaration order, to a driver-state snapshot (#1305).</summary>
    internal void WriteState(List<byte> state)
    {
        SpcDriverSnapshot.AddBytes(state, [VoiceBitset, InstructionTimer, Subnote, RepeatPoint, Note, PointerIndex, VoiceIndex, VoiceIndexTimesEight, ReleaseTimer, Volume, TargetNote, PhaseInvert, RepeatCounter, EnablePitchSlide, ReleaseFlag, EnablePitchSlideLegato, ChannelMask, Legato, Disabled, SubnoteDelta, UpdateAdsr]);
        SpcDriverSnapshot.AddWords(state, [Pointer, AdsrSettings]);
    }
}
