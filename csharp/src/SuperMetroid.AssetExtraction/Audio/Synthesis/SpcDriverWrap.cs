using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The audio unit's byte and word widths. The SPC sound driver and the S-DSP keep each counter, cursor and accumulator
/// in an 8-bit byte or a 16-bit word, and their arithmetic relies on those values wrapping; the managed player and DSP
/// store them as ints and pass each arithmetic result through these operations, so the wrap is explicit at each write.
/// </summary>
[EncodingOwner("sound driver byte and word widths")]
internal static class SpcDriverWrap
{
    /// <summary>Values one driver byte holds.</summary>
    private const int ByteRange = 0x100;

    /// <summary>Values one driver word holds.</summary>
    private const int WordRange = 0x10000;

    /// <summary><paramref name="value"/> as the driver's 8-bit byte holds it, 0 through 255.</summary>
    public static int Byte(int value) => value & (ByteRange - 1);

    /// <summary><paramref name="value"/> as the driver's 16-bit word holds it, 0 through 65535.</summary>
    public static int Word(int value) => value & (WordRange - 1);
}
