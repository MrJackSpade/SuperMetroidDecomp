using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The installed Work Robot RGB5 frames, every frame kept as installed. Record order, 64/16-tick durations,
/// and sprite palette ownership remain engine-defined.
/// </summary>
public sealed class WorkRobotPaletteCycle
{
    /// <summary>The installed frames.</summary>
    private readonly InstalledColourRows frames;

    /// <summary>Keeps the complete installed frames.</summary>
    /// <param name="frames">Each record's four colours.</param>
    internal WorkRobotPaletteCycle(InstalledColourRows frames) => this.frames = frames;

    /// <summary>Resolves one installed packed RGB555 color of a Work Robot cycle frame.</summary>
    /// <param name="frame">Zero-based palette record from 0 through 5.</param>
    /// <param name="color">Zero-based color within the four-color record.</param>
    /// <returns>The installed packed RGB555 word.</returns>
    public RgbColor Resolve(int frame, int color) => frames.Colour(frame, color);
}
