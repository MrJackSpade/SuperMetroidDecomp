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

/// <summary>Represents jump no xmovement verification.</summary>
internal static class JumpNoXMovementVerification
{
    // #1269: with no direction held and acceleration mode zero, Samus_Jumping_Movement clears
    // base speed and branches past MoveSamus_Horizontally ($90:902B), so no post-move slope
    // alignment runs. Calling the mover with zero aligned Samus to a slope under her: in the
    // 100% movie's Red Fish room she dropped two pixels the frame after a ceiling hit.
    /// <summary>verify jump no xmovement.</summary>
    internal static void VerifyJumpNoXMovement()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.RedFish);
        var samus = runtime.Player.Samus!;
        // Native state after update 366219's ceiling hit.
        samus.Pose = SamusPoseId.NormalJumpAimDownLeftPose;
        samus.Position = samus.Position.WithWhole(new(0x0225, 0x004a));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YSubacceleration = 0x1c00;
        samus.HorizontalSpeed.AccelerationMode = 0;

        SamusAerialMovement.StepNormalJump(bus, runtime.Room.LevelData!, samus,
            (ushort)(SnesButton.A | SnesButton.B), nmiFrameCounter: 0);
        AssertEqual((ushort)0x004a, samus.Position.Y.Whole, "no horizontal move means no slope alignment");
        AssertEqual((ushort)0x0225, samus.Position.X.Whole, "X is unchanged");
        Console.WriteLine("Jump without X movement: Samus_Jumping_Movement skips the horizontal mover and its slope alignment.");
    }

    /// <summary>jump no xmovement command.</summary>
    [VerificationCommand("--jump-no-x-movement")]
    internal static int JumpNoXMovementCommand()
    {
        VerifyJumpNoXMovement();
        return 0;
    }
}
