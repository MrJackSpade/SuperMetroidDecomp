using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents spring ball falling fallback verification.</summary>
internal static class SpringBallFallingFallbackVerification
{
    // #1269: Spring Ball falling (type $13) has no transition for Jump alone, so the lookup
    // fails and $91:8304 selects command six. After movement $91:EC85 clears the acceleration
    // mode and base speed and falls through to $91:EC8E's extra-speed cancel. In the 100%
    // movie the next frame's turn therefore starts from rest instead of carrying 3.A000.
    /// <summary>verify spring ball falling fallback.</summary>
    internal static void VerifySpringBallFallingFallback()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var level = runtime.Room.LevelData!;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.Size.X + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0, RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpringBall);
        samus.Pose = SamusPoseId.SpringBallFallingRightPose;
        samus.Position = samus.Position.WithWhole(new(512, 400));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        // The movie's state entering update 349335: base 1.4000, extra 1.E000, mode two.
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.BaseSubspeed = 0x4000;
        samus.HorizontalSpeed.ExtraRunSpeed = 1;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0xe000;
        samus.HorizontalSpeed.AccelerationMode = 2;
        ushort x0 = samus.Position.X.Whole;

        runtime.StepFrame((ushort)SnesButton.A);
        AssertEqual(SamusPoseId.SpringBallFallingRightPose, samus.Pose, "Jump alone keeps the falling Spring Ball pose");
        AssertEqual((ushort)(x0 + 2), samus.Position.X.Whole, "the frame's movement still uses the carried speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, "command six clears base speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSubspeed, "command six clears base subspeed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.ExtraRunSpeed, "command eight clears extra run speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.ExtraRunSubspeed, "command eight clears extra run subspeed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.AccelerationMode, "command six clears the acceleration mode");
        Console.WriteLine("Spring Ball falling fallback: command six clears horizontal speed after movement.");
    }

    /// <summary>spring ball falling fallback command.</summary>
    [VerificationCommand("--spring-ball-falling-fallback")]
    internal static int SpringBallFallingFallbackCommand()
    {
        VerifySpringBallFallingFallback();
        return 0;
    }
}
