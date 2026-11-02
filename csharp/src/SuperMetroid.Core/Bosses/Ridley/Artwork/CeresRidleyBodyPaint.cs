using SuperMetroid.Core.Enemies.Artwork;

using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.Ridley.Artwork.CeresRidleyBodyPaintDefinitions;
namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Body-fade endpoint paint. First eight words exactly alias
/// Golden Torizo's low-front armor colours; use its immutable stock provider.
/// Three remaining wing membrane paints share saturated red and
/// blue-minus-green tint5. Selected green18/1 and dark red20/blue4 (dark green zero)
/// define categorical membrane highlight/middle/shadow; replacing these levels
/// invents different paint, without exempting initial palettes, pixels or timing.
/// </summary>
internal sealed class CeresRidleyBodyPaint
{
    /// <summary>Green channel of the highlighted wing membrane endpoint.</summary>
    private readonly int highlightGreen;
    /// <summary>Green channel of the middle wing membrane endpoint.</summary>
    private readonly int middleGreen;
    /// <summary>Blue excess over green shared by the highlighted and middle membrane paints.</summary>
    private readonly int blueTint;
    /// <summary>Red channel of the darkest membrane paint.</summary>
    private readonly int darkRed;
    /// <summary>Blue channel of the darkest membrane paint.</summary>
    private readonly int darkBlue;

    /// <summary>Shared membrane material, also present in the Baby palette.</summary>
    internal CeresRidleyBodyPaint(RgbColor highlight, RgbColor middle, RgbColor dark)
    {
        highlightGreen = highlight.Green;
        middleGreen = middle.Green;
        blueTint = highlight.Blue - highlightGreen;
        darkRed = dark.Red;
        darkBlue = dark.Blue;
    }

    /// <summary>Uses Golden Torizo armor for inks zero through seven and the derived wing paints for eight through ten.</summary>
    internal RgbColor Color(int color)
    {
        if ((uint)color >= 11) throw new ArgumentOutOfRangeException(nameof(color));
        if (color < 8) return GoldenTorizoHealthPaint.Color(0, color + 1, SuperMetroid.Core.Enemies.Artwork.GoldenTorizoHealthFacing.Front);
        if (color == 10) return new(darkRed, 0, darkBlue);
        int green = color == 8 ? highlightGreen : middleGreen;
        int blue = Math.Clamp(green + blueTint, 0, MaximumChannel);
        return new(MaximumChannel, green, blue);
    }
}
