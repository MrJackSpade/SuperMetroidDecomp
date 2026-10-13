using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks how an input-free wall jump exits to spin art with and without Space Jump.</summary>
internal static class WallJumpSpinExitVerification
{
    /// <summary>
    /// Confirms command six clears X speed for a plain spin exit, while Space Jump's
    /// pose promotion skips the command and carries the wall-jump launch speed.
    /// </summary>
    internal static void VerifyWallJumpSpinExit()
    {
        Confirm(SamusEquipmentFlags.SpaceJump, speedKept: true);
        Confirm(0, speedKept: false);
        Console.WriteLine("Wall-jump spin exit: command six runs unless pose initialization promotes the spin.");

        static void Confirm(SamusEquipmentFlags items, bool speedKept)
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
            samus.EquippedItems = (ushort)items;
            samus.Pose = SamusPoseId.WallJumpRightPose;
            samus.Position = samus.Position.WithWhole(new(512, 400));
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = 3;
            samus.HorizontalSpeed.BaseSpeed = 1;
            samus.HorizontalSpeed.BaseSubspeed = 0x6000;
            samus.HorizontalSpeed.AccelerationMode = 2;

            runtime.StepFrame(0);
            string label = speedKept ? "Space Jump promotion" : "plain spin jump";
            AssertTrue(speedKept
                    ? samus.Pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose
                    : samus.Pose is SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose,
                $"{label}: input-free wall jump falls back to the expected spin art");
            AssertEqual(speedKept ? (ushort)1 : (ushort)0, samus.HorizontalSpeed.BaseSpeed, $"{label}: base speed");
            AssertEqual(speedKept ? (ushort)0x6000 : (ushort)0, samus.HorizontalSpeed.BaseSubspeed, $"{label}: base subspeed");
            AssertEqual(speedKept ? (ushort)2 : (ushort)0, samus.HorizontalSpeed.AccelerationMode, $"{label}: acceleration mode");
        }
    }

    /// <summary>Runs the focused wall-jump spin-exit check.</summary>
    [VerificationCommand("--wall-jump-spin-exit")]
    internal static int WallJumpSpinExitCommand()
    {
        VerifyWallJumpSpinExit();
        return 0;
    }
}
