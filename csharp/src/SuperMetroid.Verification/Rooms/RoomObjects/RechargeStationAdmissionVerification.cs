using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectPopulationVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World;

/// <summary>Checks admission and input-lock behavior for energy and missile stations approached from either side.</summary>
internal static class RechargeStationAdmissionVerification
{
    /// <summary>Confirms full resources reject access, a later-full access list unlocks Samus, and depleted resources still recharge.</summary>
    internal static void VerifyRechargeStationAdmission()
    {
        foreach (bool missile in new[] { false, true })
        foreach (bool right in new[] { false, true })
        {
            var bus = new TestAddressSpace();
            SeedRoomObjectPopulationRom(bus);
            RoomObjectKind header = missile ? RoomObjectKind.MissileStation : RoomObjectKind.EnergyStation;
            SeedRoomObjectPopulation(bus, 0x9000, [(byte)RoomObjectNativeWords.Word(header), LittleEndianBytes.HighByte(RoomObjectNativeWords.Word(header)), 8, 8, 0, 0, 0, 0]);
            var level = CreateRoom(new(32, 16), new RoomTerrainBlock[512], new RoomBlockBehavior[512]);
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 199, MaxHealth = 199, Missiles = 15, MaxMissiles = 15 };
            var roomObjects = RoomObjectFixtures.Unbound();
            RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria), RoomObjectPopulationImporter.Read(bus, 0x9000), new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
            int accessX = right ? 9 : 7;
            var behavior = RoomBlockBehaviorCodec.Decode(RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(checked((ushort)accessX), 8)).Behavior));
            bool Touch() => roomObjects.Stations.TryNotifyStationCollision(level, new(accessX, 8), behavior,
                (right ? SamusPoseId.RanIntoWallLeftPose : SamusPoseId.RanIntoWallRightPose).ForRoomCollision(),
                Axis.Horizontal, movingPositive: !right);
            AssertTrue(Touch(), "full recharge station remains solid");
            AssertTrue(!samus.Control.InputLocked, "full resource must be rejected before station input lock");
            roomObjects.Step(level);
            AssertTrue(!samus.Control.InputLocked && roomObjects.Stations.StationActivationEvents.Count == 0,
                "rejected recharge starts no operation and leaves controller enabled");

            if (missile) samus.Missiles--; else samus.Health--;
            AssertTrue(Touch() && samus.Control.InputLocked, "needed recharge takes control during collision setup");
            // The first access-list instruction rechecks fullness and explicitly
            // restores movement before deleting an access that no longer needs refill.
            if (missile) samus.Missiles = samus.MaxMissiles; else samus.Health = samus.MaxHealth;
            roomObjects.Step(level);
            AssertEqual(0, roomObjects.Stations.StationActivationEvents.Count, "full-resource access-list exit skips recharge");
            AssertTrue(!samus.Control.InputLocked, "full-resource access-list exit immediately restores controller input");
            AssertTrue(Touch() && !samus.Control.InputLocked, "touching completed station cannot relock input at full resource");
            if (missile) samus.Missiles = 1; else samus.Health = 25;
            AssertTrue(Touch() && samus.Control.InputLocked, "ordinary recharge still accepts depleted resource");
            for (int frame = 0; frame < 120; frame++)
                roomObjects.Step(level);
            AssertEqual(missile ? samus.MaxMissiles : samus.MaxHealth,
                missile ? samus.Missiles : samus.Health, "ordinary recharge restores the depleted resource");
            AssertTrue(!samus.Control.InputLocked, "ordinary recharge unlocks after retraction");
        }
        Console.WriteLine("Recharge admission: full energy/missiles reject before lock; later-full exits unlock; depleted recharge completes from both sides.");
    }

    /// <summary>Runs the focused station admission and controller unlock checks.</summary>
    [VerificationCommand("--recharge-station-admission")]
    internal static int RechargeStationAdmissionCommand()
    {
        Suite(nameof(VerifyRechargeStationAdmission), () => VerifyRechargeStationAdmission());
        return 0;
    }
}
