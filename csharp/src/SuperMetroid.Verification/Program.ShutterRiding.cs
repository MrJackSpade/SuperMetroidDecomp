using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyShutterRiding()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (int slot in new[] { 0, 1 })
        {
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(ShutterRidingRomData.XrayScopeRoom, cameraX: 0x100, cameraY: 0);
            var samus = runtime.Samus!;
            var platform = runtime.Enemies.Slots[slot];
            var state = runtime.Enemies.VerticalShutterStates[slot]!;
            samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
            samus.InputLocked = false;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.XPosition = platform.XPosition;
            samus.YPosition = (ushort)(platform.YPosition - platform.YRadius - samus.Kinematics.YRadius);
            runtime.StepFrame(0);
            // A real Shoot edge plants the bomb. Its explosion activates the shutter
            // and launches Samus; no diagnostic phase write substitutes for that path.
            for (int frame = 0; frame < 220; frame++)
                runtime.StepFrame(frame == 0 ? runtime.ControllerBindings.Shoot : (ushort)0);
            AssertEqual(VerticalShutterFunction.PermanentNoOp, state.Function,
                "bomb-raised shutter remains at native permanent upper stop without a second hit");
            AssertEqual(state.MinimumYPosition, platform.YPosition, "shutter reaches the authored upper Y");
            AssertEqual(0, platform.YPosition - platform.YRadius - samus.YPosition - samus.Kinematics.YRadius,
                "Morph Ball remains on the stopped shutter, not inside it");
            for (int frame = 0; frame < 30; frame++)
                runtime.StepFrame((ushort)(slot == 0 ? SnesButton.Left : SnesButton.Right));
            AssertTrue(Math.Abs(samus.XPosition - platform.XPosition) > platform.XRadius + samus.Kinematics.XRadius,
                $"Morph Ball rolls completely clear of shutter {slot} through its upper opening (X={samus.XPosition})");
            AssertEqual(VerticalShutterFunction.PermanentNoOp, state.Function,
                "rolling off does not spuriously reactivate the shutter");
        }
        Console.WriteLine("  Shootable shutters: real bomb activation, upper stop, rider alignment and roll-off agree in both room $01/$22 slots.");
    }
}
