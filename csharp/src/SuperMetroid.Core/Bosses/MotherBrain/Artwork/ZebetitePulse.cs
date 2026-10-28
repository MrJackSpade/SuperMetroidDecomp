using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The Zebetites' installed two-colour pulse: its edits, its selected colours and their identity.</summary>
internal static class ZebetitePulse
{
    /// <summary>Hashes the eight resolved pulse rows.</summary>
    internal static string ContentIdentity(Dictionary<ColourCell, RgbColor> edits) =>
        SelectedPresentationHash.Create("ZebetiteColorCatalog-v1", content =>
        {
            content.Append("frames", ZebetiteColorFormat.FrameCount);
            Span<RgbColor> row = stackalloc RgbColor[ZebetiteColorFormat.ColorsPerFrame];
            for (int frame = 0; frame < ZebetiteColorFormat.FrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(edits, frame, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Keeps the selected pulse colours that differ from the calculated native pulse.</summary>
    internal static Dictionary<ColourCell, RgbColor> Edits(RgbColor[][] frames)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < frames[frame].Length; color++)
                if (frames[frame][color] != ZebetitePulsePaint.Color(frame, color))
                    edits.Add(new ColourCell(frame, color), frames[frame][color]);
        return edits;
    }

    /// <summary>Selects an edited pulse color when present, otherwise the symmetric native color.</summary>
    internal static RgbColor Resolve(Dictionary<ColourCell, RgbColor> edits, int frame, int color) =>
        edits.TryGetValue(new ColourCell(frame, color), out RgbColor edited)
            ? edited : ZebetitePulsePaint.Color(frame, color);

    /// <summary>Writes the selected pulse row's two colors.</summary>
    internal static void Apply(Dictionary<ColourCell, RgbColor> edits, ColourRun destination, int frame)
    {
        if ((uint)frame >= ZebetiteColorFormat.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < ZebetiteColorFormat.ColorsPerFrame; color++)
            destination.Write(color, Resolve(edits, frame, color));
    }
}
