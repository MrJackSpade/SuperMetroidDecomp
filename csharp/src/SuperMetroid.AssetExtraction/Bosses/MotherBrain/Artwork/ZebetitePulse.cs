using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The Zebetites' installed two-colour pulse: its edits, its selected colours and their identity.</summary>
internal static class ZebetitePulse
{
    /// <summary>Selects an edited pulse color when present, otherwise the symmetric color.</summary>
    internal static RgbColor Resolve(Dictionary<ColourCell, RgbColor> edits, int frame, int color) =>
        edits.TryGetValue(new ColourCell(frame, color), out RgbColor edited)
            ? edited : ZebetitePulsePaint.Color(frame, color);
}
