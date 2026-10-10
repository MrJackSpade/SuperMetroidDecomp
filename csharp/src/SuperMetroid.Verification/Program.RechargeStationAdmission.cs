using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks energy and missile station admission from both sides, including full-resource rejection, mid-access completion, and depleted-resource recharge.</summary>
    private static void VerifyRechargeStationAdmission()
    {
        foreach (bool missile in new[] { false, true })
        foreach (bool right in new[] { false, true })
        {
            var bus = new TestAddressSpace();
            SeedRoomPlmPopulationRom(bus);
            ushort header = missile ? RoomPlmHeaders.MissileStation : RoomPlmHeaders.EnergyStation;
            bus.WriteBytes(0x8f9000, [(byte)header, (byte)(header >> 8), 8, 8, 0, 0, 0, 0]);
            var level = CreateRoom(32, 16, new ushort[512], new byte[512],
                blockDefinitions: new byte[0x400 * 8]);
            var streamer = level.CreateBackgroundStreamer();
            var samus = new SamusState { Health = 199, MaxHealth = 199, Missiles = 15, MaxMissiles = 15 };
            var plms = new RoomPlmSystem();
            plms.LoadRoomPopulation(bus, level, streamer, new SnesVram(),
                RoomPlmPopulationImporter.Read(bus, 0x9000), new Bank80SystemState(),
                areaIndex: AreaId.Crateria, getSamus: () => samus, isAreaTorizoDefeated: () => false);
            int accessX = right ? 9 : 7;
            var behavior = new RoomBlockBehavior(level.GetCollisionBlock(accessX, 8).Behavior);
            bool Touch() => plms.TryNotifyStationCollision(level.GetBlockIndex(accessX, 8), behavior,
                right ? SamusPoseIds.RanIntoWallLeftPose : SamusPoseIds.RanIntoWallRightPose,
                horizontal: true, movingPositive: !right, roomWidthInBlocks: 32);
            AssertTrue(Touch(), "full recharge station remains solid");
            AssertTrue(!samus.InputLocked, "full resource must be rejected before station input lock");
            plms.Step(bus, level, streamer, 0, 0, 0);
            AssertTrue(!samus.InputLocked && plms.StationActivationEvents.Count == 0,
                "rejected recharge starts no operation and leaves controller enabled");

            if (missile) samus.Missiles--; else samus.Health--;
            AssertTrue(Touch() && samus.InputLocked, "needed recharge takes control during collision setup");
            // The first access-list instruction rechecks fullness and explicitly
            // restores movement before deleting an access that no longer needs refill.
            if (missile) samus.Missiles = samus.MaxMissiles; else samus.Health = samus.MaxHealth;
            plms.Step(bus, level, streamer, 0, 0, 0);
            AssertEqual(0, plms.StationActivationEvents.Count, "full-resource access-list exit skips recharge");
            AssertTrue(!samus.InputLocked, "full-resource access-list exit immediately restores controller input");
            AssertTrue(Touch() && !samus.InputLocked, "touching completed station cannot relock input at full resource");
            if (missile) samus.Missiles = 1; else samus.Health = 25;
            AssertTrue(Touch() && samus.InputLocked, "ordinary recharge still accepts depleted resource");
            for (int frame = 0; frame < 120; frame++)
                plms.Step(bus, level, streamer, 0, 0, 0);
            AssertEqual(missile ? samus.MaxMissiles : samus.MaxHealth,
                missile ? samus.Missiles : samus.Health, "ordinary recharge restores the depleted resource");
            AssertTrue(!samus.InputLocked, "ordinary recharge unlocks after retraction");
        }
        Console.WriteLine("Recharge admission: full energy/missiles reject before lock; later-full exits unlock; depleted recharge completes from both sides.");
    }
}
