using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Enemies.Metroids;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks a placed bomb detaching a Metroid and its subsequent escape motion.</summary>
internal static class MetroidBombPlacementVerification
{
    /// <summary>Checks natural bomb detonation, four native jitter updates, and return to homing.</summary>
    internal static void VerifyMetroidBombPlacement()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0xdae1);
        Suite(nameof(VerifyMetroidBombRuntime), () => VerifyMetroidBombRuntime(room));
        var assets = LoadFixtureRoomAssets(room);
        var random = new SharedGameState(new SharedRandom());
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(FixtureEnemyTileArtwork()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(random.NextRandom) { SeedRandom = random.SetRandomNumber });
        enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(room.State.EnemyPopulation, BgArtworkFixtures.For(new SnesVram()), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), RoomEnemyFrameFixtures.StandaloneSamus()));
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 999, MaxHealth = 999, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs) };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        var target = enemies.Places.InUpdateOrder[0];
        target.Position = target.Position.WithWhole(new(samus.Position.X.Whole, (ushort)(samus.Position.Y.Whole - 8)));
        enemies.StepStandaloneFrame(enemies.StandaloneFrame(assets.LevelData) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
        enemies.Collision.ResolveOrdinarySamusContact(samus, RoomEnemyFrameFixtures.StandalonePseudoScrew(), 0, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), target.HeldOccupant(), Position16<RoomPixels>.Zero);
        // An attached Metroid shades Samus, and its detach ends the shade.
        AssertTrue(samus.MetroidAttachmentShade != SamusMetroidAttachmentShade.Inactive, "real touch attaches Metroid to morphed Samus");
        var bombs = CreateBombFixture();
        var shots = CreateProjectileFixture();
        int firstExplosion = -1, detached = -1;
        for (int frame = 0; frame < 120; frame++)
        {
            GameplayAction input = frame == 0 ? GameplayAction.Shoot : 0;
            bombs.StepFrame(new SamusTerrain { Level = assets.LevelData, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, input, input);
            if (firstExplosion < 0 && bombs.Bombs.Any(b => b.IsOccupied && b.HasBurnedOut)) firstExplosion = frame;
            enemies.StepStandaloneFrame(enemies.StandaloneFrame(assets.LevelData) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
            enemies.ResolveBombsOnInteractiveEnemies(bombs, shots, samus);
            if (samus.MetroidAttachmentShade == SamusMetroidAttachmentShade.Inactive && detached < 0) detached = frame;
            if (detached >= 0) break;
        }
        AssertTrue(firstExplosion >= 0, "placed normal bomb reaches its natural explosion");
        AssertTrue(detached >= firstExplosion, "centered placed bomb detaches attached Metroid");
        Console.WriteLine($"Centered normal bomb: explosion frame {firstExplosion}, detached frame {detached}.");
        AssertEqual(4, MetroidBehaviorDefinitions.EscapeDuration, "native bomb-detach escape interval");
        // Isolate the post-hit AI from further bomb/contact hits and check the exact
        // native jitter trajectory rather than treating a temporary detach as immunity.
        // Each escape update moves by the jitter its moves left select, then counts one down.
        for (int frame = 0; frame < 4; frame++)
        {
            int index = (MetroidBehaviorDefinitions.EscapeDuration - frame) & 3;
            ushort expectedX = unchecked((ushort)(target.Position.X.Whole + SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(bus, 0xa3ea3f + index * 2)));
            ushort expectedY = unchecked((ushort)(target.Position.Y.Whole + SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(bus, 0xa3ea3f + (index + 4) * 2)));
            // Move Samus away to prevent touch from immediately changing the returned
            // Homing state. The escape routine itself ignores her new position.
            samus.Position = samus.Position.WithWholeX(32);
            enemies.StepStandaloneFrame(enemies.StandaloneFrame(assets.LevelData) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus });
            AssertEqual(expectedX, target.Position.X.Whole, "native post-bomb escape X");
            AssertEqual(expectedY, target.Position.Y.Whole, "native post-bomb escape Y");
        }
    }

    /// <summary>Compares the retail room's placed-bomb detach trajectory with native frames 61 through 66.</summary>
    private static void VerifyMetroidBombRuntime(SelectedRoom room)
    {
        // Use the dry-floor setup from the full-alpha native capture, not the
        // former submerged fixture or a hand-positioned already-exploding bomb.
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(room.Definition, new Position16<RoomPixels>(0, 0));
        var level = runtime.Room.LoadedLevelData;
        for (int y = 0; y < level.Size.Y; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            int index = level.GetBlockIndex(new(checked((ushort)x), checked((ushort)y)));
            // Each block keeps the picture the room shows there, which the tileset's sheet holds (#1676); only its collision changes.
            level.SetTerrain(index, level.TerrainGrid[index].WithCollision(y >= 10 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air));
            level.SetBehavior(index, RoomBlockBehavior.None);
        }
        runtime.InitializeDebugGroundedSamus(128, 100, 10);
        var samus = runtime.Player.Samus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
        samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.Health = samus.MaxHealth = 999;
        samus.Position = samus.Position.WithWhole(new(128, 153));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, samus.AnimationFrameNumbered(0), 1);
        samus.PoseHistory.Restore(samus.Pose, SamusMovementType.MorphBallGround, SamusMovementType.Standing);
        var target = runtime.Enemies.Places.InUpdateOrder[0];
        foreach (var other in runtime.Enemies.Places.InUpdateOrder.Skip(1)) other.Clear();
        target.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 145));
        runtime.PlayerActions.Latch(0);

        // Exact native rows 61..66, case right/travel2/single. The complete 40-case
        // trace is enforced by DebugRunner; these boundaries run in the core suite.
        MetroidBombPlacementExpected[] expected =
        [
            // Attached, the three escape jitters, the escape's end back at the attachment, and the drain once it holds her again.
            new MetroidBombPlacementExpected(149, 145, 952),
            new MetroidBombPlacementExpected(151, 145, 952),
            new MetroidBombPlacementExpected(151, 147, 952),
            new MetroidBombPlacementExpected(149, 147, 952),
            new MetroidBombPlacementExpected(149, 145, 952),
            new MetroidBombPlacementExpected(149, 145, 951),
        ];
        for (int frame = 0; frame <= 66; frame++)
        {
            ControllerButton input = frame == 1 ? ControllerButton.X : 0;
            if (frame is >= 46 and < 54) input |= ControllerButton.Right;
            runtime.StepFrame(input);
            if (frame < 61) continue;
            AssertEqual(expected[frame - 61],
                new MetroidBombPlacementExpected(target.Position.X.Whole, target.Position.Y.Whole, checked((ushort)samus.Health)),
                $"native bomb/EnemyMain detach trajectory at frame {frame}");
            AssertEqual(FixedCoordinate.Of(0x0095, 0x0000), samus.Kinematics.Position.X, "Samus remains at native release X");
            AssertEqual(FixedCoordinate.Of(0x0099, 0xffff), samus.Kinematics.Position.Y, "Samus stays on the floor during detach");
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
