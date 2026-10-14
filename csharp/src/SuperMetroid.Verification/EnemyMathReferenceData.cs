/// <summary>Pinned cartridge addresses used as independent enemy trigonometry oracles.</summary>
internal static class EnemyMathReferenceData
{
    /// <summary>Pinned $A0:B443-$B642 sign-extended 8.8 sine words.</summary>
    public const int SignedSine = 0xa0b443;
    /// <summary>Pinned $A0:B1C3 signed 16-bit sine/cosine quadrants.</summary>
    public const int SignedSixteenBitSine = 0xa0b1c3;
    /// <summary>Pinned $AA:E03D Shaktool negative-cosine prefix and sine quadrants.</summary>
    public const int ShaktoolOrbit = 0xaae03d;
    /// <summary>Pinned $A0:B3C3 negative-cosine prefix followed by the full signed sine wave.</summary>
    public const int SignedNegativeCosine = 0xa0b3c3;
    /// <summary>Pinned $A0:B143 positive byte sine/cosine sample range.</summary>
    public const int ByteSine = 0xa0b143;
    /// <summary>Pinned $A0:B7EE UnsignedSineTable.</summary>
    public const int UnsignedSine = 0xa0b7ee;
}
