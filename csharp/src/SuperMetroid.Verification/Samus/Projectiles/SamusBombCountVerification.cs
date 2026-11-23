using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Checks the count of bombs Samus has out (#1627): it holds 0 through the bomb places' count, a Shoot edge at a full count
/// places nothing and leaves the count full, a bomb that ends after the Baby Metroid's drain cleared the count leaves it at
/// zero, and a count out of step with the bombs out places its bomb in the last place, over the bomb there, as the game's
/// search for a place ends on the last place (#2553).
/// </summary>
internal static class SamusBombCountVerification
{
    /// <summary>The updates a placed bomb's fuse and explosion take at most before its place is free.</summary>
    private const int LongestBombLife = 200;

    /// <summary>Exercises the count's fill, its cap at the place count, its stay at zero and its inconsistency.</summary>
    internal static void VerifySamusBombCount()
    {
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16),
            new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]),
            RoomLevelData.NothingBeyondGrid(), []);
        var terrain = new RoomTerrain(level, SamusRoomObjectFixtures.Uninstalled);

        // A bomb spread fills every place, and the count holds every place's bomb.
        SamusBombProjectileSystem range = CreateBombFixture();
        LaySpread(range, MorphedSamus());
        AssertEqual(SamusBombProjectileSystem.PlaceCount, range.BombCounter, "the count holds every place's bomb");

        // A full count rejects a fresh Shoot edge even with no cooldown running: nothing is placed and the count stays full.
        SamusBombProjectileSystem full = CreateBombFixture();
        full.HoldCountForDrain();
        full.StepFrame(terrain, MorphedSamus(), GameplayAction.Shoot, GameplayAction.Shoot);
        AssertEqual(SamusBombProjectileSystem.PlaceCount, full.BombCounter, "a full count stays full");
        AssertTrue(full.Bombs.All(bomb => !bomb.IsActive), "a full count places no bomb");

        // The Baby Metroid's drain clears the count while a bomb is out; the bomb's end then leaves it at zero.
        SamusBombProjectileSystem cleared = CreateBombFixture();
        SamusState samus = MorphedSamus();
        cleared.StepFrame(terrain, samus, GameplayAction.Shoot, GameplayAction.Shoot);
        AssertEqual(1, cleared.BombCounter, "a placed bomb counts one");
        cleared.ReleaseCountAfterDrain();
        for (int update = 0; update < LongestBombLife && cleared.Bombs[0].IsActive; update++)
        {
            cleared.AudioRequests.BeginRecordedUpdate();
            cleared.StepFrame(terrain, samus, 0, 0);
        }
        AssertTrue(!cleared.Bombs[0].IsActive, "the bomb ends");
        AssertEqual(0, cleared.BombCounter, "a bomb ending at a cleared count leaves it at zero");

        // A count cleared while a spread holds every place admits a bomb with no place free: the game's search for a place
        // ends on the last place, so the new bomb replaces the spread's bomb there and the others keep their places.
        SamusBombProjectileSystem inconsistent = CreateBombFixture();
        SamusState spreading = MorphedSamus();
        LaySpread(inconsistent, spreading);
        inconsistent.ReleaseCountAfterDrain();
        inconsistent.StepFrame(terrain, spreading, GameplayAction.Shoot, GameplayAction.Shoot);
        AssertEqual(1, inconsistent.BombCounter, "a bomb admitted at a cleared count counts one");
        SamusBomb last = inconsistent.Bombs[SamusBombProjectileSystem.PlaceCount - 1];
        AssertTrue(last.IsOccupied && !last.IsBombSpread,
            "a bomb admitted with every place held takes the last place over the spread's bomb there");
        AssertTrue(inconsistent.Bombs.Take(SamusBombProjectileSystem.PlaceCount - 1).All(bomb => bomb.IsBombSpread),
            "the spread's other bombs keep their places");
        Console.WriteLine("Samus bomb count: a spread fills the count, a full count places nothing, a cleared count stays at zero, and an inconsistent count places its bomb in the last place.");
    }

    /// <summary>
    /// Lays a bomb spread as the game does: Samus holds her charge for the updates a full charge takes, then holds Shoot in
    /// her ball with no bomb out, and the HUD step's bomb producer releases the spread.
    /// </summary>
    private static void LaySpread(SamusBombProjectileSystem bombs, SamusState samus)
    {
        for (int update = 0; update < BeamChargeDefinitions.FullyChargedCounter; update++)
            samus.BeamCharge.CountChargeUpdate();
        bombs.TickCooldownAndPlaceBombs(samus, GameplayAction.Shoot, GameplayAction.None);
        AssertTrue(bombs.Bombs.All(bomb => bomb.IsBombSpread), "a full charge released in the ball lays the spread in every place");
    }

    /// <summary>A morphed Samus with bombs, standing in the open.</summary>
    private static SamusState MorphedSamus()
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            EquippedItems = SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs,
            Position = FixedPosition<RoomPixels>.FromWhole(new(80, 80)),
        }.InPose(SamusPoseId.MorphBallGroundRightPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        return samus;
    }
}
