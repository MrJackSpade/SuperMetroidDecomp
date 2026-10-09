namespace SuperMetroid.Core.Assets;

/// <summary>
/// Body-fade endpoint paint at $A6:E454-E469. First eight words exactly alias
/// Golden Torizo low-front armor $84:8034-8043; use its immutable stock provider.
/// Three remaining wing membrane paints at $A6:E464-E469 share saturated red and
/// blue-minus-green tint5. Selected green18/1 and dark red20/blue4 (dark green zero)
/// define categorical membrane highlight/middle/shadow; replacing these levels
/// invents different paint, without exempting initial palettes, pixels or timing.
/// </summary>
internal sealed class CeresRidleyBodyPaintDefinitions
{
    /// <summary>Maximum value of one five-bit RGB channel.</summary>
    private const int MaximumChannel = (1 << 5) - 1;

    /// <summary>Green channel of the authored highlight membrane color.</summary>
    private readonly int highlightGreen;

    /// <summary>Green channel of the authored middle membrane color.</summary>
    private readonly int middleGreen;

    /// <summary>Highlight blue-minus-green offset used to preserve the membrane's blue tint at each shade.</summary>
    private readonly int blueTint;

    /// <summary>Red channel of the authored dark membrane color.</summary>
    private readonly int darkRed;

    /// <summary>Blue channel of the authored dark membrane color.</summary>
    private readonly int darkBlue;

    /// <summary>Builds the membrane paint levels from the final three colors of an eleven-word endpoint.</summary>
    /// <param name="endpoint">Endpoint palette containing the eight aliased armor colors followed by the three membrane levels.</param>
    internal CeresRidleyBodyPaintDefinitions(ReadOnlySpan<ushort> endpoint)
        : this(EndpointColor(endpoint, 8), EndpointColor(endpoint, 9), EndpointColor(endpoint, 10)) { }

    /// <summary>Shared membrane material, also present at $A6:E1F9-E1FE in the Baby palette.</summary>
    /// <param name="highlight">Packed RGB5 highlight color defining the upper membrane level and tint.</param>
    /// <param name="middle">Packed RGB5 middle color defining the intermediate green level.</param>
    /// <param name="dark">Packed RGB5 shadow color supplying the dark red and blue channels.</param>
    internal CeresRidleyBodyPaintDefinitions(ushort highlight, ushort middle, ushort dark)
    {
        highlightGreen = highlight >> 5 & MaximumChannel;
        middleGreen = middle >> 5 & MaximumChannel;
        blueTint = (highlight >> 10) - highlightGreen;
        darkRed = dark & MaximumChannel;
        darkBlue = dark >> 10;
    }

    /// <summary>Returns one color from an endpoint after requiring the exact eleven-color palette shape.</summary>
    /// <param name="endpoint">Palette endpoint to read.</param>
    /// <param name="index">Zero-based color index in the endpoint.</param>
    /// <returns>The packed RGB5 color at <paramref name="index"/>.</returns>
    private static ushort EndpointColor(ReadOnlySpan<ushort> endpoint, int index)
    {
        if (endpoint.Length != 11) throw new ArgumentException("Ridley body endpoint requires eleven colors.", nameof(endpoint));
        return endpoint[index];
    }
    /// <summary>Resolves one of eight aliased armor colors or one of the three synthesized membrane shades.</summary>
    /// <param name="color">Zero-based color index in the eleven-word Ridley body endpoint.</param>
    /// <returns>The packed RGB5 word selected for that endpoint index.</returns>
    internal ushort Color(int color)
    {
        if ((uint)color >= 11) throw new ArgumentOutOfRangeException(nameof(color));
        if (color < 8) return GoldenTorizoHealthPaintDefinitions.Color(0, color + 1, rear: false);
        if (color == 10) return (ushort)(darkRed | darkBlue << 10);
        int green = color == 8 ? highlightGreen : middleGreen;
        int blue = Math.Clamp(green + blueTint, 0, MaximumChannel);
        return (ushort)(MaximumChannel | green << 5 | blue << 10);
    }
}
