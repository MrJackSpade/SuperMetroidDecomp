using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void AuditBoostFloor()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.NorfairRoom1E);
        var level = runtime.LevelData!;
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.VariaSuit);
        samus.XPosition = 80;
        samus.YPosition = 420;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        int flatFloorFrames = 0;
        bool accelerated = false;
        for (int frame = 0; frame < 240; frame++)
        {
            runtime.StepFrame((ushort)((ushort)SnesButton.Right | runtime.ControllerBindings.Dash));
            accelerated |= samus.HorizontalSpeed.ExtraRunSpeed >= 4;
            if (samus.XPosition is >= 352 and < 416)
            {
                flatFloorFrames++;
                AssertEqual((ushort)451, samus.YPosition, "accelerating Samus stays on the half-height floor, including the former trap at X=403");
            }
        }
        AssertTrue(accelerated && flatFloorFrames > 0, "retail traversal exercises accelerated half-height-floor contact");
        AssertEqual((ushort)475, samus.XPosition, "held Right/Dash crosses the half-height floor and stops at the real wall at X=480");
        AssertEqual((ushort)443, samus.YPosition, "wall contact keeps Samus above the solid floor, not embedded in it");

        // Exercise the native latch boundaries separately from the full runtime sequence.
        var kinematics = samus.Kinematics;
        kinematics.XPosition = 375;
        kinematics.YPosition = 451;
        kinematics.YSubposition = 0;
        kinematics.PositionAdjustedBySlope = false;
        var floor = SamusBlockCollision.MoveVertical(bus, level, kinematics, 1 << 16,
            scanLeftToRight: true, includeSolidEnemies: false);
        AssertTrue(floor.Collided && kinematics.PositionAdjustedBySlope, "downward square-floor contact sets native support latch");
        SamusBlockCollision.MoveHorizontal(bus, level, kinematics, 7 << 16);
        AssertTrue(kinematics.PositionAdjustedBySlope, "horizontal alignment preserves prior square-floor support");
        SamusBlockCollision.MoveVertical(bus, level, kinematics, 0, scanLeftToRight: false, includeSolidEnemies: false);
        AssertTrue(kinematics.PositionAdjustedBySlope, "zero vertical movement preserves native support latch");
        kinematics.YPosition = 400;
        SamusBlockCollision.MoveVertical(bus, level, kinematics, 1 << 16, scanLeftToRight: false, includeSolidEnemies: false);
        AssertTrue(!kinematics.PositionAdjustedBySlope, "nonzero unobstructed terrain entry clears support latch");
        Console.WriteLine("  Boosted floor: retail half-height traversal, real wall endpoint and native slope-support latch lifetime agree.");
    }
}
