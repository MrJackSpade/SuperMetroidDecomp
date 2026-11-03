using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Eight grey-blue surface shades shared by the escape door and the matching room palette.
/// The reviewed escape-door material owns four selected highlight/edge levels,
/// a clipped five-step bevel ramp, blue tint3 and the middle-bevel blue channel.
/// This owner centralizes the identical material; each supplied occurrence and
/// every independently edited channel remains separate. No new paint choice is added.
/// </summary>
internal sealed class CeresDoorEscapeSurfacePaint
{
    /// <summary>Blue tint, bevel decrement, and RGB5 ceiling used by the calculated door material.</summary>
    private const int BlueTint = 3, BevelShadeStep = 5, MaximumChannel = (1 << 5) - 1;
    /// <summary>Four independently supplied highlight and edge brightness levels shared by matching door surfaces.</summary>
    internal readonly int[] highlightAndEdgeLevels;
    /// <summary>Selected bevel endpoints and middle blue channel retained from the supplied eight-color surface.</summary>
    private readonly int bevelPeak, bevelShadow, middleBevelBlue;
    /// <summary>Per-color exceptions kept when supplied art differs from the calculated shared material.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Keeps the selected color parameters and every independently edited supplied color.</summary>
    /// <param name="highlightAndEdgeLevels">The four highlight and edge brightness levels.</param>
    /// <param name="bevelPeak">The brightest bevel level.</param>
    /// <param name="bevelShadow">The darkest bevel level.</param>
    /// <param name="middleBevelBlue">The middle bevel's blue.</param>
    /// <param name="edits">The supplied colors the import found differing from the calculated material.</param>
    internal CeresDoorEscapeSurfacePaint(int[] highlightAndEdgeLevels, int bevelPeak, int bevelShadow, int middleBevelBlue,
        Dictionary<PaintColour, RgbColor> edits)
    {
        this.highlightAndEdgeLevels = highlightAndEdgeLevels;
        this.bevelPeak = bevelPeak;
        this.bevelShadow = bevelShadow;
        this.middleBevelBlue = middleBevelBlue;
        this.edits = edits;
    }

    /// <summary>Returns an authored exception where present or the calculated grey-blue door shade.</summary>
    internal RgbColor ColorAt(int index)
    {
        if ((uint)index >= 8) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new PaintColour(0, index), out RgbColor edited) ? edited : Calculate(index);
    }

    /// <summary>Builds the clipped five-step bevel ramp and blue tint, preserving the selected middle-bevel blue level.</summary>
    private RgbColor Calculate(int index)
    {
        int level = index < 4 ? highlightAndEdgeLevels[index] : Math.Max(bevelShadow, bevelPeak - BevelShadeStep * (index - 4));
        int blue = index == 0 ? level : index == 5 ? middleBevelBlue : Math.Min(MaximumChannel, level + BlueTint);
        return new(level, level, blue);
    }
}
