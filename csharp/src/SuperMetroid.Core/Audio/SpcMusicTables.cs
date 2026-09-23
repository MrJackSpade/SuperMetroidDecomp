namespace SuperMetroid.Core.Audio;

/// <summary>Immutable lookup tables embedded in Super Metroid's uploaded music driver.</summary>
internal static class SpcMusicTables
{
    /// <summary>Argument-byte count for opcodes $E0-$FE.</summary>
    internal static readonly byte[] EffectByteLengths =
    [
        1, 1, 2, 3, 0, 1, 2, 1, 2, 1, 1, 3, 0, 1, 2, 3,
        1, 3, 3, 0, 1, 3, 0, 3, 3, 3, 1, 2, 0, 0, 0,
    ];

    /// <summary>Nonlinear pan curve sampled at integer positions zero through 21.</summary>
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

    /// <summary>Sixteen authored note-volume percentage selections, encoded as byte fractions.</summary>
    /// <remarks>
    /// Issue #625 exact recipe: for index i=0..15 choose p=10*(i+1) for i&lt;4,
    /// p=5*(i+5) for 4..14, and p=99 for i=15. Then return (255*p-1)/100
    /// using integer division. This lower-side quantization is also reproduced by
    /// truncating 255/100 to Q16 BEFORE multiplying: ((255*65536/100)*p)&gt;&gt;16.
    /// The two recipes agree for every integer percentage 1..100. Ordinary
    /// floor(255*p/100) is one too high at 20/40/60/80 percent. Thus finite
    /// intermediate precision explains the discrepancies without individual sample
    /// exceptions; Q16 is a verified model, not an identification of the authoring
    /// hardware. LookupTableResearch checks all 16 values against kNoteVol in
    /// pinned spc_player.c, NTSC ROM $CF:80F4, and this array, plus the entire
    /// percentage domain. Keep the authored 99-percent endpoint and input bounds.
    /// Runtime migration and performance checks are deferred.
    /// </remarks>
    internal static readonly byte[] NoteVolumes =
        [25, 50, 76, 101, 114, 127, 140, 152, 165, 178, 191, 203, 216, 229, 242, 252];

    /// <summary>Eight authored gate-length percentage selections, encoded as byte fractions.</summary>
    /// <remarks>
    /// Issue #625 exact recipe: validate i=0..7; p=20*(i+1) for i&lt;2,
    /// p=10*(i+3) for 2..6, and p=99 for i=7. Return (255*p-1)/100.
    /// The same Q16-before-multiply model described on NoteVolumes reproduces
    /// all eight values, including the one-unit lower exact-multiple boundaries.
    /// LookupTableResearch checks kNoteGateOffPct in pinned spc_player.c, NTSC
    /// ROM $CF:80EC and this array. Do not replace the terminal 252 with 255 or
    /// move the note-length multiplication before percentage quantization.
    /// </remarks>
    internal static readonly byte[] NoteGateOffPercentages =
        [50, 101, 127, 152, 178, 203, 229, 252];

    static SpcMusicTables()
    {
        if (EffectByteLengths.Length != 31 || BaseNoteFrequencies.Length != 13)
        {
            throw new InvalidDataException("One or more fixed SPC music tables have an invalid length.");
        }
    }
}
