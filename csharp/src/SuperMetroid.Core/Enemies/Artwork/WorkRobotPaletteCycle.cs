using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable Work Robot RGB5 frames. Native record order, 64/16-tick durations,
/// and OBJ palette ownership remain engine-defined.
/// </summary>
public sealed class WorkRobotPaletteCycle
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("WorkRobotPaletteCycle-v1", content =>
        {
            content.Append("frames", WorkRobotPaletteTimingDefinitions.RecordCount);
            Span<Bgr555> row = stackalloc Bgr555[WorkRobotPaletteDefinitions.ColorCount];
            for (int frame = 0; frame < WorkRobotPaletteTimingDefinitions.RecordCount; frame++)
            {
                for (int color = 0; color < row.Length; color++)
                    row[color] = Resolve(frame, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Edited RGB5 rows, or null when every cell follows the compiled stock color pattern.</summary>
    internal readonly Bgr555[][]? frames;

    /// <summary>Keeps authored rows only when at least one color differs from the native rotation.</summary>
    internal WorkRobotPaletteCycle(Bgr555[][] frames)
    {
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
        {
            if (frames[frame][color] != StockColor(frame, color))
            {
                this.frames = frames;
                return;
            }
        }
    }

    /// <summary>
    /// $A8:CCC1, six four-color Work Robot records: red-only brightness rotates
    /// forward three positions and back. The four brightness levels form two
    /// pairs separated by sixteen, with seven between the members of each pair.
    /// </summary>
    private static Bgr555 StockColor(int frame, int color)
    {
        int phase = Math.Min(frame, 6 - frame);
        int position = (color + phase) & 3;
        return new Bgr555(31 - 16 * (position >> 1) - 7 * (position & 1), 0, 0);
    }

    /// <summary>Resolves one packed RGB555 color from an authored Work Robot cycle frame.</summary>
    /// <param name="frame">Zero-based native palette record from 0 through 5.</param>
    /// <param name="color">Zero-based color within the four-color record.</param>
    /// <returns>The selected packed SNES RGB555 word.</returns>
    public Bgr555 Resolve(int frame, int color)
    {
        if ((uint)frame >= WorkRobotPaletteTimingDefinitions.RecordCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= WorkRobotPaletteDefinitions.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return frames is null ? StockColor(frame, color) : frames[frame][color];
    }

    /// <summary>Copies one four-color frame over <paramref name="animated"/> without advancing the timer.</summary>
    /// <param name="animated">The four animated colours of the Work Robot's colour set.</param>
    /// <param name="frame">Zero-based cycle record from 0 through 5.</param>
    /// <exception cref="ArgumentException">The run is not four colours.</exception>
    public void ApplyFrame(ColourRun animated, int frame)
    {
        Ensure.LengthEqual(animated.Colors, WorkRobotPaletteDefinitions.ColorCount);
        for (int color = 0; color < WorkRobotPaletteDefinitions.ColorCount; color++)
            animated.Write(color, Resolve(frame, color));
    }
}
