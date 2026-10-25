using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable RGB5 colors for Magdollite's four-frame OBJ palette cycle.
/// The enemy hook owns frame order, eight-tick cadence, and destination selection.
/// </summary>
public sealed class MagdollitePaletteCycle
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("MagdollitePaletteCycle-v1", content =>
        {
            content.Append("frames", MagdollitePaletteDefinitions.FrameCount);
            Span<Bgr555> row = stackalloc Bgr555[MagdollitePaletteDefinitions.AnimatedColorCount];
            for (int frame = 0; frame < MagdollitePaletteDefinitions.FrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(frame, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Four independently authored glow colors underlying the native row rotations.</summary>
    private readonly Bgr555[] colors;
    /// <summary>Later-row cells that differ from the native rotation of the first row.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> edits = [];

    /// <summary>
    /// Palette_Magdollite_Glow_0..3 at $A8:AC2E-AC34 rotates left one color per
    /// phase in the four native OBJ rows. Preserve arbitrary edited later rows
    /// as deviations from that rotation, without rebuilding the native table.
    /// </summary>
    internal MagdollitePaletteCycle(Bgr555[][] frames)
    {
        colors = frames[0];
        for (int frame = 1; frame < frames.Length; frame++)
        for (int color = 0; color < colors.Length; color++)
            if (frames[frame][color] != colors[(color + frame) % colors.Length])
                edits.Add(new ColourCell(frame, color), frames[frame][color]);
    }

    /// <summary>Returns one selected glow color, using the first row's native left rotation unless the requested later-row cell was independently edited.</summary>
    /// <param name="frame">Zero-based palette-cycle row 0..3, not an elapsed-update count.</param>
    /// <param name="color">Zero-based glow color 0..3, corresponding to OBJ palette colors nine through twelve.</param>
    /// <returns>Packed SNES BGR555 color from the selected row.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or color index is outside 0..3.</exception>
    public Bgr555 Resolve(int frame, int color)
    {
        if ((uint)frame >= MagdollitePaletteDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= colors.Length)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(frame, color), out Bgr555 edited)
            ? edited : colors[(color + frame) % colors.Length];
    }

    /// <summary>Writes the selected four-color row without advancing cycle timing or altering the other colors in Magdollite's OBJ palette.</summary>
    /// <param name="animated">The four animated colours of Magdollite's colour set.</param>
    /// <param name="frame">Palette-cycle row 0..3 chosen by the enemy's graphics-drawn hook.</param>
    /// <exception cref="ArgumentOutOfRangeException">The row index is invalid.</exception>
    /// <exception cref="ArgumentException">The run is not four colours.</exception>
    public void ApplyFrame(ColourRun animated, int frame)
    {
        Ensure.LengthEqual(animated.Colors, MagdollitePaletteDefinitions.AnimatedColorCount);
        for (int color = 0; color < MagdollitePaletteDefinitions.AnimatedColorCount; color++)
            animated.Write(color, Resolve(frame, color));
    }
}
