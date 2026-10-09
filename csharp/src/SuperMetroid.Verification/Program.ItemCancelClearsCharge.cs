using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// An item-cancel edge runs the Nothing item's switched-to handler ($90:C545) even when
    /// nothing is selected, clearing the charge. The beam handler then counts held Shoot from
    /// zero. In the 13% movie Samus presses Y mid spin jump; keeping the charge there let a
    /// later turn force-release a beam native never fired.
    /// </summary>
    private static void VerifyItemCancelClearsCharge()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = 512;
        samus.YPosition = 490;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(400, 350);
        for (int frame = 0; frame < 64; frame++)
            runtime.StepFrame(0);
        AssertEqual((ushort)0, samus.SelectedHudItem, "nothing is selected");

        const ushort shoot = (ushort)SnesButton.X;
        for (int frame = 0; frame < 10; frame++)
            runtime.StepFrame(shoot);
        AssertEqual((ushort)10, runtime.Projectiles!.FlareCounter, "ten held frames charge to ten");

        runtime.StepFrame(unchecked((ushort)(shoot | (ushort)SnesButton.Y)));
        AssertEqual((ushort)0, samus.SelectedHudItem, "the cancel leaves nothing selected");
        AssertEqual((ushort)1, runtime.Projectiles.FlareCounter,
            "the cancel clears the charge before held Shoot counts it again");
        Console.WriteLine("  Item cancel: a Y press with nothing selected still clears the charge.");
    }
}
