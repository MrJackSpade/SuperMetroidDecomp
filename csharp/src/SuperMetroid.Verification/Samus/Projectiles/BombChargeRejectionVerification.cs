using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks native bomb-helper rejection and beam-charge cancellation in morph ball.</summary>
internal static class BombChargeRejectionVerification
{
    /// <summary>Distinguishes helper rejection from delayed full-charge spread release across equipment and input edges.</summary>
    internal static void VerifyBombChargeRejection()
    {
        Suite(nameof(VerifyBombChargeRelease), () => VerifyBombChargeRelease());
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]), RoomLevelData.NothingBeyondGrid());
        foreach (ushort charge in new ushort[] { 0, 58, 60 })
        foreach (bool bombsEquipped in new[] { false, true })
        foreach (bool powerBombSelected in new[] { false, true })
        foreach (bool freshShoot in new[] { false, true })
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = SamusPoseId.MorphBallGroundRightPose,
                EquippedItems = (SamusEquipmentFlags.MorphBall | (bombsEquipped ? SamusEquipmentFlags.Bombs : 0)),
                SelectedHudItem = powerBombSelected ? HudItem.PowerBomb : HudItem.Beam,
                PowerBombs = 2,
                Position = FixedPosition<RoomPixels>.FromWhole(new(80, 80)),
            }.WithBeamChargeCount(charge);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            var bombs = CreateBombFixture();
            // Down postpones a valid full-charge spread. The no-edge rejection below
            // therefore belongs only to the ordinary-bomb/Power-Bomb helper, not to
            // releasing a spread. Equipment guards that skip that helper preserve charge.
            bool chargeConsumed = bombs.StepFrameConsumingCharge(new SamusTerrain { Level = level }, samus, (ControllerButton.X | ControllerButton.Down), freshShoot ? ControllerButton.X : 0);
            bool helperRuns = powerBombSelected || bombsEquipped && charge < 60;
            bool cancelled = helperRuns && !freshShoot && charge != 0;
            string context = $"charge={charge}, Bombs={bombsEquipped}, PB={powerBombSelected}, edge={freshShoot}";
            AssertEqual(cancelled, chargeConsumed, $"native bomb rejection charge bridge: {context}");
            var cancellations = bombs.AudioRequests.SoundRequests.Where(sound => sound ==
                new SoundRequest(SamusSound.Silence, SoundRequestPriority.High)).ToArray();
            AssertEqual(cancelled ? 1 : 0, cancellations.Length, $"single native charge cancellation sound: {context}");
            AssertEqual(helperRuns && freshShoot ? 1 : 0, bombs.BombCounter,
                $"rejection must not allocate a projectile: {context}");
        }
    }

    /// <summary>Checks that releasing Shoot cancels charge without placing a bomb or spending Power Bombs.</summary>
    private static void VerifyBombChargeRelease()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]), RoomLevelData.NothingBeyondGrid());
        foreach (ushort charge in new ushort[] { 0, 1, 59, 60 })
        foreach (ushort selection in new ushort[] { 0, 3 })
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = SamusPoseId.MorphBallGroundRightPose,
                EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
                SelectedHudItem = SuperMetroid.Diagnostics.Saves.SaveSlotNumbering.SavedHudItem(selection), PowerBombs = 2,
                Position = FixedPosition<RoomPixels>.FromWhole(new(80, 80)) }.WithBeamChargeCount(charge);
            var bombs = CreateBombFixture();
            bool chargeConsumed = bombs.StepFrameConsumingCharge(new SamusTerrain { Level = level }, samus, ControllerButton.Down, 0);
            string context = $"Shoot released, charge {charge}, HUD selection {selection}";
            AssertEqual(charge != 0, chargeConsumed, $"outer native cancellation command: {context}");
            AssertEqual(charge != 0 ? 1 : 0, bombs.AudioRequests.SoundRequests.Count(sound => sound ==
                new SoundRequest(SamusSound.Silence, SoundRequestPriority.High)), $"charge cancellation sound: {context}");
            AssertEqual(0, bombs.BombCounter, $"release never places a bomb: {context}");
            AssertEqual(2, samus.PowerBombs, $"release never spends ammunition: {context}");
        }
    }
}
