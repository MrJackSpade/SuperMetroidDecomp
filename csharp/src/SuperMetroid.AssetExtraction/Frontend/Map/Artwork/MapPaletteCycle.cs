using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

/// <summary>
/// The installed map highlight cycle (#1512): each frame's hold and the colour its highlight shows, kept as installed (#1615,
/// #1637). Only the import reads the highlight colours, pre-colouring the map sprite sheet in one highlight shade per frame;
/// gameplay keeps only the holds and the frame count through <see cref="Timing"/>.
/// </summary>
public sealed class MapPaletteCycle
{
    /// <summary>The installed hold of every frame, in frame order.</summary>
    internal readonly int[] durations;

    /// <summary>The installed highlight colour of every frame, in frame order.</summary>
    private readonly RgbColor[] highlights;

    /// <summary>Keeps every frame's installed hold and highlight colour.</summary>
    /// <param name="durations">The hold of each frame.</param>
    /// <param name="highlights">The colour each frame's highlight shows.</param>
    internal MapPaletteCycle(int[] durations, RgbColor[] highlights)
    {
        if (highlights.Length != durations.Length)
            throw new InvalidDataException($"The map highlight cycle has {durations.Length} holds but {highlights.Length} highlight colours.");
        this.durations = [.. durations];
        this.highlights = [.. highlights];
        Timing = new MapHighlightCycle([.. this.durations]);
    }

    /// <summary>Number of selected ordered color frames, one through 255.</summary>
    public int FrameCount => durations.Length;

    /// <summary>The frame count and holds gameplay steps the highlight through (#1512).</summary>
    public MapHighlightCycle Timing { get; }

    /// <summary>Rejects a frame index outside the selected cycle.</summary>
    internal void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
    }

    /// <summary>
    /// The colour the map's highlighted sprites' highlight shows during frame <paramref name="frame"/>, which the import
    /// pre-colours the frame's highlight shade from.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    public RgbColor Highlight(int frame)
    {
        CheckFrame(frame);
        return highlights[frame];
    }
}
