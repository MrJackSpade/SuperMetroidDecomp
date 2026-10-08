using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: Spring Ball falling (type $13) has no transition for Jump alone, so the lookup
    // fails and $91:8304 selects command six. After movement $91:EC85 clears the acceleration
    // mode and base speed and falls through to $91:EC8E's extra-speed cancel. In the 100%
    // movie the next frame's turn therefore starts from rest instead of carrying 3.A000.
    private static void VerifySpringBallFallingFallback()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var level = runtime.LevelData!;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.WidthInBlocks + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0, RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpringBall);
        samus.Pose = SamusPoseIds.SpringBallFallingRightPose;
        samus.XPosition = 512;
        samus.YPosition = 400;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.Kinematics.YDirection = 2;
        // The movie's state entering update 349335: base 1.4000, extra 1.E000, mode two.
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.BaseSubspeed = 0x4000;
        samus.HorizontalSpeed.ExtraRunSpeed = 1;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0xe000;
        samus.HorizontalSpeed.AccelerationMode = 2;
        ushort x0 = samus.XPosition;

        runtime.StepFrame((ushort)SnesButton.A);
        AssertEqual(SamusPoseIds.SpringBallFallingRightPose, samus.Pose, "Jump alone keeps the falling Spring Ball pose");
        AssertEqual((ushort)(x0 + 2), samus.XPosition, "the frame's movement still uses the carried speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, "command six clears base speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSubspeed, "command six clears base subspeed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.ExtraRunSpeed, "command eight clears extra run speed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.ExtraRunSubspeed, "command eight clears extra run subspeed");
        AssertEqual((ushort)0, samus.HorizontalSpeed.AccelerationMode, "command six clears the acceleration mode");
        Console.WriteLine("Spring Ball falling fallback: command six clears horizontal speed after movement.");
    }
}
