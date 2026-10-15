using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;

/// <summary>Reports observed Samus/platform overlap in the Brinstar shutter room under repeated bombs.</summary>
internal static class ShutterRepeatedBombsVerification
{
    /// <summary>Records the worst vertical gap across specified bomb intervals, offsets, and unmorph times.</summary>
    internal static void AuditRepeatedShutterBombs()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int cases = 0, worstGap = 0;
        foreach (int slotIndex in new[] { 0, 1 })
        foreach (int offset in new[] { -3, 0, 3 })
        foreach (int interval in new[] { 2, 4, 8, 12, 16, 20, 32, 48, 64 })
        foreach (int unmorphAt in new[] { -1, 45, 75, 105 })
        {
            var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointersTooling.BrinstarShutterRoom);
            var samus = runtime.Player.IntroducedSamus;
            var platform = runtime.Enemies.SlotTable.Slots[slotIndex];
            samus.InputLocked = false;
            samus.Pose = SamusPoseId.MorphBallGroundRightPose;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            samus.Position = samus.Position.WithWhole(new((ushort)(platform.Position.X.Whole + offset), (ushort)(platform.Position.Y.Whole - platform.Radius.Y - samus.Kinematics.Radius.Y)));
            runtime.StepFrame(0);
            for (int frame = 0; frame < 320; frame++)
            {
                SnesButton input = frame < 220 && frame % interval == 0 ? runtime.Options.ControllerBindings.Shoot : (ushort)0;
                if (frame == unmorphAt) input |= SnesButton.Up;
                runtime.StepFrame(input);
                int gap = platform.Position.Y.Whole - platform.Radius.Y - samus.Position.Y.Whole - samus.Kinematics.Radius.Y;
                if (Math.Abs(samus.Position.X.Whole - platform.Position.X.Whole) < platform.Radius.X + samus.Kinematics.Radius.X &&
                    samus.Position.Y.Whole < platform.Position.Y.Whole && gap < worstGap)
                {
                    worstGap = gap;
                    Console.WriteLine($"Repeated bomb overlap: slot={slotIndex} offset={offset} interval={interval} unmorph={unmorphAt} frame={frame} gap={gap} Samus={samus.Position.X.Whole},{samus.Position.Y.Whole}/{(int)samus.Pose:X2} platformY={platform.Position.Y.Whole} carry={samus.Kinematics.ExtraDisplacement.Y.RawValue}");
                }
            }
            cases++;
        }
        Console.WriteLine($"{cases} repeated-bomb sequences; minimum gap {worstGap}. Investigation only: this is not a regression assertion of resolved embedding.");
    }

    /// <summary>Runs the shutter overlap diagnostic from the verification command line.</summary>
    [VerificationCommand("--shutter-repeat")]
    internal static int ShutterRepeatCommand()
    {
        AuditRepeatedShutterBombs();
        return 0;
    }
}
