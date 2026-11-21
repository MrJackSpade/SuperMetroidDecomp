using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// A Wave or Hyper shot's sideways move over terrain, as a projectile system's terrain collision makes it, for checks of the
/// block reactions its leading edge sets off.
/// </summary>
internal static class WaveShotTerrainMoves
{
    /// <summary>
    /// Moves <paramref name="held"/> by its sideways velocity and scans its leading edge's rows over
    /// <paramref name="terrain"/>, the straddled blocks past the room's ceiling read from <paramref name="straddled"/>, as the
    /// terrain collision of a projectile system built over that straddled terrain does.
    /// </summary>
    internal static void MoveWaveShotHorizontally(SamusTerrain terrain, SamusProjectileOccupant held, StraddledTerrain<RoomTerrainBlock> straddled) =>
        new ProjectileTerrainCollision(
                new ProjectileAnimation(new BeamChargeState(),
                    new SamusShotPool(new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount()), InstalledFixtures.SamusArtwork()),
                new ProjectileEarthquakes(new RoomEarthquake()),
                new ProjectileImpactAudio(GameplayAudioRequests.Standalone()),
                straddled)
            .MoveWaveShotHorizontally(terrain, held);
}
