using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Immutable RGB5 artwork for Zebetite's eight-frame two-color pulse.</summary>
public sealed class ZebetiteColorCatalog
{
    /// <summary>Only palette cells edited away from the calculated pulse, keyed by frame and color.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Keeps the selected pulse colours that differ from the calculated pulse.</summary>
    /// <param name="edits">The pulse colours the import found differing from the calculated pulse.</param>
    internal ZebetiteColorCatalog(Dictionary<PaintColour, RgbColor> edits) => this.edits = edits;

    /// <summary>Colour <paramref name="color"/>, zero or one, of pulse frame <paramref name="frame"/>, zero through seven.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame or colour is outside the pulse.</exception>
    public RgbColor Resolve(int frame, int color) =>
        (uint)frame < ZebetiteColorFormat.FrameCount && (uint)color < ZebetiteColorFormat.ColorsPerFrame
            ? ZebetitePulse.Resolve(edits, frame, color)
            : throw new ArgumentOutOfRangeException(nameof(frame), $"Zebetite pulse frame {frame}, colour {color} is outside the pulse.");
}
