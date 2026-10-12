using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Spatial;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;
using static YardAirborneVerification;
using static YardKickVerification;

internal static class YardTrajectoriesVerification
{
    internal static void VerifyYardTrajectories()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, 0xd5a7);
        var assets = LoadFixtureRoomAssets(bus, room);
        Suite(nameof(VerifyYardLanding), () => VerifyYardLanding(bus, room));
        Suite(nameof(VerifyYardRuntimeDistancePublication), () => VerifyYardRuntimeDistancePublication());
        Suite(nameof(VerifyYardKickWords), () => VerifyYardKickWords(bus, room));
        Suite(nameof(VerifyYardAirborneTrajectories), () => VerifyYardAirborneTrajectories(bus, room));
        for (int focus = 0; focus < 5; focus++)
        {
            var enemies = new RoomEnemySystem { TileArtwork = FixtureEnemyTileArtwork() };
            var random = new Bank80SystemState();
            enemies.Load(new RoomEnemyLoadRequest(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, new SnesVram(), new SnesCgram(), new StandaloneRoomEnemyServices(random.NextRandom) { SeedRandom = random.SetRandomNumber }));
            var actor = enemies.Slots[focus];
            var state = enemies.YardStates[focus]!;
            var samus = new SamusState { Health = 999, MaxHealth = 999,
                Pose = SamusPoseId.FacingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(actor.Position.X.Whole - 64)), actor.Position.Y.Whole)) };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            int largest = 0;
            for (int frame = 0; frame < 1200; frame++)
            {
                ushort x = actor.Position.X.Whole, y = actor.Position.Y.Whole;
                var function = state.MovementFunction;
                ushort cameraX = unchecked((ushort)Math.Max(0, x - 128)), cameraY = unchecked((ushort)Math.Max(0, y - 112));
                enemies.StepFrame(new RoomEnemyFrame { Camera = new(cameraX, cameraY), TimeIsFrozen = false, Samus = samus, Level = assets.LevelData });
                int distance = Math.Max(Math.Abs(unchecked((short)(actor.Position.X.Whole - x))), Math.Abs(unchecked((short)(actor.Position.Y.Whole - y))));
                if (distance > largest)
                {
                    largest = distance;
                    Console.WriteLine($"Yard {focus}, frame {frame}, {function}: ({x},{y})->({actor.Position.X.Whole},{actor.Position.Y.Whole}), crawl={state.CrawlingVelocity.X.RawValue:X4}/{state.CrawlingVelocity.Y.RawValue:X4}, airborne={state.AirborneVelocity.X.WholeWord:X4}.{state.AirborneVelocity.X.Fraction:X4}/{state.AirborneVelocity.Y.WholeWord:X4}.{state.AirborneVelocity.Y.Fraction:X4}");
                }
            }
            Console.WriteLine($"Yard {focus}: maximum axis step {largest}");
        }
    }

    private static void VerifyYardLanding(SuperMetroidAddressSpace bus, CartridgeRoomHeader room)
    {
        var words = new ushort[32 * 32];
        for (int x = 0; x < 32; x++) words[16 * 32 + x] = 0x8000;
        var floor = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);
        foreach (ushort direction in Enumerable.Range(0, 8).Select(x => (ushort)x))
        foreach (ushort facing in new ushort[] { 0, 1 })
        {
            var enemies = new RoomEnemySystem { TileArtwork = FixtureEnemyTileArtwork() };
            enemies.Load(new RoomEnemyLoadRequest(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, new SnesVram(), new SnesCgram(), new StandaloneRoomEnemyServices(() => 0)));
            var actor = enemies.Slots[0];
            var state = enemies.YardStates[0]!;
            actor.Position = FixedPosition<RoomPixels>.FromWhole(new(256, unchecked((ushort)(256 - actor.Radius.Y))));
            state.Direction = direction;
            state.AirborneFacingDirection = facing;
            state.MovementFunction = YardMovementFunction.Airborne;
            state.Behavior = YardBehavior.Dropped;
            state.AirborneVelocity = state.AirborneVelocity.WithWholeY(1);
            var samus = new SamusState { Health = 999, MaxHealth = 999,
                Pose = SamusPoseId.FacingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)) };
            samus.RefreshCollisionRadii(bus);
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(128, 128), TimeIsFrozen = false, Samus = samus, Level = floor });
            AssertEqual(YardBehavior.NormalCrawling, state.Behavior, "dropped Yard lands without becoming aggressive");
            // $A3:D1B3 calls E67A: speed from E5F0 and sign from properties,
            // not the still-stale eight-way direction used before falling.
            ushort speed = ReadNativeYardCrawlSpeed(bus, actor.Parameter1);
            AssertEqual(facing == 0 ? unchecked((ushort)-speed) : speed, state.CrawlingVelocity.X.RawValue,
                $"landed Yard direction {direction}, facing {facing}: native X reset");
            AssertEqual(speed, state.CrawlingVelocity.Y.RawValue, "landed Yard probes downward");
        }
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for the $A3:E5F0 Yard crawl-speed table")]
    private static ushort ReadNativeYardCrawlSpeed(ISnesAddressSpace bus, int parameter) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa3e5f0 + parameter * 2);

    [VerificationCommand("--yard-trajectories")]
    internal static int YardTrajectoriesCommand()
    {
        Suite(nameof(VerifyYardTrajectories), () => VerifyYardTrajectories());
        return 0;
    }
}
