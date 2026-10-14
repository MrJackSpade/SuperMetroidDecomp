namespace SuperMetroid.Core.Audio;

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
