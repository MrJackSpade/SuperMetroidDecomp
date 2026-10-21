using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

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
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        var level = runtime.Room.LoadedLevelData;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.Size.X + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0, RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpringBall);
        samus.Pose = SamusPoseId.SpringBallFallingRightPose;
        samus.Position = samus.Position.WithWhole(new(512, 400));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        // The movie's state entering update 349335: base 1.4000, extra 1.E000, mode two.
        samus.HorizontalSpeed.BaseSpeed = new SnesUnsignedSixteenSixteen(1, 0x4000);
        samus.HorizontalSpeed.ExtraRunSpeed = new SnesUnsignedSixteenSixteen(1, 0xe000);
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
        ushort x0 = samus.Position.X.Whole;

        runtime.StepFrame(SnesButton.A);
        AssertEqual(SamusPoseId.SpringBallFallingRightPose, samus.Pose, "Jump alone keeps the falling Spring Ball pose");
        AssertEqual((ushort)(x0 + 2), samus.Position.X.Whole, "the frame's movement still uses the carried speed");
        AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Whole, "command six clears base speed");
        AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Fraction, "command six clears base subspeed");
        AssertEqual(0, samus.HorizontalSpeed.ExtraRunSpeed.Whole, "command eight clears extra run speed");
        AssertEqual(0, samus.HorizontalSpeed.ExtraRunSpeed.Fraction, "command eight clears extra run subspeed");
        AssertEqual(SamusAccelerationMode.Accelerating, samus.HorizontalSpeed.AccelerationMode, "command six clears the acceleration mode");
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
