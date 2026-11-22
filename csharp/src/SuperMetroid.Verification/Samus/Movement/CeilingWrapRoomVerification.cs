using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Movement;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Frog Speedway ceiling-shot PLM saturation and native room traces.</summary>
internal static class CeilingWrapRoomVerification
{
    /// <summary>Compares production pose, movement, PLM count, and camera state against a supplied native trace.</summary>
    internal static void VerifyFrogSpeedwayRuntimeTrace(string path, ushort beams = 5, bool dash = true)
    {
        string[] native = File.ReadLines(path).Skip(1).ToArray();
        int expectedFrames = beams == 11 ? (dash ? 349 : 412) : 900;
        AssertEqual(expectedFrames, native.Length, "Native Speedway setup trace length");
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.FrogSpeedway);
        var samus = runtime.Player.Samus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
        samus.Position = samus.Position.WithWhole(new(1237, 139));
        samus.EquippedBeams = SamusEquipmentWordCodec.FromBeamWord(beams); samus.EquippedItems = 0;
        // The room loaded the entering beam's graphics; the trace starts with these beams, so load theirs as a beam change does.
        SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.ShowBeamShade(runtime.Player.Colours, samus.EquippedBeams);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(1109, 0));
        int mismatches = 0;
        for (int frame = 0; frame < native.Length; frame++)
        {
            runtime.StepFrame((ControllerButton.X | ControllerButton.Left | ControllerButton.R | (dash ? ControllerButton.B : 0)));
            string actual = $"{frame},{samus.Position.X.Whole},{samus.Kinematics.Position.X.Fraction},{samus.Position.Y.Whole},{(int)SamusPoseNumbers.Number(samus.Pose):X2},{runtime.RoomObjects.Places.ActiveCount},{runtime.Room.LoadedCamera.PixelPosition.X}";
            if (actual != native[frame] && mismatches++ < 8)
                Console.WriteLine($"Speedway mismatch:\n{actual}\n{native[frame]}");
        }
        AssertEqual(0, mismatches, "Original CPU Speedway input, pose, movement, PLM pressure and camera");
        AssertEqual((ushort)(beams == 11 ? 799 : 842), samus.Position.X.Whole, "Native Speedway success/failure endpoint");
        Console.WriteLine($"PASS {native.Length} native Speedway frames: beams={beams} dash={dash} endpoint={samus.Position.X.Whole}.");
    }

    /// <summary>Checks that a full forty-slot PLM pool permits native speed-block entry without breaking the block.</summary>
    internal static void VerifyFrogSpeedwayPoolCollision()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.FrogSpeedway);
        var level = runtime.Room.LoadedLevelData;
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(1237, 139)),
            PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose, EquippedBeams = SamusBeamFlags.Wave | SamusBeamFlags.Spazer };
        samus.Kinematics.Radius = new(5, 21);
        var roomObjects = runtime.RoomObjects;
        var shots = CreateProjectileFixture();
        var bombs = CreateBombFixture();
        RoomTerrainBlock blocker = level.GetCollisionBlock(new(76, 9)).Terrain;
        var before = SamusBlockCollision.MoveHorizontal(SamusTerrain.InRoom(level, roomObjects), samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = SignedSixteenSixteen.FromWholePixels(-1) });
        AssertTrue(before.Collided, "Ordinary speedless contact blocks before ceiling overload");
        int firstFull = -1;
        for (int frame = 0; frame < 344; frame++)
        {
            GameplayAction held = GameplayAction.Shoot;
            GameplayAction pressed = frame == 0 ? held : GameplayAction.None;
            bombs.StepFrame(SamusTerrain.InRoom(level, SamusRoomObjectFixtures.Uninstalled), samus, held, pressed);
            shots.StepFrame(SamusTerrain.InRoom(level, roomObjects), samus, new ProjectileButtonInput { ControllerInput = held, ControllerNewInput = pressed, ControllerPreviousNewInput = GameplayAction.None }, new(1109, 0), bombs, new SharedGameState(new SharedRandom()));
            roomObjects.Step(level);
            if (firstFull < 0 && roomObjects.Places.ActiveCount == 40) firstFull = frame;
        }
        AssertEqual(343, firstFull, "Authored room reaches full pool with live PLM timers");
        var after = SamusBlockCollision.MoveHorizontal(SamusTerrain.InRoom(level, roomObjects), samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = SignedSixteenSixteen.FromWholePixels(-1) });
        AssertTrue(!after.Collided, "Native full-pool collision remains carry clear without Speed Booster");
        AssertEqual((ushort)1236, samus.Position.X.Whole, "Samus enters first speed block without a clip or teleport");
        AssertEqual(blocker, level.GetCollisionBlock(new(76, 9)).Terrain, "Exhaustion bypass does not break the block");
        AssertEqual(40, roomObjects.Places.ActiveCount, "Contact cannot allocate a forty-first slot");
        Console.WriteLine("PASS Frog Speedway: live ceiling-shot overload permits unchanged speed-block entry at frame 343.");
    }

    /// <summary>Runs the focused full-pool collision check.</summary>
    [VerificationCommand("--ceiling-wrap-room")]
    internal static int CeilingWrapRoomCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayPoolCollision), () => VerifyFrogSpeedwayPoolCollision());
        return 0;
    }

    /// <summary>Compares the original Frog Speedway runtime trace.</summary>
    [VerificationCommand("--ceiling-wrap-runtime")]
    internal static int CeilingWrapRuntimeCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-runtime-410.csv"));
        return 0;
    }

    /// <summary>Checks successful Speedway traces with and without dash held.</summary>
    [VerificationCommand("--ceiling-wrap-success")]
    internal static int CeilingWrapSuccessCommand()
    {
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-run-410.csv", 11, true));
        Suite(nameof(VerifyFrogSpeedwayRuntimeTrace), () => VerifyFrogSpeedwayRuntimeTrace("csharp/test-fixtures/movement-release/frog-success-walk-410.csv", 11, false));
        return 0;
    }
}
