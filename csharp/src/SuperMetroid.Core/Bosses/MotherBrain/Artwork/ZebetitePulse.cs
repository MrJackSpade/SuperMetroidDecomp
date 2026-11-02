using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The Zebetites' installed two-colour pulse: its edits, its selected colours and their identity.</summary>
internal static class ZebetitePulse
{
    /// <summary>Selects an edited pulse color when present, otherwise the symmetric color.</summary>
    internal static RgbColor Resolve(Dictionary<ColourCell, RgbColor> edits, int frame, int color) =>
        edits.TryGetValue(new ColourCell(frame, color), out RgbColor edited)
            ? edited : ZebetitePulsePaint.Color(frame, color);

    /// <summary>Writes the selected pulse frame's two colors.</summary>
    internal static void Apply(Dictionary<ColourCell, RgbColor> edits, ColourRun destination, int frame)
    {
        if ((uint)frame >= ZebetiteColorFormat.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < ZebetiteColorFormat.ColorsPerFrame; color++)
            destination.Write(color, Resolve(edits, frame, color));
    }
}
