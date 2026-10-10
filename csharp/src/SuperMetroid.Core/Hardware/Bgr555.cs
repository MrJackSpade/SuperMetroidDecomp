using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One SNES color: three five-bit channels packed as the 15-bit CGRAM word, red in bits 0-4,
/// green in bits 5-9 and blue in bits 10-14 (despite the conventional "BGR" name, read
/// from the little-endian word's low end). Bit 15 is not a color bit.
/// </summary>
/// <remarks>
/// The packed word is reachable only through explicit <see cref="FromWord"/>/<see cref="ToWord"/>
/// conversions at ROM, memory, CGRAM-port and serialization boundaries. No implicit numeric
/// conversion exists, so the representation cannot leak back into color logic.
/// </remarks>
public readonly record struct Bgr555
{
    /// <summary>Bytes in one encoded color word, as stored in ROM, WRAM and CGRAM.</summary>
    public const int ByteCount = sizeof(ushort);

    /// <summary>The largest value of one five-bit channel.</summary>
    public const int MaxChannel = 31;

    private const ushort ColorBits = 0x7fff;

    private readonly ushort word;

    private Bgr555(ushort word) => this.word = word;

    /// <summary>Packs three channels, each 0-<see cref="MaxChannel"/>.</summary>
    public Bgr555(int red, int green, int blue)
        : this(Pack(red, green, blue))
    {
    }

    /// <summary>Black: every channel zero.</summary>
    public static Bgr555 Black => default;

    /// <summary>White: every channel at <see cref="MaxChannel"/>.</summary>
    public static Bgr555 White => new(ColorBits);

    /// <summary>The red channel, 0-<see cref="MaxChannel"/>.</summary>
    public int Red => word & MaxChannel;

    /// <summary>The green channel, 0-<see cref="MaxChannel"/>.</summary>
    public int Green => (word >> 5) & MaxChannel;

    /// <summary>The blue channel, 0-<see cref="MaxChannel"/>.</summary>
    public int Blue => (word >> 10) & MaxChannel;

    /// <summary>One channel, 0-<see cref="MaxChannel"/>.</summary>
    public int this[ColorChannel channel] => channel switch
    {
        ColorChannel.Red => Red,
        ColorChannel.Green => Green,
        ColorChannel.Blue => Blue,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>This color with one channel replaced.</summary>
    public Bgr555 With(ColorChannel channel, int value) => channel switch
    {
        ColorChannel.Red => WithRed(value),
        ColorChannel.Green => WithGreen(value),
        ColorChannel.Blue => WithBlue(value),
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>This color with its red channel replaced.</summary>
    public Bgr555 WithRed(int red) => new(red, Green, Blue);

    /// <summary>This color with its green channel replaced.</summary>
    public Bgr555 WithGreen(int green) => new(Red, green, Blue);

    /// <summary>This color with its blue channel replaced.</summary>
    public Bgr555 WithBlue(int blue) => new(Red, Green, blue);

    /// <summary>
    /// Packs three channels, saturating each into 0-<see cref="MaxChannel"/>. For paint
    /// arithmetic whose authored or native rule clamps; strict packing uses the constructor.
    /// </summary>
    public static Bgr555 Saturating(int red, int green, int blue) =>
        new(Math.Clamp(red, 0, MaxChannel), Math.Clamp(green, 0, MaxChannel), Math.Clamp(blue, 0, MaxChannel));

    /// <summary>Applies <paramref name="map"/> to each channel; the results must be valid channels.</summary>
    public Bgr555 Map(Func<ColorChannel, int, int> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return new(map(ColorChannel.Red, Red), map(ColorChannel.Green, Green), map(ColorChannel.Blue, Blue));
    }

    /// <summary>Combines matching channels of this color and <paramref name="other"/>; the results must be valid channels.</summary>
    public Bgr555 Zip(Bgr555 other, Func<ColorChannel, int, int, int> combine)
    {
        ArgumentNullException.ThrowIfNull(combine);
        return new(combine(ColorChannel.Red, Red, other.Red), combine(ColorChannel.Green, Green, other.Green),
            combine(ColorChannel.Blue, Blue, other.Blue));
    }

    /// <summary>
    /// Decodes a stored 15-bit color word. A set bit 15 is rejected: it is not part of any
    /// color, and only the CGRAM data port (<see cref="FromCgramPortWord"/>) defines a result for it.
    /// </summary>
    public static Bgr555 FromWord(ushort word)
    {
        if ((word & ~ColorBits) != 0)
            throw new ArgumentOutOfRangeException(nameof(word), word, $"BGR555 word ${word:X4} sets bit 15, which is not a color bit.");
        return new(word);
    }

    /// <summary>
    /// Decodes a word written through the CGRAM data port (<c>$2122</c>). The PPU stores only
    /// fifteen bits, so the port discards bit 15 of the second byte.
    /// </summary>
    public static Bgr555 FromCgramPortWord(ushort word) => new((ushort)(word & ColorBits));

    /// <summary>The packed 15-bit word, for ROM, memory, CGRAM and serialization boundaries.</summary>
    public ushort ToWord() => word;

    /// <summary>Host RGBA, expanding each five-bit channel to eight bits by bit replication.</summary>
    public Rgba32 ToRgba32() => new(Expand(Red), Expand(Green), Expand(Blue));

    /// <inheritdoc />
    public override string ToString() => $"BGR555(r{Red}, g{Green}, b{Blue}; ${word:X4})";

    private static ushort Pack(int red, int green, int blue)
    {
        Channel(red, nameof(red));
        Channel(green, nameof(green));
        Channel(blue, nameof(blue));
        return (ushort)(red | (green << 5) | (blue << 10));
    }

    private static void Channel(int value, string name)
    {
        if ((uint)value > MaxChannel)
            throw new ArgumentOutOfRangeException(name, value, $"A BGR555 channel is 0-{MaxChannel}.");
    }

    // Replicate the high three bits into the low end rather than merely shifting. This maps
    // SNES 0..31 exactly onto the full 0..255 display range, including both endpoints.
    private static byte Expand(int channel) => (byte)((channel << 3) | (channel >> 2));
}

/// <summary>The three five-bit channels of a <see cref="Bgr555"/> color, in word order.</summary>
public enum ColorChannel : byte
{
    /// <summary>Bits 0-4.</summary>
    Red = 0,
    /// <summary>Bits 5-9.</summary>
    Green = 1,
    /// <summary>Bits 10-14.</summary>
    Blue = 2,
}
