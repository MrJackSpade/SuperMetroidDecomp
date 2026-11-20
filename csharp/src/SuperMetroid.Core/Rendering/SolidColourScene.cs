using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>A frame filled with one colour (#2505), shown at its brightness.</summary>
public sealed class SolidColourScene : DisplayScene
{
    /// <summary>Keeps the colour and the brightness.</summary>
    /// <param name="colour">The colour filling the whole screen before the brightness applies.</param>
    /// <param name="brightness">The frame's brightness.</param>
    public SolidColourScene(RgbaPixel colour, DisplayBrightness brightness)
        : base(brightness) => Colour = colour;

    /// <summary>Keeps the colour, shown undimmed.</summary>
    /// <param name="colour">The colour filling the whole screen.</param>
    public SolidColourScene(RgbaPixel colour)
        : this(colour, DisplayBrightness.Full)
    {
    }

    /// <summary>The colour filling the whole screen before the brightness applies.</summary>
    public RgbaPixel Colour { get; }

    /// <inheritdoc/>
    public override void DrawWith<TDrawer>(TDrawer drawer) => drawer.DrawSolidColour(this);

    /// <inheritdoc/>
    internal override DisplayScene AtBrightness(DisplayBrightness brightness) => new SolidColourScene(Colour, brightness);
}
