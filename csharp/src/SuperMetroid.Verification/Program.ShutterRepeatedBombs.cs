using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void AuditRepeatedShutterBombs()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int cases = 0, worstGap = 0;
        foreach (int slotIndex in new[] { 0, 1 })
        foreach (int offset in new[] { -3, 0, 3 })
        foreach (int interval in new[] { 2, 4, 8, 12, 16, 20, 32, 48, 64 })
        foreach (int unmorphAt in new[] { -1, 45, 75, 105 })
        {
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(ShutterRidingRomData.XrayScopeRoom);
            var samus = runtime.Samus!;
            var platform = runtime.Enemies.Slots[slotIndex];
            samus.InputLocked = false;
            samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.XPosition = (ushort)(platform.XPosition + offset);
            samus.YPosition = (ushort)(platform.YPosition - platform.YRadius - samus.Kinematics.YRadius);
            runtime.StepFrame(0);
            for (int frame = 0; frame < 320; frame++)
            {
                ushort input = frame < 220 && frame % interval == 0 ? runtime.ControllerBindings.Shoot : (ushort)0;
                if (frame == unmorphAt) input |= (ushort)SnesButton.Up;
                runtime.StepFrame(input);
                int gap = platform.YPosition - platform.YRadius - samus.YPosition - samus.Kinematics.YRadius;
                if (Math.Abs(samus.XPosition - platform.XPosition) < platform.XRadius + samus.Kinematics.XRadius &&
                    samus.YPosition < platform.YPosition && gap < worstGap)
                {
                    worstGap = gap;
                    Console.WriteLine($"Repeated bomb overlap: slot={slotIndex} offset={offset} interval={interval} unmorph={unmorphAt} frame={frame} gap={gap} Samus={samus.XPosition},{samus.YPosition}/{samus.Pose:X2} platformY={platform.YPosition} carry={samus.Kinematics.ExtraYFixed}");
                }
            }
            cases++;
        }
        Console.WriteLine($"{cases} repeated-bomb sequences; minimum gap {worstGap}. Investigation only: this is not a regression assertion of resolved embedding.");
    }
}
