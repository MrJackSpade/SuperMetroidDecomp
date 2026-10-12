using SuperMetroid.Core.Hardware;

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
    private const int MaximumChannel = (1 << 5) - 1;
    private readonly int highlightGreen;
    private readonly int middleGreen;
    private readonly int blueTint;
    private readonly int darkRed;
    private readonly int darkBlue;

    internal CeresRidleyBodyPaintDefinitions(ReadOnlySpan<Bgr555> endpoint)
        : this(EndpointColor(endpoint, 8), EndpointColor(endpoint, 9), EndpointColor(endpoint, 10)) { }

    /// <summary>Shared membrane material, also present at $A6:E1F9-E1FE in the Baby palette.</summary>
    internal CeresRidleyBodyPaintDefinitions(Bgr555 highlight, Bgr555 middle, Bgr555 dark)
    {
        highlightGreen = highlight.Green;
        middleGreen = middle.Green;
        blueTint = highlight.Blue - highlightGreen;
        darkRed = dark.Red;
        darkBlue = dark.Blue;
    }

    private static Bgr555 EndpointColor(ReadOnlySpan<Bgr555> endpoint, int index)
    {
        if (endpoint.Length != 11) throw new ArgumentException("Ridley body endpoint requires eleven colors.", nameof(endpoint));
        return endpoint[index];
    }
    internal Bgr555 Color(int color)
    {
        if ((uint)color >= 11) throw new ArgumentOutOfRangeException(nameof(color));
        if (color < 8) return GoldenTorizoHealthPaintDefinitions.Color(0, color + 1, rear: false);
        if (color == 10) return new(darkRed, 0, darkBlue);
        int green = color == 8 ? highlightGreen : middleGreen;
        int blue = Math.Clamp(green + blueTint, 0, MaximumChannel);
        return new(MaximumChannel, green, blue);
    }
}
