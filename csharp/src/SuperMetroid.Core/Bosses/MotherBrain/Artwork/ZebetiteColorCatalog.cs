using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Immutable RGB5 artwork for Zebetite's eight-frame two-color pulse.</summary>
public sealed class ZebetiteColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("ZebetiteColorCatalog-v1", content =>
        {
            content.Append("frames", ZebetiteColorFormat.FrameCount);
            Span<Bgr555> row = stackalloc Bgr555[ZebetiteColorFormat.ColorsPerFrame];
            for (int frame = 0; frame < ZebetiteColorFormat.FrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(frame, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Only palette cells edited away from the calculated native pulse, keyed by frame and color.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> edits;

    /// <summary>Keeps the selected pulse colours that differ from the calculated native pulse.</summary>
    /// <param name="frames">The eight selected two-colour pulse rows.</param>
    internal ZebetiteColorCatalog(Bgr555[][] frames)
    {
        edits = [];
        for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < frames[frame].Length; color++)
                if (frames[frame][color] != NativeColor(frame, color))
                    edits.Add(new ColourCell(frame, color), frames[frame][color]);
    }

    /// <summary>Calculated symmetric pulse of the two selected barrier-core paints.</summary>
    private static Bgr555 NativeColor(int frame, int color) => ZebetitePulsePaintDefinitions.Color(frame, color);

    /// <summary>Selects an edited pulse color when present, otherwise the symmetric native color.</summary>
    private Bgr555 Resolve(int frame, int color) =>
        edits.TryGetValue(new ColourCell(frame, color), out Bgr555 edited)
            ? edited : NativeColor(frame, color);

    /// <summary>Writes the selected pulse row's two colors without advancing the shared palette counter or testing fade/linkage conditions; native <c>HandleZebetitePaletteAnimation</c> at <c>$A6:FD5E</c> targets CGRAM entries 172 and 173 (OBJ palette 2 colors 12 and 13).</summary>
    /// <param name="destination">The two pulsing colours.</param>
    /// <param name="frame">Zero-based pulse row from 0 through 7.</param>
    public void Apply(ColourRun destination, int frame)
    {
        if ((uint)frame >= ZebetiteColorFormat.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        for (int color = 0; color < ZebetiteColorFormat.ColorsPerFrame; color++)
            destination.Write(color, Resolve(frame, color));
    }
}
