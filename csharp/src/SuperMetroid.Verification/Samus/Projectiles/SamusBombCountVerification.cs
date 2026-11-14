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
/// Checks the count of bombs Samus has out (#1627): it holds 0 through the bomb slots' count, a Shoot edge at a full count
/// places nothing and leaves the count full, and a bomb that ends after the Shitroid's drain cleared the count leaves it at
/// zero.
/// </summary>
internal static class SamusBombCountVerification
{
    /// <summary>The updates a placed bomb's fuse and explosion take at most before its slot is free.</summary>
    private const int LongestBombLife = 200;

    /// <summary>Exercises the count's range, its cap at the slot count and its stay at zero.</summary>
    internal static void VerifySamusBombCount()
    {
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16),
            new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]),
            RoomLevelData.NothingBeyondGrid());
        var terrain = new SamusTerrain { Level = level };

        SamusBombProjectileSystem range = CreateBombFixture();
        range.SetSharedBombCounter(SamusBombProjectileSystem.SlotCount);
        AssertEqual(SamusBombProjectileSystem.SlotCount, range.BombCounter, "the count holds every slot's bomb");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SetSharedBombCounter(SamusBombProjectileSystem.SlotCount + 1),
            "no more bombs are out than there are slots");
        AssertThrows<ArgumentOutOfRangeException>(() => range.SetSharedBombCounter(-1), "the count is never below zero");

        // A full count rejects a fresh Shoot edge even with no cooldown running: nothing is placed and the count stays full.
        SamusBombProjectileSystem full = CreateBombFixture();
        full.SetSharedBombCounter(SamusBombProjectileSystem.SlotCount);
        full.StepFrame(terrain, MorphedSamus(), ControllerButton.X, ControllerButton.X);
        AssertEqual(SamusBombProjectileSystem.SlotCount, full.BombCounter, "a full count stays full");
        AssertTrue(full.Slots.All(slot => !slot.IsActive), "a full count places no bomb");

        // The Shitroid's drain clears the count while a bomb is out; the bomb's end then leaves it at zero.
        SamusBombProjectileSystem cleared = CreateBombFixture();
        SamusState samus = MorphedSamus();
        cleared.StepFrame(terrain, samus, ControllerButton.X, ControllerButton.X);
        AssertEqual(1, cleared.BombCounter, "a placed bomb counts one");
        cleared.SetSharedBombCounter(0);
        for (int update = 0; update < LongestBombLife && cleared.Slots[0].IsActive; update++)
        {
            cleared.AudioRequests.BeginUpdate();
            cleared.StepFrame(terrain, samus, 0, 0);
        }
        AssertTrue(!cleared.Slots[0].IsActive, "the bomb ends");
        AssertEqual(0, cleared.BombCounter, "a bomb ending at a cleared count leaves it at zero");
        Console.WriteLine("Samus bomb count: holds 0 through the slot count, a full count places nothing, and a cleared count stays at zero.");
    }

    /// <summary>A morphed Samus with bombs, standing in the open.</summary>
    private static SamusState MorphedSamus()
    {
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new BeamChargeState())
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs,
            Position = FixedPosition<RoomPixels>.FromWhole(new(80, 80)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        return samus;
    }
}
