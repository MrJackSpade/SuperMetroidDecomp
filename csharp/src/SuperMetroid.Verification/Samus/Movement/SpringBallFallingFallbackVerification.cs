using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the reported Spring Ball falling input fallback and horizontal speed reset.</summary>
internal static class SpringBallFallingFallbackVerification
{
    /// <summary>
    /// Confirms Jump alone has no type-$13 transition: command six preserves the current
    /// frame's movement, then clears acceleration and base speed before the extra-speed
    /// cancel, so the recorded next-frame turn begins from rest.
    /// </summary>
    internal static void VerifySpringBallFallingFallback()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var level = runtime.Room.LoadedLevelData;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.Size.X + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0, RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpringBall);
        samus.Pose = SamusPoseId.SpringBallFallingRightPose;
        samus.Position = samus.Position.WithWhole(new(512, 400));
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation(bus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        // The movie's state entering update 349335: base 1.4000, extra 1.E000, mode two.
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.BaseSubspeed = 0x4000;
        samus.HorizontalSpeed.ExtraRunSpeed = 1;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0xe000;
        samus.HorizontalSpeed.AccelerationMode = 2;
        ushort x0 = samus.Position.X.Whole;

        runtime.StepFrame(SnesButton.A);
        AssertEqual(SamusPoseId.SpringBallFallingRightPose, samus.Pose, "Jump alone keeps the falling Spring Ball pose");
        AssertEqual((ushort)(x0 + 2), samus.Position.X.Whole, "the frame's movement still uses the carried speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, "command six clears base speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSubspeed, "command six clears base subspeed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.ExtraRunSpeed, "command eight clears extra run speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.ExtraRunSubspeed, "command eight clears extra run subspeed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.AccelerationMode, "command six clears the acceleration mode");
        Console.WriteLine("Spring Ball falling fallback: command six clears horizontal speed after movement.");
    }

    /// <summary>Runs the focused Spring Ball falling fallback check.</summary>
    [VerificationCommand("--spring-ball-falling-fallback")]
    internal static int SpringBallFallingFallbackCommand()
    {
        VerifySpringBallFallingFallback();
        return 0;
    }
}
