using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Normal Ceres door paint. Palette slots are one-based here.
/// Shared hue channels and the three-shade gold ramp calculate; the twenty
/// paint-channel seeds and selected hue/ramp policies specify categorical material paint.
/// Slots1..8 paint visible bevels/light/shadow. Slots9..15 are separately preserved
/// copied target paints, not claimed to be visible door pixels. The uniform
/// copy supplies no rule for choosing different material colors.
/// Supplied channel changes remain independent, including changes to shared seeds.
/// </summary>
internal sealed class CeresDoorNormalPaint
{
    /// <summary>Largest RGB5 component accepted by the paint calculation.</summary>
    private const int MaximumChannel = (1 << 5) - 1;
    /// <summary>Blue lift applied to the first two pale door highlights.</summary>
    private const int HighlightBlueTint = 14;

    /// <summary>Independent copied target paint for door slots nine through fourteen.</summary>
    internal CeresDoorWarmTargetPaint WarmTargets { get; }
    /// <summary>Selected categorical channel seeds keyed by palette slot and channel.</summary>
    internal readonly Dictionary<CeresDoorPaintChannel, int> paint;
    /// <summary>Supplied colors that depart from the calculated normal-door composition.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Keeps the chosen material paint channels, the copied warm targets and the independently edited colors.</summary>
    /// <param name="warmTargets">The copied warm target paint for slots nine through fourteen.</param>
    /// <param name="paint">The categorical channel seeds by slot and channel.</param>
    /// <param name="edits">The supplied colors the import found differing from the composition, keyed by one-based slot.</param>
    internal CeresDoorNormalPaint(CeresDoorWarmTargetPaint warmTargets, Dictionary<CeresDoorPaintChannel, int> paint,
        Dictionary<PaintColour, RgbColor> edits)
    {
        WarmTargets = warmTargets;
        this.paint = paint;
        this.edits = edits;
    }

    /// <summary>Reads a stored material-paint channel for one door palette slot.</summary>
    private int Seed(int slot, ColorChannel channel) => paint[new CeresDoorPaintChannel(slot, channel)];

    /// <summary>Resolves a supplied edit or calculates the requested one-based door paint slot.</summary>
    internal RgbColor ColorAt(int index)
    {
        if ((uint)index >= 15) throw new ArgumentOutOfRangeException(nameof(index));
        int slot = index + 1;
        var slotCell = new PaintColour(0, index + 1);
        return edits.TryGetValue(slotCell, out RgbColor color) ? color : Calculate(slot);
    }

    /// <summary>Builds a normal door shade from categorical channel seeds and copied warm targets.</summary>
    private RgbColor Calculate(int slot)
    {
        if (slot is >= 9 and <= 14) return WarmTargets.ColorAt(slot - 9);
        int green = slot == 15 ? MaximumChannel : slot == 4 ? 0 : slot == 8 ? Seed(7, ColorChannel.Green) : Seed(slot, ColorChannel.Green);
        int blue = slot is 1 or 2 ? Math.Min(MaximumChannel, green + HighlightBlueTint) : Seed(slot, ColorChannel.Blue);
        return new FadeLevelColour(0, green, blue).ToColour();
    }
}
