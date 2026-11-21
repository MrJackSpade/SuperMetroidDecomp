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
    /// <summary>The ten delays each wing beat leaves behind, by running frame.</summary>
    private readonly IdentityTable<CeresGetawayWingBeat, IReadOnlyList<int>> leftByWings;

    /// <summary>Keeps the delays the import pipeline validated for each wing frame, by running frame.</summary>
    internal CeresRidleyGetawayBoostDelays(IReadOnlyList<int> leftByWingsRaised, IReadOnlyList<int> leftByWingsLowered) =>
        leftByWings = new(new Dictionary<CeresGetawayWingBeat, IReadOnlyList<int>>
        {
            [CeresGetawayWingBeat.Raised] = Ensure.NotNull(leftByWingsRaised),
            [CeresGetawayWingBeat.Lowered] = Ensure.NotNull(leftByWingsLowered),
        });

    /// <summary>The ten delays wing beat <paramref name="beat"/> leaves behind, by running frame.</summary>
    public IReadOnlyList<int> LeftBy(CeresGetawayWingBeat beat) => leftByWings[beat];
}
