using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyXrayInput()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(ShutterRidingRomData.XrayScopeRoom);
        var samus = runtime.Samus!;
        var support = runtime.Enemies.Slots[1];
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = support.XPosition;
        samus.YPosition = (ushort)(support.YPosition - support.YRadius - samus.Kinematics.YRadius);
        samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
        samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(0);
        runtime.StepFrame((ushort)SnesButton.Select);
        AssertEqual(SamusXrayRomData.SelectedHudItem, samus.SelectedHudItem, "Select reaches the equipped X-ray through ordinary HUD input");
        runtime.StepFrame(runtime.ControllerBindings.Dash);
        AssertTrue(samus.Xray.IsActive, "held Run activates selected X-ray without diagnostic activation");
        AssertTrue(runtime.TimeIsFrozen, "X-ray activation freezes gameplay");
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.ControllerBindings.Dash);
        AssertEqual(XrayBeamPhase.Full, samus.Xray.BeamPhase, "held Run widens the X-ray beam fully");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.TimeIsFrozen, "releasing Run restores ordinary gameplay");
        Console.WriteLine("  X-ray input: normal Select/Run activation, full beam and release restore gameplay.");
    }
}
