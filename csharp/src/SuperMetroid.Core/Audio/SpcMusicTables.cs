namespace SuperMetroid.Core.Audio;

/// <summary>Immutable lookup tables embedded in Super Metroid's uploaded music driver.</summary>
internal static class SpcMusicTables
{
    /// <summary>Number of contiguous native effect opcodes from SetInstrument through SetFastForward.</summary>
    internal const int EffectCount = (int)SpcMusicEffect.SetFastForward - (int)SpcMusicEffect.SetInstrument + 1;

    /// <summary>Returns the operand count for the zero-based native effect index 0..30.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all NTSC J/U v1.0 bytes, pinned
    /// spc_player.c and decoder/codec/lookahead consumers. Named opcode cases express
    /// command format directly; numerical fitting has no meaning here. The decoder
    /// pre-reads one operand when count is nonzero, handlers consume the remainder,
    /// and lookahead skips the whole count. SkipByte thus has count2, while the final
    /// three commands have count0. Preserve IndexOutOfRangeException outside0..30;
    /// callers with explicit InvalidDataException guards retain those guards.
    /// </remarks>
    internal static byte EffectByteLength(int index)
    {
        if ((uint)index >= EffectCount) throw new IndexOutOfRangeException();
        byte opcode = (byte)(index + (int)SpcMusicEffect.SetInstrument);
        return Game.ClosedNativeWords.Decode<SpcMusicEffect>(opcode, "SPC music effect opcode") switch
        {
            SpcMusicEffect.DisableVibrato or SpcMusicEffect.DisableTremolo or
            SpcMusicEffect.DisablePitchEnvelope or SpcMusicEffect.DisableEcho or
            SpcMusicEffect.CutKey or SpcMusicEffect.FastForwardForFrames or
            SpcMusicEffect.SetFastForward => 0,
            SpcMusicEffect.SetInstrument or SpcMusicEffect.SetPan or
            SpcMusicEffect.SetMasterVolume or SpcMusicEffect.SetTempo or
            SpcMusicEffect.SetGlobalTransposition or SpcMusicEffect.SetChannelTransposition or
            SpcMusicEffect.SetChannelVolume or SpcMusicEffect.FadeVibrato or
            SpcMusicEffect.SetFineTune or SpcMusicEffect.SetPercussionBase => 1,
            SpcMusicEffect.FadePan or SpcMusicEffect.FadeMasterVolume or
            SpcMusicEffect.FadeTempo or SpcMusicEffect.FadeChannelVolume or
            SpcMusicEffect.SkipByte => 2,
            SpcMusicEffect.EnableVibrato or SpcMusicEffect.EnableTremolo or
            SpcMusicEffect.CallPattern or SpcMusicEffect.PitchEnvelopeTo or
            SpcMusicEffect.PitchEnvelopeFrom or SpcMusicEffect.EnableEcho or
            SpcMusicEffect.ConfigureEcho or SpcMusicEffect.FadeEchoVolume or
            SpcMusicEffect.PitchSlide => 3,
            _ => throw new IndexOutOfRangeException(),
        };
    }
    /// <summary>SPC1E1D..1E31 contains21 pan samples. The curve's exact generator remains
    /// required review under1165; no retention exception has been established.</summary>
    internal const int PanSampleCount = 21;
    private static readonly byte[] panVolume =
        [0, 1, 3, 7, 13, 21, 30, 41, 52, 66, 81, 94, 103, 110, 115, 119, 122, 124, 125, 126, 127];

    /// <summary>Native local interpolation view, indices0..21. Index21 is the adjacent
    /// sharp-echo FIR preset's first coefficient atSPC1E32/CF:8A3A: maximal positive
    /// signed gain127, not another pan-curve sample. Preserve this bounded stock alias;
    /// the caller's pan indices21..255 continue to read mutable SPC RAM separately.</summary>
    internal static byte PanVolume(int index)
    {
        if ((uint)index > PanSampleCount) throw new IndexOutOfRangeException();
        return index == PanSampleCount ? (byte)sbyte.MaxValue : panVolume[index];
    }

    /// <summary>Returns the one-octave pitch basis for semitone 0..12, including the next C.</summary>
    /// <remarks>
    /// Independently verified for #1165 against all original NTSC words and the native
    /// WritePitchInner consumer. Floor(440*8.192*2^((n-9)/12)) explains every word.
    /// Evaluate from A440 before quantizing; scaling an already-rounded C loses precision.
    /// The decimal semitone ratio is the lower 28-place bound of the twelfth root of two,
    /// independently bracketed by integer powers in VerifySpcPitchBasisAlgorithm.
    /// Decimal multiplication/division is deterministic; at most nine steps are required.
    /// All thirteen results remain between the same integer boundaries using either root
    /// bound. Caller-owned byte-delta interpolation, octave shifts and scaling stay separate.
    /// </remarks>
    internal static ushort BaseNoteFrequency(int semitone)
    {
        if ((uint)semitone > 12) throw new IndexOutOfRangeException();
        const decimal semitoneRatio = 1.0594630943592952645618252949m;
        decimal frequency = 440m * 8192m / 1000m;
        for (int note = 9; note < semitone; note++) frequency *= semitoneRatio;
        for (int note = semitone; note < 9; note++) frequency /= semitoneRatio;
        return (ushort)frequency;
    }

    /// <summary>Returns the exact volume byte for note-command low nibble 0..15.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against every NTSC J/U v1.0 byte, pinned
    /// spc_player.c kNoteVol, and the decoder/scaling consumers. Percentages are
    /// 10,20,30,40 then 45..95 by fives, ending at99. Quantize with (255*p-1)/100:
    /// exact multiples round from below, consistent with truncating 2.55 to Q16
    /// before multiplication. That explains the values without asserting the historical
    /// generator. Preserve byte quantization before subsequent channel-volume scaling.
    /// </remarks>
    internal static byte NoteVolume(int index)
    {
        if ((uint)index >= 16) throw new IndexOutOfRangeException();
        int percentage = index < 4 ? 10 * (index + 1) : index == 15 ? 99 : 5 * (index + 5);
        return (byte)((255 * percentage - 1) / 100);
    }

    /// <summary>Returns the exact gate byte for note-command bits4..6, index0..7.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against every NTSC byte and pinned kNoteGateOffPct.
    /// Percentages are20,40 then50..90 by tens, ending at99; use the same lower-side
    /// (255*p-1)/100 quantization as NoteVolume. Decoder masking is caller-owned.
    /// Keep the terminal252 and multiply by note ticks only after this quantization.
    /// </remarks>
    internal static byte NoteGateOffPercentage(int index)
    {
        if ((uint)index >= 8) throw new IndexOutOfRangeException();
        int percentage = index < 2 ? 20 * (index + 1) : index == 7 ? 99 : 10 * (index + 3);
        return (byte)((255 * percentage - 1) / 100);
    }
}
