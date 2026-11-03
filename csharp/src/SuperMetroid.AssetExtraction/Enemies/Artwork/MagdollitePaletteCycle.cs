using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Editable RGB5 colors for Magdollite's four-frame OBJ palette cycle.
/// The enemy hook owns frame order, eight-tick cadence, and destination selection.
/// </summary>
public sealed class MagdollitePaletteCycle
{
    /// <summary>Four independently authored glow colors underlying the frames' rotations.</summary>
    private readonly RgbColor[] colors;
    /// <summary>Later-frame cells that differ from the rotation of the first frame.</summary>
    internal readonly Dictionary<ColourStep, RgbColor> edits;

    /// <summary>
    /// Palette_Magdollite_Glow_0..3 rotates left one color per frame of the four-frame cycle; keeps the first frame and the
    /// later-frame cells that deviate from that rotation.
    /// </summary>
    /// <param name="colors">The first frame's four glow colors.</param>
    /// <param name="edits">The later-frame cells the import found differing from the rotation.</param>
    internal MagdollitePaletteCycle(RgbColor[] colors, Dictionary<ColourStep, RgbColor> edits)
    {
        this.colors = colors;
        this.edits = edits;
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
        return edits.TryGetValue(new ColourStep(frame, color), out RgbColor edited)
            ? edited : colors[(color + frame) % colors.Length];
    }

    /// <summary>Each frame replaces the colours from the tenth (index nine) of Magdollite's colours, which its glow shades recolour (#1512).</summary>
    internal const int FirstAnimatedColor = 9;

    /// <summary>The four colours each frame replaces, nine through twelve; the other twelve stay unchanged.</summary>
    internal const int AnimatedColorCount = 4;
}
