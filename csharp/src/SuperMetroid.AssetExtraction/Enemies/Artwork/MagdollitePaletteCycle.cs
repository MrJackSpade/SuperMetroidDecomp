using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The installed RGB5 colors for Magdollite's four-frame OBJ palette cycle, every frame kept as installed.
/// The enemy hook owns frame order, eight-tick cadence, and destination selection.
/// </summary>
public sealed class MagdollitePaletteCycle
{
    /// <summary>The installed frames.</summary>
    private readonly InstalledColourRows frames;

    /// <summary>Keeps the complete installed frames.</summary>
    /// <param name="frames">Each frame's four glow colors.</param>
    internal MagdollitePaletteCycle(InstalledColourRows frames) => this.frames = frames;

    /// <summary>Returns one installed glow color.</summary>
    /// <param name="frame">Zero-based palette-cycle frame 0..3, not an elapsed-update count.</param>
    /// <param name="color">Zero-based glow color 0..3, corresponding to OBJ palette colors nine through twelve.</param>
    /// <returns>Packed BGR555 color from the selected frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside 0..3.</exception>
    public RgbColor Resolve(int frame, int color) => frames.Colour(frame, color);

    /// <summary>Each frame replaces the colours from the tenth (index nine) of Magdollite's colours, which its glow shades recolour (#1512).</summary>
    internal const int FirstAnimatedColor = 9;

    /// <summary>The four colours each frame replaces, nine through twelve; the other twelve stay unchanged.</summary>
    internal const int AnimatedColorCount = 4;
}
