using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The running delays each of the getaway's wing frames leaves behind for Samus's unauthored Speed Booster cadence
/// (<see cref="UnauthoredBoostCadence"/>, #1338). The import decodes them once from the player's cartridge, from what the
/// wing frame's floor update leaves where the unauthored cadence reads, so Core holds no floor update, queue position or
/// cartridge address. The capsule's frames leave nothing the cadence reads.
/// </summary>
public sealed class CeresRidleyGetawayBoostDelays
{
    /// <summary>Ridley's wings' two frames.</summary>
    public const int WingFrameCount = 2;

    /// <summary>Each wing frame's ten delays, by frame then running frame.</summary>
    private readonly int[][] wings;

    /// <summary>Keeps validated delays.</summary>
    private CeresRidleyGetawayBoostDelays(int[][] wings) => this.wings = wings;

    /// <summary>
    /// Validates the installed delays: two wing frames of <see cref="UnauthoredBoostCadence.FrameCount"/> delays, each
    /// 0 through <see cref="UnauthoredBoostCadence.LongestDelay"/> ticks.
    /// </summary>
    /// <param name="wings">Each wing frame's delays, by frame then running frame.</param>
    /// <exception cref="InvalidDataException">A frame or delay is missing, extra or out of range.</exception>
    public static CeresRidleyGetawayBoostDelays Load(IReadOnlyList<IReadOnlyList<int>> wings)
    {
        Ensure.NotNull(wings);
        if (wings.Count != WingFrameCount)
            throw new InvalidDataException($"Ceres Ridley's getaway needs {WingFrameCount} wing frames' delays; found {wings.Count}.");
        var copy = new int[WingFrameCount][];
        for (int frame = 0; frame < WingFrameCount; frame++)
        {
            IReadOnlyList<int> delays = wings[frame] ?? throw new InvalidDataException($"Ceres Ridley wing frame {frame} has no delays.");
            if (delays.Count != UnauthoredBoostCadence.FrameCount)
                throw new InvalidDataException($"Ceres Ridley wing frame {frame} needs {UnauthoredBoostCadence.FrameCount} delays; found {delays.Count}.");
            foreach (int delay in delays)
            {
                if (delay is < 0 or > UnauthoredBoostCadence.LongestDelay)
                    throw new InvalidDataException($"Ceres Ridley wing frame {frame} leaves a {delay}-tick delay, outside 0 through {UnauthoredBoostCadence.LongestDelay}.");
            }
            copy[frame] = [.. delays];
        }
        return new CeresRidleyGetawayBoostDelays(copy);
    }

    /// <summary>The ten delays wing frame <paramref name="frame"/> leaves behind, by running frame.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of the wing's frames.</exception>
    public IReadOnlyList<int> LeftBy(CeresFloorAnimationFrame frame) => frame switch
    {
        CeresFloorAnimationFrame.WingFrame0 => wings[0],
        CeresFloorAnimationFrame.WingFrame1 => wings[1],
        CeresFloorAnimationFrame.BabyFrame0 or CeresFloorAnimationFrame.BabyFrame1 or CeresFloorAnimationFrame.BabyFrame2 or
            CeresFloorAnimationFrame.ElevatorLight or CeresFloorAnimationFrame.ElevatorDark =>
            throw new InvalidDataException($"The getaway's {frame} floor frame leaves no Speed Booster delays."),
        _ => throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}."),
    };
}
