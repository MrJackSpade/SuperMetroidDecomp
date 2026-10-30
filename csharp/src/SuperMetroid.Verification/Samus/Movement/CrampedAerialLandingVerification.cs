using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SyntheticSamusAnimationDelaysVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus's crouch fallback when a spin landing cannot fit between retail barrier cells.</summary>
internal static class CrampedAerialLandingVerification
{
    /// <summary>
    /// Reproduces issue #259's exact collision geometry and captured pre-landing Samus state.
    /// The fixture uses the retail room's two shootable barrier cells but omits unrelated art,
    /// enemies, and PLMs so the assertion stays focused on the production pose-change path.
    /// </summary>
    internal static void VerifyCrampedAerialLandingPoseCollision()
    {
        var bus = new TestAddressSpace();

        // These are the three retail pose records involved in the failure. Only direction,
        // shot direction, and Y radius are consumed here, but complete eight-byte definitions
        // make their cartridge layout explicit and keep RefreshCollisionRadii on its real path.
        WritePoseDefinition(
            bus,
            (int)SamusPoseId.SpinJumpRightPose,
            [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
        WritePoseDefinition(
            bus,
            (int)SamusPoseId.SpinLandingRightPose,
            [0x08, 0x00, 0xff, 0x02, 0x00, 0x00, 0x15, 0x00]);
        WritePoseDefinition(
            bus,
            (int)SamusPoseId.CrouchingRightPose,
            [0x08, 0x05, 0xff, 0x02, 0x00, 0x00, 0x10, 0x00]);
        SeedSyntheticAnimationDelay(bus, SamusPoseId.CrouchingRightPose, 0xc100, 0x01);

        const int roomWidth = 16;
        const int roomHeight = 16;
        var foreground = new RoomTerrainBlock[roomWidth * roomHeight];

        // Room $02/$0B has shootable solid cells at (8,2) and (8,4). At captured X=$0094,
        // Samus's horizontal radius overlaps column eight, leaving exactly 32 vertical pixels
        // between the bottom of the ceiling cell and the top of the floor cell.
        foreground[2 * roomWidth + 8] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock);
        foreground[4 * roomWidth + 8] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock);
        RoomLevelData level = CreateRoom(new(roomWidth, roomHeight),
            foreground,
            new byte[foreground.Length]);
        var samus = new SamusState
        {
            Pose = SamusPoseId.SpinJumpRightPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0094, 0x0034)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
        samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(3);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithWhole(4);

        bool installedLanding = SamusPoseTransitions.TryApplyAerialLanding(samus, 

            level,
            wasSpinning: true,
            controllerInput: (ControllerButton.Right | ControllerButton.A),
            frameCounter: 39440);

        AssertTrue(!installedLanding,
            "cramped spin landing rejects the 42-pixel landing body");
        AssertEqual(SamusPoseId.CrouchingRightPose, samus.Pose,
            "two-sided non-Morph landing collision selects native crouch fallback");
        AssertEqual(0x0030, samus.Position.Y.Whole,
            "crouch fallback preserves the captured floor boundary");
        AssertEqual(12, samus.Kinematics.Radius.Y,
            "collision-selected crouch retains the live spin radius until next alpha");
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        AssertEqual(16, samus.Kinematics.Radius.Y,
            "crouch fallback fits the 32-pixel retail barrier opening");
        AssertEqual(0x0020, samus.Position.Y.Whole - samus.Kinematics.Radius.Y,
            "crouch top meets the barrier ceiling without overlap");
        AssertEqual(0x0040, samus.Position.Y.Whole + samus.Kinematics.Radius.Y,
            "crouch bottom remains on the barrier floor");
        // Original F404 returns changed-pose carry for this substitution. The caller
        // skips command five; #461's original-CPU trajectory proves this retained state.
        AssertEqual(SamusAccelerationMode.Decelerating, samus.HorizontalSpeed.AccelerationMode,
            "rejected full landing skips collision command five");
        AssertEqual(3, samus.HorizontalSpeed.BaseSpeed.Whole,
            "cramped landing preserves horizontal speed");
        AssertEqual(4, samus.Kinematics.YSpeed.Whole,
            "cramped landing preserves vertical speed");
        AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.YDirection,
            "cramped landing does not publish grounded direction");

        WritePoseDefinition(bus, (int)SamusPoseId.ScrewAttackRightPose,
            [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
        samus.Pose = SamusPoseId.ScrewAttackRightPose;
        samus.Position = samus.Position.WithWholeY(0x0034);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPoseTransitions.TryApplyAerialLanding(samus, level, true, 0, 39440);
        AssertTrue(samus.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
            "rejected landing still performs pre-command suit palette restoration");
    }
}
