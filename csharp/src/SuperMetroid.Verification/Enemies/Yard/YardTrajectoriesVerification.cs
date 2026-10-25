using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardAirborneVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardKickVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Yard;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks Yard landing, kick, airborne motion, and room trajectory behavior.</summary>
internal static class YardTrajectoriesVerification
{
    /// <summary>Runs focused Yard movement checks and records each of five retail actors' largest frame step.</summary>
    internal static void VerifyYardTrajectories()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0xd5a7);
        var assets = LoadFixtureRoomAssets(room);
        Suite(nameof(VerifyYardLanding), () => VerifyYardLanding(bus, room));
        Suite(nameof(VerifyYardRuntimeDistancePublication), () => VerifyYardRuntimeDistancePublication());
        Suite(nameof(VerifyYardKickWords), () => VerifyYardKickWords(bus, room));
        Suite(nameof(VerifyYardAirborneTrajectories), () => VerifyYardAirborneTrajectories(room));
        for (int focus = 0; focus < 5; focus++)
        {
            var random = new SharedGameState();
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(random.NextRandom) { SeedRandom = random.SetRandomNumber }) { TileArtwork = FixtureEnemyTileArtwork() };
            enemies.Load(new RoomEnemyLoadRequest(room.State.EnemyPopulation, room.State.EnemyGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new GameplayColours(new SpriteColourSets())));
            var actor = enemies.SlotTable.Slots[focus];
            var state = enemies.YardStates[focus];
            var samus = new SamusState { Health = 999, MaxHealth = 999,
                Pose = SamusPoseId.FacingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(actor.Position.X.Whole - 64)), actor.Position.Y.Whole)) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            int largest = 0;
            for (int frame = 0; frame < 1200; frame++)
            {
                ushort x = actor.Position.X.Whole, y = actor.Position.Y.Whole;
                var function = (state ?? throw new InvalidOperationException("Verification requires state.")).MovementFunction;
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

    /// <summary>Checks that a dropped Yard resumes ordinary crawling using facing and native speed, regardless of stale direction.</summary>
    private static void VerifyYardLanding(SuperMetroidAddressSpace bus, SelectedRoom room)
    {
        var words = new ushort[32 * 32];
        for (int x = 0; x < 32; x++) words[16 * 32 + x] = 0x8000;
        var floor = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);
        foreach (ushort direction in Enumerable.Range(0, 8).Select(x => (ushort)x))
        foreach (YardAirborneFacing facing in Enum.GetValues<YardAirborneFacing>())
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(() => 0)) { TileArtwork = FixtureEnemyTileArtwork() };
            enemies.Load(new RoomEnemyLoadRequest(room.State.EnemyPopulation, room.State.EnemyGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new GameplayColours(new SpriteColourSets())));
            var actor = enemies.SlotTable.Slots[0];
            var state = enemies.YardStates[0];
            actor.Position = FixedPosition<RoomPixels>.FromWhole(new(256, unchecked((ushort)(256 - actor.Radius.Y))));
            (state ?? throw new InvalidOperationException("Verification requires state.")).Direction = direction;
            state.AirborneFacingDirection = facing;
            state.MovementFunction = YardMovementFunction.Airborne;
            state.Behavior = YardBehavior.Dropped;
            state.AirborneVelocity = state.AirborneVelocity.WithWholeY(1);
            var samus = new SamusState { Health = 999, MaxHealth = 999,
                Pose = SamusPoseId.FacingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(128, 128), TimeIsFrozen = false, Samus = samus, Level = floor });
            AssertEqual(YardBehavior.NormalCrawling, state.Behavior, "dropped Yard lands without becoming aggressive");
            // $A3:D1B3 calls E67A: speed from E5F0 and sign from properties,
            // not the still-stale eight-way direction used before falling.
            ushort speed = ReadNativeYardCrawlSpeed(bus, state.CrawlingSpeedIndex);
            AssertEqual(facing == YardAirborneFacing.Left ? unchecked((ushort)-speed) : speed, state.CrawlingVelocity.X.RawValue,
                $"landed Yard direction {direction}, facing {facing}: native X reset");
            AssertEqual(speed, state.CrawlingVelocity.Y.RawValue, "landed Yard probes downward");
        }
    }

    /// <summary>Reads the parameter-selected crawl speed from the retail bank-$A3 table.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the $A3:E5F0 Yard crawl-speed table")]
    private static ushort ReadNativeYardCrawlSpeed(ISnesAddressSpace bus, int parameter) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa3e5f0 + parameter * 2);

    /// <summary>Runs the Yard trajectory verification suite.</summary>
    [VerificationCommand("--yard-trajectories")]
    internal static int YardTrajectoriesCommand()
    {
        Suite(nameof(VerifyYardTrajectories), () => VerifyYardTrajectories());
        return 0;
    }
}
