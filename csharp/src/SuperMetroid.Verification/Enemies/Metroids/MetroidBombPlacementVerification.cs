using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Metroids;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks a placed bomb detaching a Metroid and its subsequent escape motion.</summary>
internal static class MetroidBombPlacementVerification
{
    /// <summary>Checks natural bomb detonation, four native jitter updates, and return to homing.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Metroid post-bomb escape jitter table")]
    internal static void VerifyMetroidBombPlacement()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0xdae1);
        Suite(nameof(VerifyMetroidBombRuntime), () => VerifyMetroidBombRuntime(room));
        var assets = LoadFixtureRoomAssets(room);
        var random = new SharedGameState();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(random.NextRandom) { SeedRandom = random.SetRandomNumber }) { TileArtwork = FixtureEnemyTileArtwork() };
        enemies.Load(new RoomEnemyLoadRequest(room.State.EnemyPopulation, room.State.EnemyGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new GameplayColours(new SpriteColourSets())));
        var samus = new SamusState { Health = 999, MaxHealth = 999, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs) };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        var target = enemies.SlotTable.Slots[0];
        target.Position = target.Position.WithWhole(new(samus.Position.X.Whole, (ushort)(samus.Position.Y.Whole - 8)));
        enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = assets.LevelData });
        enemies.Collision.ResolveOrdinarySamusContact(samus, 0);
        var state = enemies.MetroidStates[0];
        AssertEqual(MetroidAiFunction.AttachedToSamus, (state ?? throw new InvalidOperationException("Verification requires state.")).Function, "real touch attaches Metroid to morphed Samus");
        var bombs = CreateBombFixture();
        var shots = CreateProjectileFixture();
        int firstExplosion = -1, detached = -1;
        for (int frame = 0; frame < 120; frame++)
        {
            ControllerButton input = frame == 0 ? ControllerButton.X : 0;
            bombs.StepFrame(new SamusTerrain { Level = assets.LevelData }, samus, input, input);
            if (firstExplosion < 0 && bombs.Slots.Any(b => b.Type != 0 && b.BombTimer.IsZero)) firstExplosion = frame;
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = assets.LevelData });
            enemies.Collision.ResolveOrdinaryBombHits(bombs, shots, samus);
            if (state.Function == MetroidAiFunction.PowerBombEscape && detached < 0) detached = frame;
            if (detached >= 0) break;
        }
        AssertTrue(firstExplosion >= 0, "placed normal bomb reaches its natural explosion");
        AssertTrue(detached >= firstExplosion, "centered placed bomb detaches attached Metroid");
        Console.WriteLine($"Centered normal bomb: explosion frame {firstExplosion}, detached frame {detached}, escape timer {state.EscapeTimer.RawValue}.");
        AssertEqual((ushort)4, state.EscapeTimer.RawValue, "native bomb-detach escape interval");
        // Isolate the post-hit AI from further bomb/contact hits and check the exact
        // native jitter trajectory rather than treating a temporary detach as immunity.
        for (int frame = 0; frame < 4; frame++)
        {
            int index = state.EscapeTimer.RawValue & 3;
            ushort expectedX = unchecked((ushort)(target.Position.X.Whole + SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa3ea3f + index * 2)));
            ushort expectedY = unchecked((ushort)(target.Position.Y.Whole + SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xa3ea3f + (index + 4) * 2)));
            // Move Samus away to prevent touch from immediately changing the returned
            // Homing state. The escape routine itself ignores her new position.
            samus.Position = samus.Position.WithWholeX(32);
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = assets.LevelData });
            AssertEqual(expectedX, target.Position.X.Whole, "native post-bomb escape X");
            AssertEqual(expectedY, target.Position.Y.Whole, "native post-bomb escape Y");
            AssertEqual(frame == 3 ? MetroidAiFunction.Homing : MetroidAiFunction.PowerBombEscape,
                state.Function, "native escape phase ends after four updates");
        }
    }

    /// <summary>Compares the retail room's placed-bomb detach trajectory with native frames 61 through 66.</summary>
    private static void VerifyMetroidBombRuntime(CartridgeRoomHeader room)
    {
        // Use the dry-floor setup from the full-alpha native capture, not the
        // former submerged fixture or a hand-positioned already-exploding bomb.
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room.Definition, new Position16<RoomPixels>(0, 0));
        var level = runtime.Room.LoadedLevelData;
        for (int y = 0; y < level.Size.Y; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            int index = level.GetBlockIndex(new(checked((ushort)x), checked((ushort)y)));
            level.SetForegroundEntry(index, y >= 10 ? (ushort)0x8000 : (ushort)0);
            level.SetBehavior(index, 0);
        }
        runtime.InitializeDebugGroundedSamus(128, 100, 10);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
        samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.Health = samus.MaxHealth = 999;
        samus.Position = samus.Position.WithWhole(new(128, 153));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 0, 1);
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement =
            SnesLittleEndian.Word((byte)SamusFacingDirection.Right, (byte)SamusMovementType.MorphBallGround);
        samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
        samus.PoseHistory.LastDifferentPose = 0;
        var target = runtime.Enemies.SlotTable.Slots[0];
        foreach (var other in runtime.Enemies.SlotTable.Slots.Skip(1)) other.Clear();
        target.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 145));
        runtime.Controller1.Latch(0);
        var state = runtime.Enemies.MetroidStates[0];

        // Exact native rows 61..66, case right/travel2/single. The complete 40-case
        // trace is enforced by DebugRunner; these boundaries run in the core suite.
        MetroidBombPlacementExpected[] expected =
        [
            new MetroidBombPlacementExpected(149, 145, MetroidAiFunction.AttachedToSamus, 0, 952),
            new MetroidBombPlacementExpected(151, 145, MetroidAiFunction.PowerBombEscape, 3, 952),
            new MetroidBombPlacementExpected(151, 147, MetroidAiFunction.PowerBombEscape, 2, 952),
            new MetroidBombPlacementExpected(149, 147, MetroidAiFunction.PowerBombEscape, 1, 952),
            new MetroidBombPlacementExpected(149, 145, MetroidAiFunction.Homing, 0, 952),
            new MetroidBombPlacementExpected(149, 145, MetroidAiFunction.AttachedToSamus, 0, 951),
        ];
        for (int frame = 0; frame <= 66; frame++)
        {
            ControllerButton input = frame == 1 ? ControllerButton.X : 0;
            if (frame is >= 46 and < 54) input |= ControllerButton.Right;
            runtime.StepFrame(input);
            if (frame < 61) continue;
            AssertEqual(expected[frame - 61],
                new MetroidBombPlacementExpected(target.Position.X.Whole, target.Position.Y.Whole, (state ?? throw new InvalidOperationException("Verification requires state.")).Function, state.EscapeTimer.RawValue, checked((ushort)samus.Health)),
                $"native bomb/EnemyMain detach trajectory at frame {frame}");
            AssertEqual(0x00950000u, samus.Kinematics.Position.X.RawValue, "Samus remains at native release X");
            AssertEqual(0x0099ffffu, samus.Kinematics.Position.Y.RawValue, "Samus stays on the floor during detach");
        }
    }

    /// <summary>Runs the Metroid bomb placement and escape regression.</summary>
    [VerificationCommand("--metroid-bomb-placement")]
    internal static int MetroidBombPlacementCommand()
    {
        Suite(nameof(VerifyMetroidBombPlacement), () => VerifyMetroidBombPlacement());
        return 0;
    }
}
