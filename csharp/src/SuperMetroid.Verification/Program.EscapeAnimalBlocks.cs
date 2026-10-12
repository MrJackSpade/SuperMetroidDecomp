using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static EscapeRoomEffectsVerification;
using static Program;
using static SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using static GrappleDoorsVerification;

/// <summary>Represents escape animal blocks verification.</summary>
internal static class EscapeAnimalBlocksVerification
{
    /// <summary>Verifies escape animal blocks.</summary>
    internal static void VerifyEscapeAnimalBlocks(string romPath)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        ushort[] words = Enumerable.Repeat((ushort)0x8123, 256).ToArray();
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(words, new byte[256], new ushort[256]), new byte[8192]);
        var streamer = new BackgroundTilemapStreamer(16, words, new ushort[256], new byte[8192]);
        var plms = new RoomPlmSystem();
        var system = new Bank80SystemState();
        // The real escape room population installs event ownership; its grey door may
        // continue stepping alongside the rescue animation, as it does in gameplay.
        plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, 0x8412), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => true }, Events = new FixtureEventFlags { HasEvent = system.HasEvent, SetEvent = system.SetEvent } });
        int origin = level.GetBlockIndex(new(15, 10));
        plms.EscapeAnimals.SetupCrittersEscapeBlock(level, origin);
        AssertEqual((byte)0x4f, level.GetCollisionBlock(new(15, 10)).Behavior, "rescue origin is shootable BTS 4F");
        for (int y = 11; y <= 12; y++)
            AssertEqual((ushort)0xd123, level.GetCollisionBlock(new(15, checked((ushort)y))).LevelWord, "rescue extensions preserve visual tile");
        for (int y = 10; y <= 12; y++)
        {
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(248, (ushort)(y * 16 + 8))) };
            SeedStationaryGrappleCollisionProbe(bus, samus);
            int before = plms.SlotTable.ActiveCount;
            SamusGrappleMovement.StepFiring(bus, level, samus,
                (ushort)SuperMetroid.Core.Input.SnesButton.X, plms);
            AssertEqual(GrapplePhase.CancelPending, samus.Grapple.Phase,
                "grapple stops on rescue origin or extension");
            AssertEqual(before, plms.SlotTable.ActiveCount, "zero grapple word cannot allocate the rescue animation");
            AssertEqual((byte)0x4f, level.GetCollisionBlock(new(15, 10)).Behavior, "grapple leaves rescue wall intact");
            AssertTrue(!system.HasEvent(EventNumber.CrittersEscaped), "grapple alone cannot rescue animals");
        }
        AssertTrue(!plms.TrySpawnProjectileShotBlock(level, origin, (byte)0x4f, 0, true), "native zero grapple word rejected");
        AssertTrue(plms.TrySpawnProjectileShotBlock(level, origin, (byte)0x4f, 1, true), "nonzero beam opens rescue wall");
        AssertTrue(!system.HasEvent(EventNumber.CrittersEscaped), "rescue event waits for animation");
        for (int frame = 0; frame < 16; frame++)
            plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(system.HasEvent(EventNumber.CrittersEscaped), "native animation sets rescue event");
        for (int y = 10; y <= 12; y++)
            AssertEqual((ushort)0x80ff, level.GetCollisionBlock(new(15, checked((ushort)y))).LevelWord, "native rescue opening clears artwork but remains solid to Samus");
        Console.WriteLine("Animal rescue: native setup, ROM break animation, cleared wall and event verified.");
    }

    /// <summary>Escape animals command.</summary>
    [VerificationCommand("--escape-animals", "argument1")]
    internal static int EscapeAnimalsCommand(string[] args)
    {
        Suite(nameof(VerifyEscapeAnimalBlocks), () => VerifyEscapeAnimalBlocks(args[1]));
        Suite(nameof(VerifyEscapeRoomEffects), () => VerifyEscapeRoomEffects(args[1]));
        return 0;
    }
}
