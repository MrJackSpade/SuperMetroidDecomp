namespace SuperMetroid.Core.Assets;

/// <summary>
/// One game colour (#1420): three five-bit channels, red, green and blue, each 0 through <see cref="MaxChannel"/>,
/// the precision every authored palette and colour effect works in. A colour is made from and read as its channels.
/// </summary>
/// <remarks>
/// The channels are stored packed into one field, which is this type's private storage and nothing else: no member
/// exposes it. Decoding and encoding a stored colour word belong to the import (<c>ColourWordCodec</c>), never to Core.
/// </remarks>
[EncodingOwner("five-bit channels packed into the colour's own storage")]
public readonly record struct RgbColor
{
    /// <summary>The largest value of one five-bit channel.</summary>
    public const int MaxChannel = 31;

    /// <summary>The colour's three five-bit channels, red lowest, as validated.</summary>
    private readonly ushort channels;

    /// <summary>Packs three channels, each 0-<see cref="MaxChannel"/>.</summary>
    public RgbColor(int red, int green, int blue) => channels = Pack(red, green, blue);

    /// <summary>Black: every channel zero.</summary>
    public static RgbColor Black => default;

    /// <summary>White: every channel at <see cref="MaxChannel"/>.</summary>
    public static RgbColor White => new(MaxChannel, MaxChannel, MaxChannel);

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

    /// <summary>Host RGBA, expanding each five-bit channel to eight bits by bit replication.</summary>
    public RgbaPixel ToRgbaPixel() => new(Expand(Red), Expand(Green), Expand(Blue));

    /// <inheritdoc />
    public override string ToString() => $"Rgb(r{Red}, g{Green}, b{Blue})";

    /// <summary>Validates and packs three five-bit channels into the colour's storage.</summary>
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
            throw new ArgumentOutOfRangeException(name, value, $"A colour channel is 0-{MaxChannel}.");
    }

    /// <summary>
    /// Replicates the high three bits into the low end, mapping a five-bit channel's 0..31 across the full
    /// 0..255 display range including both endpoints.
    /// </summary>
    private static byte Expand(int channel) => (byte)((channel << 3) | (channel >> 2));
}
