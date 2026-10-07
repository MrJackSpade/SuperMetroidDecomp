namespace SuperMetroid.Core.Game;

/// <summary>Immutable integer samples used by the shared enemy multiplication routines.
/// These are engine math definitions, not editable presentation assets.</summary>
public static class EnemyTrigonometryTables
{
    /// <summary>$A0:B1C3-$B3C2, SineCosineTables_16bitSine and its three
    /// quadrant continuations. These signed samples peak at +/-32767, unlike
    /// the sign-extended 8.8 table. The positive half equals the stored unsigned
    /// half-wave shifted right once; sign is restored only after that truncation.</summary>
    /// <remarks>Independently reviewed for #1165; keep this existing algorithm: for byte angle a, let n = a &amp; 127 and
    /// U(n) = floor(65535*sin(n*pi/128)). The exact result is floor(U(n)/2),
    /// negated only when a &gt;= 128. All 256 words match the NTSC J/U v1.0 ROM
    /// and pinned bank_A0.asm. This preserves
    /// the half-unit scale (32767.5), rather than assuming a scale of 32767.
    /// The #1165 research evaluator used bounded decimal arithmetic, not Math.Sin.
    /// Bull adds a quarter-turn for X, while Yapping Maw negates and narrows its
    /// input angle before sampling. Individual investigation: #625 / #909.
    /// </remarks>
    public static short SignedSixteenBitSine(byte angle)
    {
        int magnitude = UnsignedHalfWave(angle & 127) >> 1;
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
    /// <remarks>Independently reviewed for #1165; keep this existing algorithm: sign(a)*floor(256*sin((a &amp; 127)*pi/128)),
    /// with positive sign for a &lt; 128, reproduces the native words. Treat the
    /// quarter-turn magnitude as exactly 256; the byte table saturates it to 255.
    /// The #1165 research also checked the entire 320-word negative-cosine prefix/full
    /// sine view. Any later replacement must preserve PhantoonWaveRomData's
    /// separate odd-byte composition and final $8B instruction-byte overread;
    /// these are byte-addressing behavior, not additional sine samples.
    /// Individual investigation: #625 / #910.</remarks>
    public static short SignedSine(byte angle)
    {
        int halfWaveIndex = angle & 127;
        int magnitude = halfWaveIndex == 64 ? 256 : EightBitHalfWave(halfWaveIndex);
        return (short)(angle < 128 ? magnitude : -magnitude);
    }

    /// <summary>$A0:B3C3-$B642, SineCosineTables_NegativeCosine_SignExtended
    /// followed by the full signed sine wave. The 320-word range admits byte angle
    /// plus a 64-word offset; do not truncate the supplied index before validation.</summary>
    /// <remarks>
    /// Physical view of the same signed 8.8 cycle as <see cref="SignedSine"/>:
    /// word index 0..319 maps to byte angle <c>(index - 64) &amp; 255</c>.
    /// Independently reviewed for #1165 against all 320 original words, prefix bounds
    /// and byte-phase aliases. Keep this existing view; do not duplicate its storage.
    /// </remarks>
    public static short SignedNegativeCosineWord(int index)
    {
        if ((uint)index >= 320) throw new ArgumentOutOfRangeException(nameof(index));
        return SignedSine(unchecked((byte)(index - 64)));
    }

    /// <summary>$A0:B143 SineCosineTables_8bitSine and cosine continuation:
    /// min(255, floor(256*sin(index*pi/128))) for index 0..127.</summary>
    /// <remarks>
    /// Independently checked for #1165 against all 128 NTSC bytes and bank_A0.asm.
    /// Scale 256 before truncation; only the exact quarter-turn saturates to 255.
    /// Invalid indices preserve the former span's IndexOutOfRangeException.
    /// Sign, radius truncation and separate whole/fraction negation stay in callers.
    /// </remarks>
    public static byte EightBitHalfWave(int index)
    {
        if ((uint)index >= 128) throw new IndexOutOfRangeException();
        return (byte)Math.Min(255, (int)(256 * UnitHalfWave(index)));
    }

    /// <summary>$A0:B7EE UnsignedSineTable:
    /// floor(65535*sin(index*pi/128)) for index 0..127.</summary>
    /// <remarks>
    /// Independently checked for #1165 against all 128 original NTSC words and
    /// pinned bank_A0.asm. Scale 65535, not saturated 65536; preserve quantization
    /// before multiplication and before SignedSixteenBitSine's shift and sign.
    /// This identifies the exact numerical convention, not the historical authoring tool.
    /// Invalid indices preserve the former span's IndexOutOfRangeException.
    /// </remarks>
    public static ushort UnsignedHalfWave(int index)
    {
        if ((uint)index >= 128) throw new IndexOutOfRangeException();
        return (ushort)(65535 * UnitHalfWave(index));
    }

    // Reflect to [0, pi/2], with exact endpoints so integer boundaries cannot drift.
    // Twelve alternating Taylor terms leave an omitted term below 5.2e-21;
    // 1e-19 covers that remainder, decimal rounding and the 28-place pi constant.
    // Full-domain ROM proofs establish both scales' exact integer results. This
    // allocation-free recurrence avoids libm and per-sample correction constants.
    internal static decimal UnitHalfWave(int index)
    {
        if ((uint)index >= 128) throw new IndexOutOfRangeException();
        int phase = Math.Min(index, 128 - index);
        if (phase == 0) return 0;
        if (phase == 64) return 1;
        decimal x = phase * 3.1415926535897932384626433833m / 128;
        decimal squared = x * x;
        decimal term = x, sum = x;
        for (int k = 1; k < 12; k++)
        {
            term = -term * squared / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return sum;
    }
}
