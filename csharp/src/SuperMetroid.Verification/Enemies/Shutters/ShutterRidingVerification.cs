using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Morph Ball rides and rolls off both bomb-raised shutters in the retail room.</summary>
internal static class ShutterRidingVerification
{
    /// <summary>Uses real bomb activation to check the upper stop, rider alignment, and roll-off in both slots.</summary>
    internal static void VerifyShutterRiding()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (int slot in new[] { 0, 1 })
        {
            var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointersTooling.BrinstarShutterRoom, camera: new Position16<RoomPixels>(0x100, 0));
            var samus = runtime.Player.IntroducedSamus;
            var platform = runtime.Enemies.SlotTable.Slots[slot];
            var state = runtime.Enemies.VerticalShutterStates[slot];
            samus.Pose = SamusPoseId.MorphBallGroundRightPose;
            samus.InputLocked = false;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Presentation.InitializeAnimation(bus);
            samus.Position = samus.Position.WithWhole(new(platform.Position.X.Whole, (ushort)(platform.Position.Y.Whole - platform.Radius.Y - samus.Kinematics.Radius.Y)));
            runtime.StepFrame(0);
            // A real Shoot edge plants the bomb. Its explosion activates the shutter
            // and launches Samus; no diagnostic phase write substitutes for that path.
            for (int frame = 0; frame < 220; frame++)
                runtime.StepFrame(frame == 0 ? runtime.Options.ControllerBindings.Shoot : (ushort)0);
            AssertEqual(VerticalShutterFunction.PermanentNoOp, (state ?? throw new InvalidOperationException("Verification requires state.")).Function,
                "bomb-raised shutter remains at native permanent upper stop without a second hit");
            AssertEqual(state.MinimumYPosition, platform.Position.Y.Whole, "shutter reaches the authored upper Y");
            AssertEqual(0, platform.Position.Y.Whole - platform.Radius.Y - samus.Position.Y.Whole - samus.Kinematics.Radius.Y,
                "Morph Ball remains on the stopped shutter, not inside it");
            for (int frame = 0; frame < 30; frame++)
                runtime.StepFrame((slot == 0 ? SnesButton.Left : SnesButton.Right));
            AssertTrue(Math.Abs(samus.Position.X.Whole - platform.Position.X.Whole) > platform.Radius.X + samus.Kinematics.Radius.X,
                $"Morph Ball rolls completely clear of shutter {slot} through its upper opening (X={samus.Position.X.Whole})");
            AssertEqual(VerticalShutterFunction.PermanentNoOp, state.Function,
                "rolling off does not spuriously reactivate the shutter");
        }
        Console.WriteLine("  Shootable shutters: real bomb activation, upper stop, rider alignment and roll-off agree in both room $01/$22 slots.");
    }

    /// <summary>Runs the focused shutter rider comparison.</summary>
    [VerificationCommand("--shutter-riding")]
    internal static int ShutterRidingCommand()
    {
        Suite(nameof(VerifyShutterRiding), () => VerifyShutterRiding());
        return 0;
    }
}
