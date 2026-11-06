using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

/// <summary>
/// The installed map highlight cycle (#1512): each frame's hold and its sixteen colours. Only the import reads the colours,
/// pre-colouring the map sprite sheet in one highlight shade per frame; gameplay keeps only the holds and the frame count
/// through <see cref="Timing"/>.
/// </summary>
public sealed class MapPaletteCycle
{
    /// <summary>Frame holds that differ from the stock fifteen/three-tick schedule.</summary>
    internal readonly Dictionary<int, int> durationEdits = [];

    /// <summary>Independent forward color phases or the complete supplied sequence for other lengths.</summary>
    internal readonly RgbColor[][] colors;

    /// <summary>Reverse-traversal colors that differ from their mirrored forward phase, by frame and colour.</summary>
    internal readonly Dictionary<ColourStep, RgbColor> reverseColorEdits = [];

    /// <summary>
    /// The cycle pauses fifteen ticks at the loop origin and advances other
    /// highlight phases every three ticks. Independently edited frame holds remain exact.
    /// </summary>
    private static int StockDuration(int frame) => frame == 0 ? 15 : 3;

    /// <summary>Retains independent holds and paint edits while deriving the stock fourteen-frame mirror.</summary>
    /// <param name="durations">The hold of each frame.</param>
    /// <param name="suppliedColors">The sixteen colours of each frame.</param>
    internal MapPaletteCycle(int[] durations, RgbColor[][] suppliedColors)
    {
        FrameCount = durations.Length;
        for (int frame = 0; frame < FrameCount; frame++)
            if (durations[frame] != StockDuration(frame)) durationEdits.Add(frame, durations[frame]);
        // The fourteen stock frames traverse eight phases forward then backward.
        // Retain the independent phase artwork; only the reverse traversal is calculated.
        int phaseCount = FrameCount == 14 ? 8 : FrameCount;
        colors = suppliedColors[..phaseCount];
        for (int frame = phaseCount; frame < FrameCount; frame++)
        for (int color = 0; color < MapPaletteCycleFormat.ColorCount; color++)
            if (suppliedColors[frame][color] != colors[Phase(frame)][color])
                reverseColorEdits.Add(new ColourStep(frame, color), suppliedColors[frame][color]);
        Timing = new MapHighlightCycle([.. Enumerable.Range(0, FrameCount).Select(Duration)]);
    }

    /// <summary>Number of selected ordered color frames, one through 255.</summary>
    public int FrameCount { get; }

    /// <summary>The frame count and holds gameplay steps the highlight through (#1512).</summary>
    public MapHighlightCycle Timing { get; }

    /// <summary>Maps a stock reverse frame to its matching forward paint phase.</summary>
    private int Phase(int frame) => FrameCount == 14 ? Math.Min(frame, 14 - frame) : frame;

    /// <summary>Rejects a frame index outside the selected cycle.</summary>
    private void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
    }

    /// <summary>Returns the authored hold for one color frame in map-palette animation updates.</summary>
    /// <returns>A hold of one through 254 ticks; stock frame zero holds fifteen ticks and other stock frames hold three.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    public int Duration(int frame)
    {
        CheckFrame(frame);
        return durationEdits.TryGetValue(frame, out int edited) ? edited : StockDuration(frame);
    }

    /// <summary>
    /// The sixteen colours frame <paramref name="frame"/> shows on the map's highlighted sprites, including the first,
    /// which the import pre-colours the frame's highlight shade from.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    public RgbColor[] Colours(int frame)
    {
        CheckFrame(frame);
        var colours = new RgbColor[MapPaletteCycleFormat.ColorCount];
        for (int color = 0; color < colours.Length; color++)
            colours[color] = reverseColorEdits.TryGetValue(new ColourStep(frame, color), out RgbColor edited) ? edited : colors[Phase(frame)][color];
        return colours;
    }
}
