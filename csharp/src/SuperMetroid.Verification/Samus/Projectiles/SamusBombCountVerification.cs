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
/// places nothing and leaves the count full, a bomb that ends after the Shitroid's drain cleared the count leaves it at
/// zero, and a count out of step with the bombs out fails loudly instead of reusing a place (#1919).
/// </summary>
internal static class SamusBombCountVerification
{
    /// <summary>The updates a placed bomb's fuse and explosion take at most before its place is free.</summary>
    private const int LongestBombLife = 200;

    /// <summary>Exercises the count's range, its cap at the place count, its stay at zero and its inconsistency.</summary>
    internal static void VerifySamusBombCount()
    {
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16),
            new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]),
            RoomLevelData.NothingBeyondGrid(), []);
        var terrain = new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled };

        SamusBombProjectileSystem range = CreateBombFixture();
        global::SuperMetroid.Core.PrivateState.Set(range, "BombCounter", (int)(SamusBombProjectileSystem.PlaceCount));
        AssertEqual(SamusBombProjectileSystem.PlaceCount, range.BombCounter, "the count holds every place's bomb");
        AssertThrows<ArgumentOutOfRangeException>(() => global::SuperMetroid.Core.PrivateState.Set(range, "BombCounter", (int)(SamusBombProjectileSystem.PlaceCount + 1)),
            "no more bombs are out than there are places");
        AssertThrows<ArgumentOutOfRangeException>(() => global::SuperMetroid.Core.PrivateState.Set(range, "BombCounter", (int)(-1)), "the count is never below zero");

        // A full count rejects a fresh Shoot edge even with no cooldown running: nothing is placed and the count stays full.
        SamusBombProjectileSystem full = CreateBombFixture();
        full.HoldCountForDrain();
        full.StepFrame(terrain, MorphedSamus(), GameplayAction.Shoot, GameplayAction.Shoot);
        AssertEqual(SamusBombProjectileSystem.PlaceCount, full.BombCounter, "a full count stays full");
        AssertTrue(full.Bombs.All(bomb => !bomb.IsActive), "a full count places no bomb");

        // The Shitroid's drain clears the count while a bomb is out; the bomb's end then leaves it at zero.
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

        // A count cleared while a spread holds every place admits a bomb with no place free, which fails loudly.
        SamusBombProjectileSystem inconsistent = CreateBombFixture();
        SamusState spreading = MorphedSamus();
        inconsistent.SpawnBombSpread(spreading);
        inconsistent.ReleaseCountAfterDrain();
        AssertThrows<InvalidOperationException>(
            () => inconsistent.StepFrame(terrain, spreading, GameplayAction.Shoot, GameplayAction.Shoot),
            "a bomb admitted with every place held fails instead of reusing the last place");
        Console.WriteLine("Samus bomb count: holds 0 through the place count, a full count places nothing, a cleared count stays at zero, and an inconsistent count fails.");
    }

    /// <summary>A morphed Samus with bombs, standing in the open.</summary>
    private static SamusState MorphedSamus()
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs,
            Position = FixedPosition<RoomPixels>.FromWhole(new(80, 80)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        return samus;
    }
}
