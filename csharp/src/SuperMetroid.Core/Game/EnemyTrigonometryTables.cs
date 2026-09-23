namespace SuperMetroid.Core.Game;

/// <summary>Immutable integer samples used by the shared enemy multiplication routines.
/// These are engine math definitions, not editable presentation assets.</summary>
public static class EnemyTrigonometryTables
{
    /// <summary>$A0:B1C3-$B3C2, SineCosineTables_16bitSine and its three
    /// quadrant continuations. These signed samples peak at +/-32767, unlike
    /// the sign-extended 8.8 table. The positive half equals the stored unsigned
    /// half-wave shifted right once; sign is restored only after that truncation.</summary>
    /// <remarks>Issue #625 research: for byte angle a, let n = a &amp; 127 and
    /// U(n) = floor(65535*sin(n*pi/128)). The exact result is floor(U(n)/2),
    /// negated only when a &gt;= 128. All 256 words match the NTSC J/U v1.0 ROM
    /// and pinned bank_A0.asm in csharp/tools/LookupTableResearch. This preserves
    /// the half-unit scale (32767.5), rather than assuming a scale of 32767.
    /// The research evaluator uses bounded decimal arithmetic, not Math.Sin.
    /// Bull adds a quarter-turn for X, while Yapping Maw negates and narrows its
    /// input angle before sampling. Individual investigation: #625 / #909.
    /// </remarks>
    public static short SignedSixteenBitSine(byte angle)
    {
        int magnitude = UnsignedHalfWave[angle & 127] >> 1;
        return (short)(angle < 128 ? magnitude : -magnitude);
    }

    /// <summary>$86:C26C/$C27A, CalculateSine/Cosine: multiply unsigned speed by
    /// the $A0:B443 signed sample, keep product bits 8..23, then restore the sign.
    /// Callers add a quarter-turn themselves when selecting cosine.</summary>
    public static ushort MultiplySignedSine(ushort speed, byte angle)
    {
        short sample = SignedSine(angle);
        int magnitude = speed * Math.Abs((int)sample) >> 8;
        return unchecked((ushort)(sample < 0 ? -magnitude : magnitude));
    }

    /// <summary>$A0:B443-$B642, SineCosineTables_8bitSine_SignExtended and
    /// its three quadrant continuations. Unlike the byte table, peaks are +/-256.</summary>
    /// <remarks>Issue #625 research: sign(a)*floor(256*sin((a &amp; 127)*pi/128)),
    /// with positive sign for a &lt; 128, reproduces the native words. Treat the
    /// quarter-turn magnitude as exactly 256; the byte table saturates it to 255.
    /// LookupTableResearch checks the entire 320-word negative-cosine prefix/full
    /// sine view as well. Any later replacement must preserve PhantoonWaveRomData's
    /// separate odd-byte composition and final $8B instruction-byte overread;
    /// these are byte-addressing behavior, not additional sine samples.
    /// Individual investigation: #625 / #910.</remarks>
    public static short SignedSine(byte angle)
    {
        int halfWaveIndex = angle & 127;
        int magnitude = halfWaveIndex == 64 ? 256 : EightBitHalfWave[halfWaveIndex];
        return (short)(angle < 128 ? magnitude : -magnitude);
    }

    /// <summary>$A0:B3C3-$B642, SineCosineTables_NegativeCosine_SignExtended
    /// followed by the full signed sine wave. The 320-word range admits byte angle
    /// plus a 64-word offset; do not truncate the supplied index before validation.</summary>
    /// <remarks>
    /// Physical view of the same signed 8.8 cycle as <see cref="SignedSine"/>:
    /// word index 0..319 maps to byte angle <c>(index - 64) &amp; 255</c>.
    /// All 320 words match pinned ROM and assembly. Investigation: #625 / #910.
    /// </remarks>
    public static short SignedNegativeCosineWord(int index)
    {
        if ((uint)index >= 320) throw new ArgumentOutOfRangeException(nameof(index));
        return SignedSine(unchecked((byte)(index - 64)));
    }

    /// <summary>$A0:B143, SineCosineTables_8bitSine and its cosine continuation.
    /// The positive half-wave peaks at 255, not 256. Callers supply sign separately.</summary>
    /// <remarks>
    /// Issue #625 algorithm finding: for integer n in 0..127, every byte equals
    /// min(255, floor(256*sin(n*pi/128))). Scaling by 255 is not equivalent.
    /// csharp/tools/LookupTableResearch exhaustively compares all 128 values to
    /// this span, the SHA-256-pinned NTSC J/U v1.0 ROM, and pinned bank_A0.asm.
    /// Its deterministic candidate reflects n to min(n,128-n), handles 0 and 64
    /// exactly, and evaluates twelve decimal Taylor terms through x^23/23! with
    /// x = n*pi/128. Pi is 3.1415926535897932384626433833m; each remaining scaled
    /// result has the same floor at both ends of a conservative +/-256e-19
    /// error interval. Thus no platform Math.Sin rounding is required for parity.
    /// Reject indices outside 0..127 before reflection; caller angle wrapping
    /// remains a separate operation. Production migration and hot-path cost have
    /// not been evaluated; the table is intentionally retained for a later pass.
    /// Byte-table investigation: #625 / #907. Signed and 16-bit physical
    /// sine tables require their own bounded proofs despite sharing phase.
    /// </remarks>
    public static ReadOnlySpan<byte> EightBitHalfWave =>
    [
        0x00,0x06,0x0c,0x12,0x19,0x1f,0x25,0x2b,0x31,0x38,0x3e,0x44,0x4a,0x50,0x56,0x5c,
        0x61,0x67,0x6d,0x73,0x78,0x7e,0x83,0x88,0x8e,0x93,0x98,0x9d,0xa2,0xa7,0xab,0xb0,
        0xb5,0xb9,0xbd,0xc1,0xc5,0xc9,0xcd,0xd1,0xd4,0xd8,0xdb,0xde,0xe1,0xe4,0xe7,0xea,
        0xec,0xee,0xf1,0xf3,0xf4,0xf6,0xf8,0xf9,0xfb,0xfc,0xfd,0xfe,0xfe,0xff,0xff,0xff,
        0xff,0xff,0xff,0xff,0xfe,0xfe,0xfd,0xfc,0xfb,0xf9,0xf8,0xf6,0xf4,0xf3,0xf1,0xee,
        0xec,0xea,0xe7,0xe4,0xe1,0xde,0xdb,0xd8,0xd4,0xd1,0xcd,0xc9,0xc5,0xc1,0xbd,0xb9,
        0xb5,0xb0,0xab,0xa7,0xa2,0x9d,0x98,0x93,0x8e,0x88,0x83,0x7e,0x78,0x73,0x6d,0x67,
        0x61,0x5c,0x56,0x50,0x4a,0x44,0x3e,0x38,0x31,0x2b,0x25,0x1f,0x19,0x12,0x0c,0x06,
    ];

    /// <summary>$A0:B7EE, UnsignedSineTable: 128 samples of the positive half-wave
    /// scaled to 65535. Preserve the stored truncation rather than evaluating Math.Sin.</summary>
    /// <remarks>
    /// Issue #625 algorithm finding: floor(65535*sin(n*pi/128)) matches all 128
    /// words for n in 0..127. A scale of 65536, even with a saturated peak, differs
    /// at 84 indices. LookupTableResearch checks the exact 65535 formula against
    /// this span, the NTSC J/U v1.0 ROM and pinned bank_A0.asm using the same
    /// reflected twelve-term decimal evaluator described on EightBitHalfWave.
    /// Handle n=0 and n=64 exactly; for every other index, both endpoints of the
    /// scaled +/-65535e-19 error interval truncate to the same stored integer.
    /// This is an exhaustively verified deterministic candidate, not evidence of
    /// the original author's generator. A replacement must reject indices outside
    /// 0..127 and preserve callers' multiplication/truncation/sign order; benchmarks
    /// and consumer migration are deferred. Run: dotnet run --project
    /// csharp/tools/LookupTableResearch (repository root, local retail ROM required).
    /// Individual investigation: #625 / #908. The separate signed 16-bit
    /// table is derived by shifting each unsigned sample before applying sign.
    /// </remarks>
    public static ReadOnlySpan<ushort> UnsignedHalfWave =>
    [
        0x0000,0x0648,0x0c8f,0x12d5,0x1917,0x1f56,0x258f,0x2bc3,
        0x31f1,0x3816,0x3e33,0x4447,0x4a4f,0x504d,0x563e,0x5c21,
        0x61f7,0x67bd,0x6d73,0x7319,0x78ac,0x7e2e,0x839b,0x88f5,
        0x8e39,0x9367,0x987f,0x9d7f,0xa266,0xa735,0xabea,0xb085,
        0xb504,0xb967,0xbdae,0xc1d7,0xc5e3,0xc9d0,0xcd9e,0xd14c,
        0xd4da,0xd847,0xdb93,0xdebd,0xe1c4,0xe4a9,0xe76a,0xea08,
        0xec82,0xeed7,0xf108,0xf313,0xf4f9,0xf6b9,0xf852,0xf9c6,
        0xfb13,0xfc3a,0xfd39,0xfe12,0xfec3,0xff4d,0xffb0,0xffeb,
        0xffff,0xffeb,0xffb0,0xff4d,0xfec3,0xfe12,0xfd39,0xfc3a,
        0xfb13,0xf9c6,0xf852,0xf6b9,0xf4f9,0xf313,0xf108,0xeed7,
        0xec82,0xea08,0xe76a,0xe4a9,0xe1c4,0xdebd,0xdb93,0xd847,
        0xd4da,0xd14c,0xcd9e,0xc9d0,0xc5e3,0xc1d7,0xbdae,0xb967,
        0xb504,0xb085,0xabea,0xa735,0xa266,0x9d7f,0x987f,0x9367,
        0x8e39,0x88f5,0x839b,0x7e2e,0x78ac,0x7319,0x6d73,0x67bd,
        0x61f7,0x5c21,0x563e,0x504d,0x4a4f,0x4447,0x3e33,0x3816,
        0x31f1,0x2bc3,0x258f,0x1f56,0x1917,0x12d5,0x0c8f,0x0648,
    ];
}
