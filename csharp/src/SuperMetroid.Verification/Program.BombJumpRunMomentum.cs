using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

internal static class BombJumpRunMomentumVerification
{
    /// <summary>
    /// A bomb jump's special target pose installs through $91:F433, which runs the normal-jump
    /// initializer $91:F543: retained extra run speed selects deceleration mode two. In the 13%
    /// movie a bomb lifts a dashing Samus, and her run speed then decays rather than grows.
    /// </summary>
    internal static void VerifyBombJumpRunMomentum()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const int width = 8, height = 8;
        var floorBlocks = new ushort[width * height];
        for (int x = 0; x < width; x++)
            floorBlocks[4 * width + x] = 0x8000;
        var floor = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(floorBlocks, new byte[floorBlocks.Length], new ushort[floorBlocks.Length]), new byte[8]);

        var samus = new SamusState { Pose = SamusPoseId.MovingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(48, 43)) };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.HorizontalSpeed.ExtraRunSpeed = 2;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0xc000;
        samus.PublishBombJumpDirection(1);
        AssertTrue(samus.TrySetupPublishedBombJump(bus, floor, timeIsFrozen: false, nmiFrameCounter: 0, controllerNewInput: 0),
            "a running Samus accepts the bomb jump");
        AssertEqual(SamusPoseId.NormalJumpForwardLeftPose, samus.Pose, "the bomb jump installs pose $52");
        AssertEqual(SamusHorizontalAccelerationModes.Decelerating, samus.HorizontalSpeed.AccelerationMode,
            "retained extra run speed selects deceleration");
        Console.WriteLine("  Bomb-jump run momentum: the forward-jump pose decelerates retained dash speed.");
    }

    [VerificationCommand("--bomb-jump-run-momentum")]
    internal static int BombJumpRunMomentumCommand()
    {
        Suite(nameof(VerifyBombJumpRunMomentum), () => VerifyBombJumpRunMomentum());
        return 0;
    }
}
