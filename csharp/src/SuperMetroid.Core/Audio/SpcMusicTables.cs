namespace SuperMetroid.Core.Audio;

/// <summary>Immutable lookup tables embedded in Super Metroid's uploaded music driver.</summary>
internal static class SpcMusicTables
{
    /// <summary>Argument-byte count for opcodes $E0-$FE.</summary>
    /// <remarks>
    /// Issues #625 and #923 retain this 31-entry command-format catalog. The
    /// pinned NTSC J/U v1.0 ROM at $CF:87A8 (file $2787A8) matches every byte
    /// of kEffectByteLength in upstream-sm/src/spc_player.c and this array.
    /// Each entry is the authored operand count for one opcode, not a sampled
    /// numeric function: adjacent opcodes have unrelated semantics and arities.
    /// The native decoder reads the first operand when the count is nonzero,
    /// then individual handlers consume remaining operands; key-off lookahead
    /// skips the entire count. A replacement must preserve all 31 mappings,
    /// reject opcodes outside $E0-$FE, and retain those distinct read paths.
    /// No shorter generator is supported by the command format evidence.
    /// </remarks>
    internal static readonly byte[] EffectByteLengths =
    [
        1, 1, 2, 3, 0, 1, 2, 1, 2, 1, 1, 3, 0, 1, 2, 3,
        1, 3, 3, 0, 1, 3, 0, 3, 3, 3, 1, 2, 0, 0, 0,
    ];

    /// <summary>Nonlinear pan curve sampled at integer positions zero through 21.</summary>
    /// <remarks>
    /// Issues #625 and #924 retain the 22 authored samples. The pinned NTSC
    /// J/U v1.0 ROM at $CF:8A25 (file $278A25) matches every byte of
    /// kVolumeTable in upstream-sm/src/spc_player.c and this array. No
    /// independently evidenced analytic or integer generator reproduces the
    /// quantized curve, so a fitted function would only restate these samples.
    /// The native driver interpolates adjacent entries using the low byte of
    /// the pan position, then mirrors the position around $1400 for the other
    /// stereo side. It deliberately reads SPC RAM beyond this local table for
    /// integer positions 21 and above; those address-level reads are separate
    /// from the 22-byte catalog and must not be replaced by clamping.
    /// </remarks>
    internal static readonly byte[] PanVolume =
        [0, 1, 3, 7, 13, 21, 30, 41, 52, 66, 81, 94, 103, 110, 115, 119, 122, 124, 125, 126, 127, 127];

    /// <summary>One-octave pitch basis plus the next C used for fractional interpolation.</summary>
    /// <remarks>
    /// Issues #625 and #918 exact formula: for semitone n=0..12,
    /// floor(440*8.192*2^((n-9)/12)).
    /// This is A440 equal temperament with the driver's pitch scaling, evaluated
    /// BEFORE rounding the C basis. Using 2143*2^(n/12) instead fails at n=7/8.
    /// csharp/tools/LookupTableResearch proves all 13 integers without floating
    /// point: find the greatest k with k^12*1000^12*2^9 &lt;= (440*8192)^12*2^n.
    /// It also proves k+1 fails that inequality. The oracle is kBaseNoteFreqs in
    /// pinned upstream-sm/src/spc_player.c, independently matched to NTSC J/U v1.0
    /// ROM $CF:8A6E (file $278A6E), and this array. A later bounded replacement
    /// must reject n outside 0..12 and retain the driver's subsequent byte-sized
    /// interpolation, octave shifts and instrument scaling. The integer-root proof
    /// is research code, not a proposed hot-path implementation or benchmark.
    /// </remarks>
    internal static readonly ushort[] BaseNoteFrequencies =
        [2143, 2270, 2405, 2548, 2700, 2860, 3030, 3211, 3402, 3604, 3818, 4045, 4286];

    /// <summary>$CF:80F4 uploaded driver kNoteVol, sixteen unsigned volume fractions.</summary>
    internal const int NoteVolumeReferenceAddress = 0xcf80f4;
    /// <summary>$CF:80EC uploaded driver kNoteGateOffPct, eight unsigned gate fractions.</summary>
    internal const int NoteGateReferenceAddress = 0xcf80ec;

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
    static SpcMusicTables()
    {
        if (EffectByteLengths.Length != 31 || BaseNoteFrequencies.Length != 13)
        {
            throw new InvalidDataException("One or more fixed SPC music tables have an invalid length.");
        }
    }
}
