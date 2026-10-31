namespace SuperMetroid.Core.Assets;

/// <summary>
/// One game colour (#1420): three five-bit channels, red, green and blue, each 0 through <see cref="MaxChannel"/>,
/// the precision every authored palette and colour effect works in. Packed as a word, red takes bits 0-4, green bits
/// 5-9 and blue bits 10-14; bit 15 is not a colour bit.
/// </summary>
/// <remarks>
/// The packed word is reachable only through explicit <see cref="FromWord"/>/<see cref="ToWord"/>
/// conversions at the installed palette documents' and serialization boundaries. No implicit numeric
/// conversion exists, so the representation cannot leak back into colour logic.
/// This type is the single owner of the RGB5 colour form: channel packing, the bit-15 rule and the
/// palette-port masking all live here, so its stored word is that form.
/// </remarks>
[EncodingOwner("RGB5 colour word of the installed palette documents")]
public readonly record struct RgbColor
{
    /// <summary>Bytes in one encoded color word, as installed artwork stores it.</summary>
    public const int ByteCount = sizeof(ushort);

    /// <summary>The largest value of one five-bit channel.</summary>
    public const int MaxChannel = 31;

    /// <summary>Mask for the fifteen CGRAM color bits, excluding the unused high bit.</summary>
    private const ushort ColorBits = (1 << 15) - 1;

    /// <summary>The colour's three five-bit channels, red lowest, as validated.</summary>
    private readonly ushort channels;

    /// <summary>Accepts a previously validated or CGRAM-masked packed word.</summary>
    private RgbColor(ushort word) => channels = word;

    /// <summary>Packs three channels, each 0-<see cref="MaxChannel"/>.</summary>
    public RgbColor(int red, int green, int blue)
        : this(Pack(red, green, blue))
    {
    }

    /// <summary>Black: every channel zero.</summary>
    public static RgbColor Black => default;

    /// <summary>White: every channel at <see cref="MaxChannel"/>.</summary>
    public static RgbColor White => new(ColorBits);

    /// <summary>The red channel, 0-<see cref="MaxChannel"/>.</summary>
    public int Red => channels & MaxChannel;

    /// <summary>The green channel, 0-<see cref="MaxChannel"/>.</summary>
    public int Green => (channels >> 5) & MaxChannel;

    /// <summary>The blue channel, 0-<see cref="MaxChannel"/>.</summary>
    public int Blue => (channels >> 10) & MaxChannel;

    /// <summary>One channel, 0-<see cref="MaxChannel"/>.</summary>
    public int this[ColorChannel channel] => channel switch
    {
        ColorChannel.Red => Red,
        ColorChannel.Green => Green,
        ColorChannel.Blue => Blue,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>This color with one channel replaced.</summary>
    public RgbColor With(ColorChannel channel, int value) => channel switch
    {
        ColorChannel.Red => WithRed(value),
        ColorChannel.Green => WithGreen(value),
        ColorChannel.Blue => WithBlue(value),
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>This color with its red channel replaced.</summary>
    public RgbColor WithRed(int red) => new(red, Green, Blue);

    /// <summary>This color with its green channel replaced.</summary>
    public RgbColor WithGreen(int green) => new(Red, green, Blue);

    /// <summary>This color with its blue channel replaced.</summary>
    public RgbColor WithBlue(int blue) => new(Red, Green, blue);

    /// <summary>
    /// Packs three channels, saturating each into 0-<see cref="MaxChannel"/>. For paint
    /// arithmetic whose authored or native rule clamps; strict packing uses the constructor.
    /// </summary>
    public static RgbColor Saturating(int red, int green, int blue) =>
        new(Math.Clamp(red, 0, MaxChannel), Math.Clamp(green, 0, MaxChannel), Math.Clamp(blue, 0, MaxChannel));

    /// <summary>Applies <paramref name="map"/> to each channel; the results must be valid channels.</summary>
    public RgbColor Map(Func<ColorChannel, int, int> map)
    {
        Ensure.NotNull(map);
        return new(map(ColorChannel.Red, Red), map(ColorChannel.Green, Green), map(ColorChannel.Blue, Blue));
    }

    /// <summary>Combines matching channels of this color and <paramref name="other"/>; the results must be valid channels.</summary>
    public RgbColor Zip(RgbColor other, Func<ColorChannel, int, int, int> combine)
    {
        Ensure.NotNull(combine);
        return new(combine(ColorChannel.Red, Red, other.Red), combine(ColorChannel.Green, Green, other.Green),
            combine(ColorChannel.Blue, Blue, other.Blue));
    }

    /// <summary>Whether <paramref name="word"/> is a stored color: bit 15, which is not a color bit, is clear.</summary>
    public static bool IsColorWord(ushort word) => (word & ~ColorBits) == 0;

    /// <summary>
    /// Decodes a stored 15-bit color word. A set bit 15 is rejected: it is not part of any
    /// color, and only <see cref="FromWordIgnoringTopBit"/> defines a result for it.
    /// </summary>
    public static RgbColor FromWord(ushort word)
    {
        if ((word & ~ColorBits) != 0)
            throw new ArgumentOutOfRangeException(nameof(word), word, $"BGR555 word ${word:X4} sets bit 15, which is not a color bit.");
        return new(word);
    }

    /// <summary>
    /// The colour a packed word shows when its top bit, which is not a colour bit, may be set: the top bit is dropped. A
    /// colour step that carries out of the blue channel, and a stored word loaded as a palette, show this colour, as the
    /// original palette hardware kept only fifteen bits.
    /// </summary>
    public static RgbColor FromWordIgnoringTopBit(int word) => new((ushort)(word & ColorBits));

    /// <summary>The packed 15-bit word, for ROM, memory, CGRAM and serialization boundaries.</summary>
    public ushort ToWord() => channels;

    /// <summary>Host RGBA, expanding each five-bit channel to eight bits by bit replication.</summary>
    public RgbaPixel ToRgbaPixel() => new(Expand(Red), Expand(Green), Expand(Blue));

    /// <inheritdoc />
    public override string ToString() => $"BGR555(r{Red}, g{Green}, b{Blue}; ${channels:X4})";

    /// <summary>Validates and packs three five-bit channels into a SNES color word.</summary>
    private static ushort Pack(int red, int green, int blue)
    {
        Channel(red, nameof(red));
        Channel(green, nameof(green));
        Channel(blue, nameof(blue));
        return (ushort)(red | (green << 5) | (blue << 10));
    }

    /// <summary>Rejects a component outside the five-bit channel range before packing.</summary>
    private static void Channel(int value, string name)
    {
        if ((uint)value > MaxChannel)
            throw new ArgumentOutOfRangeException(name, value, $"A BGR555 channel is 0-{MaxChannel}.");
    }

    /// <summary>
    /// Replicates the high three bits into the low end, mapping SNES 0..31 across the full
    /// 0..255 display range including both endpoints.
    /// </summary>
    private static byte Expand(int channel) => (byte)((channel << 3) | (channel >> 2));
}
