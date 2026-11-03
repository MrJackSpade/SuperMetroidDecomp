using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Six shared warm target paints: the normal door, the escape door
/// and Norfair Ridley. The existing selected seven
/// material channels and gold shade steps belong to one paint operation; resource
/// occurrences retain their independently supplied values. These are copied target
/// colors, not a claim about visible door pixels.
/// </summary>
internal sealed class CeresDoorWarmTargetPaint
{
    /// <summary>Largest five-bit BGR component used by the white door highlight.</summary>
    private const int Maximum = (1 << 5) - 1;
    /// <summary>Red-channel decrement between successive computed gold shades.</summary>
    private const int GoldRedStep = 3;
    /// <summary>Green-channel decrement between successive computed gold shades.</summary>
    private const int GoldGreenStep = 6;
    /// <summary>Independent paint channels retained from the six supplied target colors so shared shade relationships survive edits.</summary>
    private readonly int highlightBlue, amberRed, amberGreen, red, goldRed, goldGreen, goldBlue;
    /// <summary>Per-color overrides for supplied targets that do not fit the shared warm-paint calculation.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Keeps the shared warm channels and the supplied targets that do not fit them.</summary>
    /// <param name="channels">The seven shared paint channels.</param>
    /// <param name="edits">The supplied targets the import found differing from the calculation.</param>
    internal CeresDoorWarmTargetPaint(CeresDoorWarmTargetChannels channels, Dictionary<PaintColour, RgbColor> edits)
    {
        highlightBlue = channels.HighlightBlue;
        amberRed = channels.AmberRed;
        amberGreen = channels.AmberGreen;
        red = channels.Red;
        goldRed = channels.GoldRed;
        goldGreen = channels.GoldGreen;
        goldBlue = channels.GoldBlue;
        this.edits = edits;
    }

    /// <summary>Returns an independently supplied target when present, otherwise computes the matching warm highlight, amber, red, or gold shade.</summary>
    internal RgbColor ColorAt(int index)
    {
        if ((uint)index >= 6) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new PaintColour(0, index), out RgbColor edited) ? edited : Calculate(index);
    }

    /// <summary>Builds the white, amber, red, and three descending gold targets from shared five-bit channel parameters.</summary>
    private RgbColor Calculate(int index)
    {
        if (index == 0) return new(Maximum, Maximum, highlightBlue);
        if (index == 1) return new(amberRed, amberGreen, 0);
        if (index == 2) return new(red, 0, 0);
        int shade = index - 3;
        return new(Math.Max(0, goldRed - GoldRedStep * shade),
            Math.Max(0, goldGreen - GoldGreenStep * shade),
            Math.Max(0, goldBlue * (1 - shade)));
    }
}
