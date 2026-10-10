using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: with no direction held and acceleration mode zero, Samus_Jumping_Movement clears
    // base speed and branches past MoveSamus_Horizontally ($90:902B), so no post-move slope
    // alignment runs. Calling the mover with zero aligned Samus to a slope under her: in the
    // 100% movie's Red Fish room she dropped two pixels the frame after a ceiling hit.
    private static void VerifyJumpNoXMovement()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.RedFish);
        var samus = runtime.Samus!;
        // Native state after update 366219's ceiling hit.
        samus.Pose = SamusPoseId.NormalJumpAimDownLeftPose;
        samus.XPosition = 0x0225;
        samus.YPosition = 0x004a;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YSubposition = 0;
        samus.Kinematics.YDirection = 2;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.Kinematics.YSubacceleration = 0x1c00;
        samus.HorizontalSpeed.AccelerationMode = 0;

        SamusAerialMovement.StepNormalJump(bus, runtime.LevelData!, samus,
            (ushort)(SnesButton.A | SnesButton.B), nmiFrameCounter: 0);
        AssertEqual((ushort)0x004a, samus.YPosition, "no horizontal move means no slope alignment");
        AssertEqual((ushort)0x0225, samus.XPosition, "X is unchanged");
        Console.WriteLine("Jump without X movement: Samus_Jumping_Movement skips the horizontal mover and its slope alignment.");
    }
}
