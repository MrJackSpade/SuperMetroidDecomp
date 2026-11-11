using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

/// <summary>
/// The installed map highlight cycle (#1512): each frame's hold and its sixteen colours, kept as installed (#1615). Only
/// the import reads the colours, pre-colouring the map sprite sheet in one highlight shade per frame; gameplay keeps only
/// the holds and the frame count through <see cref="Timing"/>.
/// </summary>
public sealed class MapPaletteCycle
{
    /// <summary>The installed hold of every frame, in frame order.</summary>
    private readonly int[] durations;

    /// <summary>The installed sixteen colours of every frame, in frame order.</summary>
    private readonly RgbColor[][] colors;

    /// <summary>Keeps every frame's installed hold and colours.</summary>
    /// <param name="durations">The hold of each frame.</param>
    /// <param name="colors">The sixteen colours of each frame.</param>
    internal MapPaletteCycle(int[] durations, RgbColor[][] colors)
    {
        if (colors.Length != durations.Length)
            throw new InvalidDataException($"The map highlight cycle has {durations.Length} holds but {colors.Length} colour frames.");
        this.durations = [.. durations];
        this.colors = [.. colors.Select(frame => (RgbColor[])frame.Clone())];
        Timing = new MapHighlightCycle([.. this.durations]);
    }

    /// <summary>Number of selected ordered color frames, one through 255.</summary>
    public int FrameCount => durations.Length;

    /// <summary>The frame count and holds gameplay steps the highlight through (#1512).</summary>
    public MapHighlightCycle Timing { get; }

    /// <summary>Rejects a frame index outside the selected cycle.</summary>
    private void CheckFrame(int frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
    }

    /// <summary>Returns the installed hold for one color frame in map-palette animation updates.</summary>
    /// <returns>A hold of one through 254 ticks.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    public int Duration(int frame)
    {
        CheckFrame(frame);
        return durations[frame];
    }

    /// <summary>
    /// The sixteen colours frame <paramref name="frame"/> shows on the map's highlighted sprites, including the first,
    /// which the import pre-colours the frame's highlight shade from.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
    public RgbColor[] Colours(int frame)
    {
        CheckFrame(frame);
        return (RgbColor[])colors[frame].Clone();
    }
}
