using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyEscapeAnimalBlocks(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        ushort[] words = Enumerable.Repeat((ushort)0x8123, 256).ToArray();
        var level = new RoomLevelData(16, 16, words, new byte[256], new ushort[256], new byte[8192]);
        var streamer = new BackgroundTilemapStreamer(16, words, new ushort[256], new byte[8192]);
        var plms = new RoomPlmSystem();
        var system = new Bank80SystemState();
        // The real escape room population installs event ownership; its grey door may
        // continue stepping alongside the rescue animation, as it does in gameplay.
        plms.LoadRoomPopulation(bus, level, streamer, new SnesVram(), 0x8412,
            system, AreaId.Crateria, () => null, () => true,
            hasEvent: system.HasEvent, setEvent: system.SetEvent);
        int origin = level.GetBlockIndex(15, 10);
        plms.SetupCrittersEscapeBlock(level, origin);
        AssertEqual((byte)0x4f, level.GetCollisionBlock(15, 10).Behavior, "rescue origin is shootable BTS 4F");
        for (int y = 11; y <= 12; y++)
            AssertEqual((ushort)0xd123, level.GetCollisionBlock(15, y).LevelWord, "rescue extensions preserve visual tile");
        for (int y = 10; y <= 12; y++)
        {
            var samus = new SamusState { XPosition = 248, YPosition = (ushort)(y * 16 + 8) };
            samus.Grapple.Phase = GrapplePhase.Firing;
            int before = plms.ActiveCount;
            var reaction = SamusGrappleMovement.StepFiring(bus, level, samus,
                (ushort)SuperMetroid.Core.Input.SnesButton.X, plms);
            AssertTrue(reaction.CancelQueued, "grapple stops on rescue origin or extension");
            AssertEqual(before, plms.ActiveCount, "zero grapple word cannot allocate the rescue animation");
            AssertEqual((byte)0x4f, level.GetCollisionBlock(15, 10).Behavior, "grapple leaves rescue wall intact");
            AssertTrue(!system.HasEvent(EventNumber.CrittersEscaped), "grapple alone cannot rescue animals");
        }
        AssertTrue(!plms.TrySpawnProjectileShotBlock(level, origin, (byte)0x4f, 0, true), "native zero grapple word rejected");
        AssertTrue(plms.TrySpawnProjectileShotBlock(level, origin, (byte)0x4f, 1, true), "nonzero beam opens rescue wall");
        AssertTrue(!system.HasEvent(EventNumber.CrittersEscaped), "rescue event waits for animation");
        for (int frame = 0; frame < 16; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertTrue(system.HasEvent(EventNumber.CrittersEscaped), "native animation sets rescue event");
        for (int y = 10; y <= 12; y++)
            AssertEqual((ushort)0x80ff, level.GetCollisionBlock(15, y).LevelWord, "native rescue opening clears artwork but remains solid to Samus");
        Console.WriteLine("Animal rescue: native setup, ROM break animation, cleared wall and event verified.");
    }
}
