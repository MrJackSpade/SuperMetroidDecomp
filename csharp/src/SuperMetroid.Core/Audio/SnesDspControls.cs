namespace SuperMetroid.Core.Audio;

/// <summary>The S-DSP's global and per-voice registers as typed controls (#631).</summary>
[DspRegisterBoundary("typed controls: S-DSP register file")]
internal readonly struct SnesDspControls(ManagedSnesDsp dsp)
{
    /// <summary>The global (non-voice) registers.</summary>
    public SnesDspGlobalControls Global => new(dsp);

    /// <summary>Voice <paramref name="index"/> (0-7).</summary>
    public SnesDspVoiceControls Voice(int index) => new(dsp, index);

    /// <summary>The EDL ($7D) echo delay steps a stored EDL byte selects (its low four bits).</summary>
    public static int EchoDelayOf(byte echoDelay) => echoDelay & SnesDspRegisterLayout.Fields.EchoDelayMask;
}
