using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Reproduction of the item-cancel edge that resets an unselected
/// charge beam before held Shoot advances its flare counter again.</summary>
internal static class ItemCancelClearsChargeVerification
{
    /// <summary>
    /// An item-cancel edge runs the Nothing item's switched-to handler ($90:C545) even when
    /// nothing is selected, clearing the charge. The beam handler then counts held Shoot from
    /// zero. In the 13% movie Samus presses Y mid spin jump; keeping the charge there let a
    /// later turn force-release a beam native never fired.
    /// </summary>
    internal static void VerifyItemCancelClearsCharge()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(512, 490));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(400, 350));
        for (int frame = 0; frame < 64; frame++)
            runtime.StepFrame(0);
        AssertEqual((ushort)0, samus.SelectedHudItem, "nothing is selected");

        const ushort shoot = (ushort)SnesButton.X;
        for (int frame = 0; frame < 10; frame++)
            runtime.StepFrame(shoot);
        AssertEqual((ushort)10, runtime.Projectiles.Charge.FlareCounter, "ten held frames charge to ten");

        runtime.StepFrame(unchecked((ushort)(shoot | (ushort)SnesButton.Y)));
        AssertEqual((ushort)0, samus.SelectedHudItem, "the cancel leaves nothing selected");
        AssertEqual((ushort)1, runtime.Projectiles.Charge.FlareCounter,
            "the cancel clears the charge before held Shoot counts it again");
        Console.WriteLine("  Item cancel: a Y press with nothing selected still clears the charge.");
    }

    /// <summary>Runs the focused charge-reset reproduction from the verification CLI.</summary>
    [VerificationCommand("--item-cancel-clears-charge")]
    internal static int ItemCancelClearsChargeCommand()
    {
        Suite(nameof(VerifyItemCancelClearsCharge), () => VerifyItemCancelClearsCharge());
        return 0;
    }
}
