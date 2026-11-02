using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable RGB5 colors for Magdollite's four-frame OBJ palette cycle.
/// The enemy hook owns frame order, eight-tick cadence, and destination selection.
/// </summary>
public sealed class MagdollitePaletteCycle
{
    /// <summary>Four independently authored glow colors underlying the frames' rotations.</summary>
    private readonly RgbColor[] colors;
    /// <summary>Later-frame cells that differ from the rotation of the first frame.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>
    /// Palette_Magdollite_Glow_0..3 rotates left one color per
    /// frame of the four-frame cycle. Preserve arbitrary edited later frames
    /// as deviations from that rotation, without rebuilding the table.
    /// </summary>
    internal MagdollitePaletteCycle(RgbColor[][] frames)
    {
        colors = frames[0];
        for (int frame = 1; frame < frames.Length; frame++)
        for (int color = 0; color < colors.Length; color++)
            if (frames[frame][color] != colors[(color + frame) % colors.Length])
                edits.Add(new ColourCell(frame, color), frames[frame][color]);
    }

    /// <summary>Returns one selected glow color, using the first frame's left rotation unless the requested later-frame cell was independently edited.</summary>
    /// <param name="frame">Zero-based palette-cycle frame 0..3, not an elapsed-update count.</param>
    /// <param name="color">Zero-based glow color 0..3, corresponding to OBJ palette colors nine through twelve.</param>
    /// <returns>Packed BGR555 color from the selected frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside 0..3.</exception>
    public RgbColor Resolve(int frame, int color)
    {
        if ((uint)frame >= MagdollitePaletteDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= colors.Length)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(frame, color), out RgbColor edited)
            ? edited : colors[(color + frame) % colors.Length];
    }

    /// <summary>Writes the selected frame's four colors without advancing cycle timing or altering the other colors in Magdollite's OBJ palette.</summary>
    /// <param name="animated">The four animated colours of Magdollite's colour set.</param>
    /// <param name="frame">Palette-cycle frame 0..3 chosen by the enemy's graphics-drawn hook.</param>
    /// <exception cref="ArgumentOutOfRangeException">The frame index is invalid.</exception>
    /// <exception cref="ArgumentException">The run is not four colours.</exception>
    public void ApplyFrame(ColourRun animated, int frame)
    {
        Ensure.LengthEqual(animated.Colors, MagdollitePaletteDefinitions.AnimatedColorCount);
        for (int color = 0; color < MagdollitePaletteDefinitions.AnimatedColorCount; color++)
            animated.Write(color, Resolve(frame, color));
    }
}
