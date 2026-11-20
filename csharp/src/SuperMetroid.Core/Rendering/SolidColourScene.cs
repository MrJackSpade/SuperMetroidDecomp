using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>A frame filled with one colour (#2505), shown at its brightness.</summary>
public sealed class SolidColourScene : DisplayScene
{
    /// <summary>Keeps the colour and the brightness.</summary>
    /// <param name="colour">The colour filling the whole screen before the brightness applies.</param>
    /// <param name="brightness">The frame's brightness, from black (zero) to <see cref="DisplayBrightness.Full"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The brightness is below black or above full.</exception>
    public SolidColourScene(RgbaPixel colour, int brightness = DisplayBrightness.Full)
        : base(brightness) => Colour = colour;

    /// <summary>The colour filling the whole screen before the brightness applies.</summary>
    public RgbaPixel Colour { get; }

    /// <inheritdoc/>
    public override void DrawWith<TDrawer>(TDrawer drawer) => drawer.DrawSolidColour(this);

    /// <inheritdoc/>
    internal override DisplayScene AtBrightness(int brightness) => new SolidColourScene(Colour, brightness);
}
