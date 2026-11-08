namespace SuperMetroid.Core.Assets;

/// <summary>
/// One plain colour (#1420, #1544): red, green and blue, each from none (0) to full (<see cref="FullChannel"/>), as the
/// display shows it. A colour is made from and read as its channels. Every colour the game shows lies on its fade scale
/// (<see cref="FadeLevelColour"/>): a fade, flash, glow or ramp computes its steps on those levels and shows the plain
/// colour of the level it reaches, so the colour itself holds no levels, no stored form and no colour arithmetic of its own.
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

    /// <summary>The host pixel showing this colour, opaque.</summary>
    public RgbaPixel ToRgbaPixel() => new((byte)Red, (byte)Green, (byte)Blue); // cartridge-encoding: allow(HostPixelFormat) - each channel is held to 0-255 by the constructor, and the host RGBA8888 pixel stores it in one byte

    /// <inheritdoc />
    public override string ToString() => $"Rgb(r{Red}, g{Green}, b{Blue})";

    /// <summary>Rejects a channel outside 0-<see cref="FullChannel"/>.</summary>
    private static int Channel(int value, string name) =>
        value is >= 0 and <= FullChannel
            ? value
            : throw new ArgumentOutOfRangeException(name, value, $"A colour channel is 0-{FullChannel}.");
}
