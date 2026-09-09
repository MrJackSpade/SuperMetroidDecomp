using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyYardTrajectories()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CartridgeRoomHeader.Load(bus, 0xd5a7);
        var assets = CartridgeRoomAssets.Load(bus, room);
        VerifyYardLanding(bus, room);
        VerifyYardRuntimeDistancePublication();
        VerifyYardAirborneTrajectories(bus, room);
        for (int focus = 0; focus < 5; focus++)
        {
            var enemies = new RoomEnemySystem();
            var random = new Bank80SystemState();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                new SnesVram(), new SnesCgram(), random.NextRandom, random.SetRandomNumber);
            var actor = enemies.Slots[focus];
            var state = enemies.YardStates[focus]!;
            var samus = new SamusState { Health = 999, MaxHealth = 999,
                Pose = SamusPoseIds.FacingLeftNormalPose, XPosition = unchecked((ushort)(actor.XPosition - 64)), YPosition = actor.YPosition };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            int largest = 0;
            for (int frame = 0; frame < 1200; frame++)
            {
                ushort x = actor.XPosition, y = actor.YPosition;
                var function = state.MovementFunction;
                ushort cameraX = unchecked((ushort)Math.Max(0, x - 128)), cameraY = unchecked((ushort)Math.Max(0, y - 112));
                enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
                int distance = Math.Max(Math.Abs(unchecked((short)(actor.XPosition - x))), Math.Abs(unchecked((short)(actor.YPosition - y))));
                if (distance > largest)
                {
                    largest = distance;
                    Console.WriteLine($"Yard {focus}, frame {frame}, {function}: ({x},{y})->({actor.XPosition},{actor.YPosition}), crawl={state.CrawlingXVelocity:X4}/{state.CrawlingYVelocity:X4}, airborne={state.AirborneXVelocity:X4}.{state.AirborneXSubvelocity:X4}/{state.AirborneYVelocity:X4}.{state.AirborneYSubvelocity:X4}");
                }
            }
            Console.WriteLine($"Yard {focus}: maximum axis step {largest}");
        }
    }

    private static void VerifyYardLanding(SuperMetroidAddressSpace bus, CartridgeRoomHeader room)
    {
        var words = new ushort[32 * 32];
        for (int x = 0; x < 32; x++) words[16 * 32 + x] = 0x8000;
        var floor = new RoomLevelData(32, 32, words, new byte[words.Length], new ushort[words.Length], new byte[8]);
        foreach (ushort direction in Enumerable.Range(0, 8).Select(x => (ushort)x))
        foreach (ushort facing in new ushort[] { 0, 1 })
        {
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                new SnesVram(), new SnesCgram(), () => 0);
            var actor = enemies.Slots[0];
            var state = enemies.YardStates[0]!;
            actor.XPosition = 256;
            actor.YPosition = unchecked((ushort)(256 - actor.YRadius));
            actor.XSubposition = actor.YSubposition = 0;
            state.Direction = direction;
            state.AirborneFacingDirection = facing;
            state.MovementFunction = YardMovementFunction.Airborne;
            state.Behavior = 3;
            state.AirborneYVelocity = 1;
            var samus = new SamusState { Health = 999, MaxHealth = 999,
                Pose = SamusPoseIds.FacingLeftNormalPose, XPosition = 32, YPosition = 32 };
            samus.RefreshCollisionRadii(bus);
            enemies.StepFrame(128, 128, false, samus, level: floor);
            AssertEqual((ushort)0, state.Behavior, "dropped Yard lands without becoming aggressive");
            // $A3:D1B3 calls E67A: speed from E5F0 and sign from properties,
            // not the still-stale eight-way direction used before falling.
            ushort speed = SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(bus, 0xa3e5f0 + actor.Parameter1 * 2);
            AssertEqual(facing == 0 ? unchecked((ushort)-speed) : speed, state.CrawlingXVelocity,
                $"landed Yard direction {direction}, facing {facing}: native X reset");
            AssertEqual(speed, state.CrawlingYVelocity, "landed Yard probes downward");
        }
    }
}
