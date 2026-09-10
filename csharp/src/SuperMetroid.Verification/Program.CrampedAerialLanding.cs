using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Reproduces issue #259's exact collision geometry and captured pre-landing Samus state.
    /// The fixture uses the retail room's two shootable barrier cells but omits unrelated art,
    /// enemies, and PLMs so the assertion stays focused on the production pose-change path.
    /// </summary>
    private static void VerifyCrampedAerialLandingPoseCollision()
    {
        var bus = new TestAddressSpace();

        // These are the three retail pose records involved in the failure. Only direction,
        // shot direction, and Y radius are consumed here, but complete eight-byte definitions
        // make their cartridge layout explicit and keep RefreshCollisionRadii on its real path.
        WritePoseDefinition(
            bus,
            SamusPoseIds.SpinJumpRightPose,
            [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
        WritePoseDefinition(
            bus,
            SamusPoseIds.SpinLandingRightPose,
            [0x08, 0x00, 0xff, 0x02, 0x00, 0x00, 0x15, 0x00]);
        WritePoseDefinition(
            bus,
            SamusPoseIds.CrouchingRightPose,
            [0x08, 0x05, 0xff, 0x02, 0x00, 0x00, 0x10, 0x00]);
        WriteTestWord(
            bus,
            SamusMovementRomData.Poses.AnimationDelayListPointers +
                SamusPoseIds.CrouchingRightPose * sizeof(ushort),
            0xc100);
        bus.WriteBytes(0x91c100, [0x01]);

        const int roomWidth = 16;
        const int roomHeight = 16;
        var foreground = new ushort[roomWidth * roomHeight];

        // Room $02/$0B has shootable solid cells at (8,2) and (8,4). At captured X=$0094,
        // Samus's horizontal radius overlaps column eight, leaving exactly 32 vertical pixels
        // between the bottom of the ceiling cell and the top of the floor cell.
        foreground[2 * roomWidth + 8] = 0xc000;
        foreground[4 * roomWidth + 8] = 0xc000;
        RoomLevelData level = CreateRoom(
            roomWidth,
            roomHeight,
            foreground,
            new byte[foreground.Length]);
        var samus = new SamusState
        {
            Pose = SamusPoseIds.SpinJumpRightPose,
            XPosition = 0x0094,
            YPosition = 0x0034,
        };
        samus.RefreshCollisionRadii(bus);
        samus.HorizontalSpeed.AccelerationMode = 2;
        samus.HorizontalSpeed.BaseSpeed = 3;
        samus.Kinematics.YDirection = 2;
        samus.Kinematics.YSpeed = 4;

        bool installedLanding = samus.TryApplyAerialLanding(
            bus,
            level,
            wasSpinning: true,
            controllerInput: (ushort)(SnesButton.Right | SnesButton.A),
            nmiFrameCounter: 39440);

        AssertTrue(!installedLanding,
            "cramped spin landing rejects the 42-pixel landing body");
        AssertEqual(SamusPoseIds.CrouchingRightPose, samus.Pose,
            "two-sided non-Morph landing collision selects native crouch fallback");
        AssertEqual(0x0030, samus.YPosition,
            "crouch fallback preserves the captured floor boundary");
        AssertEqual(16, samus.Kinematics.YRadius,
            "crouch fallback fits the 32-pixel retail barrier opening");
        AssertEqual(0x0020, samus.YPosition - samus.Kinematics.YRadius,
            "crouch top meets the barrier ceiling without overlap");
        AssertEqual(0x0040, samus.YPosition + samus.Kinematics.YRadius,
            "crouch bottom remains on the barrier floor");
        // Original F404 returns changed-pose carry for this substitution. The caller
        // skips command five; #461's original-CPU trajectory proves this retained state.
        AssertEqual(2, samus.HorizontalSpeed.AccelerationMode,
            "rejected full landing skips collision command five");
        AssertEqual(3, samus.HorizontalSpeed.BaseSpeed,
            "cramped landing preserves horizontal speed");
        AssertEqual(4, samus.Kinematics.YSpeed,
            "cramped landing preserves vertical speed");
        AssertEqual(2, samus.Kinematics.YDirection,
            "cramped landing does not publish grounded direction");

        WritePoseDefinition(bus, SamusPoseIds.ScrewAttackRightPose,
            [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
        samus.Pose = SamusPoseIds.ScrewAttackRightPose;
        samus.YPosition = 0x0034;
        samus.RefreshCollisionRadii(bus);
        samus.TryApplyAerialLanding(bus, level, true, 0, 39440);
        AssertTrue(samus.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
            "rejected landing still performs pre-command suit palette restoration");
    }
}
