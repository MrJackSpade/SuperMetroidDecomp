using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Speed Booster traversal across Norfair's half-height floor and support latch.</summary>
internal static class BoostFloorAuditVerification
{
    /// <summary>Replays the retail crossing and isolates the slope-support latch's movement boundaries.</summary>
    internal static void AuditBoostFloor()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.NorfairRoom1E);
        var level = runtime.Room.LoadedLevelData;
        var samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.VariaSuit);
        samus.Position = samus.Position.WithWhole(new(80, 420));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(0);
        int flatFloorFrames = 0;
        bool accelerated = false;
        for (int frame = 0; frame < 240; frame++)
        {
            runtime.StepFrame((SnesButton.Right | runtime.Options.ControllerBindings.Dash));
            accelerated |= samus.HorizontalSpeed.ExtraRunSpeed >= 4;
            if (samus.Position.X.Whole is >= 352 and < 416)
            {
                flatFloorFrames++;
                AssertEqual((ushort)451, samus.Position.Y.Whole, "accelerating Samus stays on the half-height floor, including the former trap at X=403");
            }
        }
        AssertTrue(accelerated && flatFloorFrames > 0, "retail traversal exercises accelerated half-height-floor contact");
        AssertEqual((ushort)475, samus.Position.X.Whole, "held Right/Dash crosses the half-height floor and stops at the real wall at X=480");
        AssertEqual((ushort)443, samus.Position.Y.Whole, "wall contact keeps Samus above the solid floor, not embedded in it");

        // Exercise the native latch boundaries separately from the full runtime sequence.
        var kinematics = samus.Kinematics;
        kinematics.Position = kinematics.Position.WithWholeX(375).WithWholeY(451).WithFractionY(0);
        kinematics.PositionAdjustedBySlope = false;
        var floor = SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level }, kinematics, new VerticalBlockMove { Displacement = 1 << 16, ScanLeftToRight = true, IncludeSolidEnemies = false });
        AssertTrue(floor.Collided && kinematics.PositionAdjustedBySlope, "downward square-floor contact sets native support latch");
        SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = level }, kinematics, new HorizontalBlockMove { Displacement = 7 << 16 });
        AssertTrue(kinematics.PositionAdjustedBySlope, "horizontal alignment preserves prior square-floor support");
        SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level }, kinematics, new VerticalBlockMove { Displacement = 0, ScanLeftToRight = false, IncludeSolidEnemies = false });
        AssertTrue(kinematics.PositionAdjustedBySlope, "zero vertical movement preserves native support latch");
        kinematics.Position = kinematics.Position.WithWholeY(400);
        SamusBlockCollision.MoveVertical(new SamusTerrain { Bus = bus, Level = level }, kinematics, new VerticalBlockMove { Displacement = 1 << 16, ScanLeftToRight = false, IncludeSolidEnemies = false });
        AssertTrue(!kinematics.PositionAdjustedBySlope, "nonzero unobstructed terrain entry clears support latch");
        Console.WriteLine("  Boosted floor: retail half-height traversal, real wall endpoint and native slope-support latch lifetime agree.");
    }

    /// <summary>Runs the focused boosted-floor traversal check.</summary>
    [VerificationCommand("--boost-floor-audit")]
    internal static int BoostFloorAuditCommand()
    {
        AuditBoostFloor();
        return 0;
    }
}
