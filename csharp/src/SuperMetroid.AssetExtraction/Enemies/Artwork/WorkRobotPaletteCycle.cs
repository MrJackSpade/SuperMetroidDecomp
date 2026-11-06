using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Editable Work Robot RGB5 frames. Record order, 64/16-tick durations,
/// and sprite palette ownership remain engine-defined.
/// </summary>
public sealed class WorkRobotPaletteCycle
{
    /// <summary>Edited RGB5 frames, or null when every cell follows the compiled stock color pattern.</summary>
    internal readonly RgbColor[][]? frames;

    /// <summary>Keeps the authored frames when at least one color differs from the rotation.</summary>
    /// <param name="frames">The authored frames, or null when every cell follows the stock rotation.</param>
    internal WorkRobotPaletteCycle(RgbColor[][]? frames) => this.frames = frames;

    /// <summary>
    /// Six four-color Work Robot records: red-only brightness rotates
    /// forward three positions and back. The four brightness levels form two
    /// pairs separated by sixteen, with seven between the members of each pair.
    /// </summary>
    internal static RgbColor StockColor(int frame, int color)
    {
        int phase = Math.Min(frame, 6 - frame);
        int position = (color + phase) & 3;
        return new RgbColor(31 - 16 * (position >> 1) - 7 * (position & 1), 0, 0);
    }

    /// <summary>Resolves one packed RGB555 color from an authored Work Robot cycle frame.</summary>
    /// <param name="frame">Zero-based palette record from 0 through 5.</param>
    /// <param name="color">Zero-based color within the four-color record.</param>
    /// <returns>The selected packed RGB555 word.</returns>
    public RgbColor Resolve(int frame, int color)
    {
        if ((uint)frame >= WorkRobotPaletteTimingDefinitions.StepCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= WorkRobotPaletteDefinitions.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return frames is null ? StockColor(frame, color) : frames[frame][color];
    }

}
