namespace SuperMetroid.Core.Assets;

/// <summary>
/// One plain colour (#1420, #1544): red, green and blue, each from none (0) to full (<see cref="FullChannel"/>), as the
/// display shows it. A colour is made from and read as its channels, and moved by colour operations: a blend part of the
/// way toward another colour, a colour scaled toward black, a colour added to another. The game's authored colours, and
/// the few places whose timing counts a colour in the levels the game steps it through, use
/// <see cref="FadeLevelColour"/>; the colour itself holds no levels and no stored form.
/// </summary>
public readonly record struct RgbColor
{
    /// <summary>A channel at full strength.</summary>
    public const int FullChannel = 255;

    /// <summary>Makes a colour from its three channels, each 0-<see cref="FullChannel"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A channel is outside 0-<see cref="FullChannel"/>.</exception>
    public RgbColor(int red, int green, int blue)
    {
        Red = Channel(red, nameof(red));
        Green = Channel(green, nameof(green));
        Blue = Channel(blue, nameof(blue));
    }

    /// <summary>Black: every channel none.</summary>
    public static RgbColor Black => default;

    /// <summary>White: every channel full.</summary>
    public static RgbColor White => new(FullChannel, FullChannel, FullChannel);

    /// <summary>The red channel, 0-<see cref="FullChannel"/>.</summary>
    public int Red { get; }

    /// <summary>The green channel, 0-<see cref="FullChannel"/>.</summary>
    public int Green { get; }

    /// <summary>The blue channel, 0-<see cref="FullChannel"/>.</summary>
    public int Blue { get; }

    /// <summary>
    /// This colour blended <paramref name="part"/> of <paramref name="parts"/> of the way toward <paramref name="toward"/>:
    /// none of the way is this colour, all of it is <paramref name="toward"/>, and each channel between is rounded toward
    /// this colour.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parts"/> is not positive or <paramref name="part"/> is outside 0-<paramref name="parts"/>.</exception>
    public RgbColor Blend(RgbColor toward, int part, int parts)
    {
        _ = Ensure.BetweenInclusive(1, int.MaxValue, parts);
        _ = Ensure.BetweenInclusive(0, parts, part);
        return new(Blended(Red, toward.Red, part, parts), Blended(Green, toward.Green, part, parts),
            Blended(Blue, toward.Blue, part, parts));
    }

    /// <summary>
    /// This colour scaled to <paramref name="part"/> of <paramref name="parts"/> of its strength: none is black, all of it
    /// is this colour, and each channel between is rounded down.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parts"/> is not positive or <paramref name="part"/> is outside 0-<paramref name="parts"/>.</exception>
    public RgbColor Scaled(int part, int parts)
    {
        _ = Ensure.BetweenInclusive(1, int.MaxValue, parts);
        _ = Ensure.BetweenInclusive(0, parts, part);
        return new(Red * part / parts, Green * part / parts, Blue * part / parts);
    }

    /// <summary>This colour with <paramref name="added"/> added to it, each channel held at full.</summary>
    public RgbColor Plus(RgbColor added) =>
        new(Math.Min(FullChannel, Red + added.Red), Math.Min(FullChannel, Green + added.Green),
            Math.Min(FullChannel, Blue + added.Blue));

    /// <summary>The host pixel showing this colour, opaque.</summary>
    public RgbaPixel ToRgbaPixel() => new((byte)Red, (byte)Green, (byte)Blue);

    /// <inheritdoc />
    public override string ToString() => $"Rgb(r{Red}, g{Green}, b{Blue})";

    /// <summary>One channel <paramref name="part"/> of <paramref name="parts"/> of the way toward <paramref name="toward"/>, rounded toward <paramref name="from"/>.</summary>
    private static int Blended(int from, int toward, int part, int parts) => from + (toward - from) * part / parts;

    /// <summary>Rejects a channel outside 0-<see cref="FullChannel"/>.</summary>
    private static int Channel(int value, string name) =>
        value is >= 0 and <= FullChannel
            ? value
            : throw new ArgumentOutOfRangeException(name, value, $"A colour channel is 0-{FullChannel}.");
}
