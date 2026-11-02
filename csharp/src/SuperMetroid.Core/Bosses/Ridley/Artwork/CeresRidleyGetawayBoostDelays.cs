using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The running delays each of the getaway's wing frames leaves behind for Samus's unauthored Speed Booster cadence
/// (<see cref="UnauthoredBoostCadence"/>, #1338). The import decodes them once from the player's game data, from what the
/// wing frame's floor update leaves where the unauthored cadence reads, so Core holds no floor update or queue position.
/// The capsule's frames leave nothing the cadence reads.
/// </summary>
public sealed class CeresRidleyGetawayBoostDelays
{
    /// <summary>Each wing frame's ten delays, by frame then running frame.</summary>
    private readonly int[][] wings;

    /// <summary>Keeps the delays the import pipeline validated, by wing frame then running frame.</summary>
    internal CeresRidleyGetawayBoostDelays(int[][] wings) => this.wings = wings;

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
