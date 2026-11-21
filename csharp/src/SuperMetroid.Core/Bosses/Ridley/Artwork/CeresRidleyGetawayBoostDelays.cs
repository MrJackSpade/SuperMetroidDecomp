using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The running delays each of the getaway's wing frames leaves behind for Samus's unauthored Speed Booster cadence
/// (<see cref="UnauthoredBoostCadence"/>, #1338). The import decodes them once from the player's game data, from what the
/// wing frame's floor update leaves where the unauthored cadence reads, so Core holds no floor update or queue position.
/// The capsule's frames leave nothing the cadence reads.
/// </summary>
public sealed class CeresRidleyGetawayBoostDelays
{
    /// <summary>The ten delays each wing frame leaves behind, by running frame, keyed by the wing frame.</summary>
    private readonly IReadOnlyDictionary<CeresFloorActorPicture, IReadOnlyList<int>> leftByWing;

    /// <summary>Keeps the delays the import pipeline validated for each wing frame, by running frame.</summary>
    /// <param name="leftByWing">Each wing frame's delays, keyed by the wing frame.</param>
    internal CeresRidleyGetawayBoostDelays(IReadOnlyDictionary<CeresFloorActorPicture, IReadOnlyList<int>> leftByWing) =>
        this.leftByWing = Ensure.NotNull(leftByWing);

    /// <summary>The ten delays wing frame <paramref name="frame"/> leaves behind, by running frame.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of the wing's frames.</exception>
    public IReadOnlyList<int> LeftBy(CeresFloorActorPicture frame) => leftByWing.TryGetValue(frame, out IReadOnlyList<int>? delays)
        ? delays
        : throw new InvalidDataException($"The getaway's {frame} floor frame leaves no Speed Booster delays.");
}
