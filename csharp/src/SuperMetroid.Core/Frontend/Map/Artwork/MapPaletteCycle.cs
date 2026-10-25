using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Immutable authored highlight colors and durations; contains no palette addresses or audio commands.</summary>
public sealed class MapPaletteCycle
{
    /// <summary>Frame holds that differ from the native fifteen/three-tick schedule.</summary>
    internal readonly Dictionary<int, byte> durationEdits = [];
    /// <summary>Independent forward color phases or the complete supplied sequence for other lengths.</summary>
    internal readonly Bgr555[][] colors;
    /// <summary>Reverse-traversal colors that differ from their mirrored forward phase.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> reverseColorEdits = [];

    /// <summary>
    /// Native $82:C10C pauses fifteen ticks at the loop origin and advances other
    /// highlight phases every three ticks. Independently edited frame holds remain exact.
    /// </summary>
    private static byte StockDuration(int frame) => frame == 0 ? (byte)15 : (byte)3;

    /// <summary>Retains independent holds and paint edits while deriving the stock fourteen-frame mirror.</summary>
    internal MapPaletteCycle(byte[] durations, Bgr555[][] suppliedColors)
    {
        FrameCount = durations.Length;
        for (int frame = 0; frame < FrameCount; frame++)
            if (durations[frame] != StockDuration(frame)) durationEdits.Add(frame, durations[frame]);
        // The fourteen native frames traverse eight phases forward then backward.
        // Retain the independent phase artwork; only the reverse traversal is calculated.
        int phaseCount = FrameCount == 14 ? 8 : FrameCount;
        colors = suppliedColors[..phaseCount];
        for (int frame = phaseCount; frame < FrameCount; frame++)
        for (int color = 0; color < MapPaletteCycleFormat.ColorCount; color++)
            if (suppliedColors[frame][color] != colors[Phase(frame)][color])
                reverseColorEdits.Add(new ColourCell(frame, color), suppliedColors[frame][color]);
    }

    /// <summary>Number of selected ordered color frames, one through 255; the animation owner, not this resource, advances and wraps the frame cursor.</summary>
    public int FrameCount { get; }

    /// <summary>Maps a stock reverse frame to its matching forward paint phase.</summary>
    private int Phase(int frame) => FrameCount == 14 ? Math.Min(frame, 14 - frame) : frame;

    /// <summary>Rejects a frame index outside the selected cycle.</summary>
    private void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
    }

    /// <summary>Returns the authored hold for one color frame in map-palette animation updates.</summary>
    /// <param name="frame">Zero-based selected frame index, strictly below <see cref="FrameCount"/>; not automatically wrapped.</param>
    /// <returns>A hold of one through 254 ticks; stock frame zero holds fifteen ticks and other stock frames hold three.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    public byte Duration(int frame)
    {
        CheckFrame(frame);
        return durationEdits.ContainsKey(frame) ? durationEdits[frame] : StockDuration(frame);
    }

    /// <summary>Copies the selected frame's sixteen colours over <paramref name="row"/> without advancing time or requesting audio.</summary>
    /// <param name="row">The screen row the highlights colour, changed in place.</param>
    /// <param name="frame">Zero-based frame index, strictly below <see cref="FrameCount"/>.</param>
    /// <remarks>Writes all sixteen colours including the row's first colour.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    /// <exception cref="ArgumentException"><paramref name="row"/> does not hold sixteen colours.</exception>
    public void Apply(ColourRun row, int frame)
    {
        CheckFrame(frame);
        if (row.Count != MapPaletteCycleFormat.ColorCount)
            throw new ArgumentException($"A highlight cycle colours {MapPaletteCycleFormat.ColorCount} colours.", nameof(row));
        for (int color = 0; color < MapPaletteCycleFormat.ColorCount; color++)
            row.Write(color,
                reverseColorEdits.TryGetValue(new ColourCell(frame, color), out Bgr555 edited)
                    ? edited : colors[Phase(frame)][color]);
    }
}
