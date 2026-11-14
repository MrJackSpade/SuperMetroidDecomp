using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using static SuperMetroid.Verification.Fx.WaveDoorBandsVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks beam impact and gate scan termination in the Kronic Boost room.</summary>
internal static class GateBeamCollisionVerification
{
    /// <summary>Checks native impact position and same-frame instruction advance without waking the wrong-side switch.</summary>
    internal static void VerifyKronicGateBeamCollision()
    {
        Suite(nameof(VerifyGateScanTermination), () => VerifyGateScanTermination());
        Suite(nameof(VerifyHorizontalWaveDoorBands), () => VerifyHorizontalWaveDoorBands());
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.KronicBoost);
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(140, 360)),
            PoseId = SamusPoseId.NormalJumpAimDiagonalUpLeftPose };
        var level = runtime.Room.LoadedLevelData;
        var roomObjects = runtime.RoomObjects;
        for (int warm = 0; warm < 2; warm++) roomObjects.Step(level);
        var shots = CreateProjectileFixture();
        var bombs = CreateBombFixture();
        ControllerButton input = ControllerButton.X;
        bombs.StepFrame(new SamusTerrain { Level = level }, samus, input, input);
        shots.StepFrame(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new(0, 224), bombs);
        var shot = shots.Slots[0];
        AssertEqual(SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false).AsFamily(SamusProjectileFamily.BeamExplosion), shot.Type, "Native beam impacts gate during producer collision");
        AssertEqual((ushort)113, shot.Position.X.Whole, "Native impact X, before any continuing flight");
        AssertEqual((ushort)329, shot.Position.Y.Whole, "Native impact Y");
        AssertEqual(0xa00f, shot.InstructionPointer, "Same-frame native impact instruction advancement");
        AssertTrue(roomObjects.PopulationSlots.Where(s => s.Behaviour is DownwardGateObject)
            .All(s => s.Behaviour is DownwardGateObject { Triggered: false }), "Beam cannot wake the wrong-side switch through the gate body");
        Console.WriteLine("PASS Kronic Boost native initial gate-beam impact and no switch activation.");
    }

    /// <summary>Checks vertical and horizontal wave or ordinary scans stop after the first gate probe.</summary>
    private static void VerifyGateScanTermination()
    {
        // Two adjacent gate probes make an unwanted second scan observable as a
        // second allocation. Air before the first gate also tests forced collision.
        foreach (bool horizontal in new[] { false, true })
        foreach (bool wave in new[] { false, true })
        foreach (bool full in new[] { false, true })
        {
            var words = new RoomTerrainBlock[256];
            var bts = new RoomBlockBehavior[256];
            int first = horizontal ? 33 : 18;
            int second = horizontal ? 49 : 19;
            words[first] = words[second] = RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.ShootableBlock);
            bts[first] = bts[second] = RoomBlockBehaviorCodec.Decode(0x10);
            var level = CreateRoom(new(16, 16), words, bts);
            var roomObjects = RoomObjectFixtures.Unbound();
            if (full)
                for (int i = 0; i < 40; i++)
                    AssertTrue(roomObjects.TrySpawnProjectileShotBlock(level, level.BlockAt(first), 0x10, SamusProjectileType.None, true), "Fill gate probe pool");
            var shot = new SamusProjectile(0) { Position = new(new(33, 0), new(33, 0)),
                Radius = new(17, 17), Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-1, -1) };
            bool result = false;
            if (wave && horizontal) ProjectileTerrainCollision.ScanHorizontalWaveShotReactions(level, shot, roomObjects, InstalledFixtures.SamusOverreads.StraddledTerrain);
            else if (wave) ProjectileTerrainCollision.ScanVerticalWaveShotReactions(level, shot, roomObjects);
            else result = horizontal
                ? ProjectileTerrainCollision.ScanHorizontalShotReactions(level, shot, roomObjects)
                : ProjectileTerrainCollision.ScanVerticalShotReactions(level, shot, roomObjects);
            AssertEqual(full ? 40 : 1, roomObjects.SlotTable.ActiveCount, "Gate setup terminates span after first allocation");
            if (!wave) AssertEqual(!full, result, "Only executed gate setup overrides earlier air reaction");
            AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.ShootableBlock), level.GetCollisionBlockByIndex(first).Terrain, "Gate scan does not mutate terrain");
        }
    }

    /// <summary>Runs the gate beam impact and scan termination regression.</summary>
    [VerificationCommand("--gate-beam-collision")]
    internal static int GateBeamCollisionCommand()
    {
        Suite(nameof(VerifyKronicGateBeamCollision), () => VerifyKronicGateBeamCollision());
        return 0;
    }
}
