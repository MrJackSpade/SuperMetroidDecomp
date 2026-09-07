using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGrappleSpinInput()
    {
        foreach (bool left in new[] { false, true })
        foreach (bool held in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(0x93fe);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.XPosition = 512;
            samus.YPosition = 256;
            samus.Pose = left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SelectedHudItem = 4;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.GrappleBeam;
            samus.Kinematics.YDirection = 2;
            runtime.StepFrame((ushort)SnesButton.X);
            Console.WriteLine($"Spin fire first frame: pose={samus.Pose:X2}, grapple={samus.Grapple.Phase}");
            AssertEqual(left ? SamusPoseIds.NormalJumpGunExtendedLeftPose : SamusPoseIds.NormalJumpGunExtendedRightPose,
                samus.Pose, "Fire exits the spin through the ROM pose table");
            AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "spin HUD handler does not fire before pose transition");
            runtime.StepFrame(held ? (ushort)SnesButton.X : (ushort)0);
            AssertEqual(GrapplePhase.Firing, samus.Grapple.Phase, "preserved Fire edge starts grapple on the following frame");
            runtime.StepFrame(held ? (ushort)SnesButton.X : (ushort)0);
            AssertEqual(held ? GrapplePhase.Firing : GrapplePhase.CancelPending, samus.Grapple.Phase,
                "held grapple extends; one-frame tap cancels on the next firing pass");
        }
    }
}
