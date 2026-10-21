using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Spatial;

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
            SeedRoomPlmPopulationRom(bus);
            PlmHeaderId header = missile ? PlmHeaderId.MissileStation : PlmHeaderId.EnergyStation;
            SeedRoomPlmPopulation(bus, 0x9000, [(byte)(ushort)header, SnesLittleEndian.HighByte((ushort)header), 8, 8, 0, 0, 0, 0]);
            var level = CreateRoom(new(32, 16), new ushort[512], new byte[512],
                blockDefinitions: new byte[0x400 * 8]);
            var samus = new SamusState { Health = 199, MaxHealth = 199, Missiles = 15, MaxMissiles = 15 };
            var plms = PlmFixtures.Unbound();
            PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
            int accessX = right ? 9 : 7;
            var behavior = new RoomBlockBehavior(level.GetCollisionBlock(new(checked((ushort)accessX), 8)).Bts.Value);
            bool Touch() => plms.Stations.TryNotifyStationCollision(level.GetBlockIndex(new(checked((ushort)accessX), 8)), behavior,
                (right ? SamusPoseId.RanIntoWallLeftPose : SamusPoseId.RanIntoWallRightPose).ForRoomCollision(),
                Axis.Horizontal, movingPositive: !right, roomWidthInBlocks: 32);
            AssertTrue(Touch(), "full recharge station remains solid");
            AssertTrue(!samus.Control.AlphaLocked, "full resource must be rejected before station input lock");
            plms.Step(level);
            AssertTrue(!samus.Control.AlphaLocked && plms.Stations.StationActivationEvents.Count == 0,
                "rejected recharge starts no operation and leaves controller enabled");

            if (missile) samus.Missiles--; else samus.Health--;
            AssertTrue(Touch() && samus.Control.AlphaLocked, "needed recharge takes control during collision setup");
            // The first access-list instruction rechecks fullness and explicitly
            // restores movement before deleting an access that no longer needs refill.
            if (missile) samus.Missiles = samus.MaxMissiles; else samus.Health = samus.MaxHealth;
            plms.Step(level);
            AssertEqual(0, plms.Stations.StationActivationEvents.Count, "full-resource access-list exit skips recharge");
            AssertTrue(!samus.Control.AlphaLocked, "full-resource access-list exit immediately restores controller input");
            AssertTrue(Touch() && !samus.Control.AlphaLocked, "touching completed station cannot relock input at full resource");
            if (missile) samus.Missiles = 1; else samus.Health = 25;
            AssertTrue(Touch() && samus.Control.AlphaLocked, "ordinary recharge still accepts depleted resource");
            for (int frame = 0; frame < 120; frame++)
                plms.Step(level);
            AssertEqual(missile ? samus.MaxMissiles : samus.MaxHealth,
                missile ? samus.Missiles : samus.Health, "ordinary recharge restores the depleted resource");
            AssertTrue(!samus.Control.AlphaLocked, "ordinary recharge unlocks after retraction");
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
