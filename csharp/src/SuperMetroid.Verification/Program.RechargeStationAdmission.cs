using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static AssertionsVerification;
using static RoomPlmPopulationVerification;
using static SyntheticRoomCartridgeFixturesVerification;
using static VerificationHelpersVerification;

/// <summary>Represents recharge station admission verification.</summary>
internal static class RechargeStationAdmissionVerification
{
    /// <summary>Verifies recharge station admission.</summary>
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
            var streamer = level.CreateBackgroundStreamer();
            var samus = new SamusState { Health = 199, MaxHealth = 199, Missiles = 15, MaxMissiles = 15 };
            var plms = new RoomPlmSystem();
            plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
            int accessX = right ? 9 : 7;
            var behavior = new RoomBlockBehavior(level.GetCollisionBlock(new(checked((ushort)accessX), 8)).Behavior);
            bool Touch() => plms.Stations.TryNotifyStationCollision(level.GetBlockIndex(new(checked((ushort)accessX), 8)), behavior,
                right ? SamusPoseId.RanIntoWallLeftPose : SamusPoseId.RanIntoWallRightPose,
                horizontal: true, movingPositive: !right, roomWidthInBlocks: 32);
            AssertTrue(Touch(), "full recharge station remains solid");
            AssertTrue(!samus.InputLocked, "full resource must be rejected before station input lock");
            plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertTrue(!samus.InputLocked && plms.Stations.StationActivationEvents.Count == 0,
                "rejected recharge starts no operation and leaves controller enabled");

            if (missile) samus.Missiles--; else samus.Health--;
            AssertTrue(Touch() && samus.InputLocked, "needed recharge takes control during collision setup");
            // The first access-list instruction rechecks fullness and explicitly
            // restores movement before deleting an access that no longer needs refill.
            if (missile) samus.Missiles = samus.MaxMissiles; else samus.Health = samus.MaxHealth;
            plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual(0, plms.Stations.StationActivationEvents.Count, "full-resource access-list exit skips recharge");
            AssertTrue(!samus.InputLocked, "full-resource access-list exit immediately restores controller input");
            AssertTrue(Touch() && !samus.InputLocked, "touching completed station cannot relock input at full resource");
            if (missile) samus.Missiles = 1; else samus.Health = 25;
            AssertTrue(Touch() && samus.InputLocked, "ordinary recharge still accepts depleted resource");
            for (int frame = 0; frame < 120; frame++)
                plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual(missile ? samus.MaxMissiles : samus.MaxHealth,
                missile ? samus.Missiles : samus.Health, "ordinary recharge restores the depleted resource");
            AssertTrue(!samus.InputLocked, "ordinary recharge unlocks after retraction");
        }
        Console.WriteLine("Recharge admission: full energy/missiles reject before lock; later-full exits unlock; depleted recharge completes from both sides.");
    }

    /// <summary>Recharge station admission command.</summary>
    [VerificationCommand("--recharge-station-admission")]
    internal static int RechargeStationAdmissionCommand()
    {
        Suite(nameof(VerifyRechargeStationAdmission), () => VerifyRechargeStationAdmission());
        return 0;
    }
}
