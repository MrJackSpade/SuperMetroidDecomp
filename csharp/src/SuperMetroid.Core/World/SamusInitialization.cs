using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.World;

/// <summary>
/// Initialises each new Samus, as the game does when a runtime starts, a saved game loads, the game starts at Ceres, Samus
/// arrives on Zebes after Ceres or an attract demo loads: a fresh Samus in her default state, and the camera's horizontal
/// measure of her movement set to one pixel (<see cref="YardKickTrackingDistance.RecordSamusInitialization"/>) until the
/// camera first tracks her. Her pose, place and inventory are set separately.
/// </summary>
/// <param name="audioRequests">Where each Samus queues her sounds.</param>
/// <param name="overreads">The Samus animation reads past their animations' ends that every Samus shares.</param>
/// <param name="unauthoredBoostCadence">The unauthored Speed Booster cadence every Samus shares.</param>
/// <param name="beamCharge">The beam charge every Samus of the runtime shares.</param>
/// <param name="artwork">The installed Samus artwork every Samus draws from.</param>
/// <param name="yardKickDistance">The camera's measure of Samus's horizontal movement, which a new Samus resets.</param>
internal sealed partial class SamusInitialization(GameplayAudioRequests audioRequests, SamusOverreadServices overreads,
    UnauthoredBoostCadence unauthoredBoostCadence, BeamChargeState beamCharge, SamusArtworkBinding artwork,
    YardKickTrackingDistance yardKickDistance)
{
    /// <summary>A fresh Samus in her default state, resetting the camera's measure of her movement.</summary>
    internal SamusState Initialize()
    {
        yardKickDistance.RecordSamusInitialization();
        return new SamusState(audioRequests, overreads.FramesBeyondAnimation, unauthoredBoostCadence, SamusCompatibility.ForNewSamus(),
            beamCharge, artwork);
    }
}
