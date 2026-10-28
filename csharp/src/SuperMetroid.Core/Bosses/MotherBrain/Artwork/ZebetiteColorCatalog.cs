using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Immutable RGB5 artwork for Zebetite's eight-frame two-color pulse.</summary>
public sealed class ZebetiteColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => ZebetitePulse.ContentIdentity(edits);

    /// <summary>Only palette cells edited away from the calculated native pulse, keyed by frame and color.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits;

    /// <summary>Keeps the selected pulse colours that differ from the calculated native pulse.</summary>
    /// <param name="frames">The eight selected two-colour pulse rows.</param>
    internal ZebetiteColorCatalog(RgbColor[][] frames) => edits = ZebetitePulse.Edits(frames);

    /// <summary>Writes the selected pulse row's two colors without advancing the shared palette counter or testing fade/linkage conditions; native <c>HandleZebetitePaletteAnimation</c> at <c>$A6:FD5E</c> targets CGRAM entries 172 and 173 (OBJ palette 2 colors 12 and 13).</summary>
    /// <param name="destination">The two pulsing colours.</param>
    /// <param name="frame">Zero-based pulse row from 0 through 7.</param>
    public void Apply(ColourRun destination, int frame) => ZebetitePulse.Apply(edits, destination, frame);
}
