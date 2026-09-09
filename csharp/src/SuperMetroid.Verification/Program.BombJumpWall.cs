using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombJumpWallContact()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var words = new ushort[16 * 16];
        for (int row = 0; row < 10; row++) words[row * 16 + 4] = 0x8000;
        var level = new RoomLevelData(16, 16, words, new byte[words.Length], new ushort[words.Length], new byte[8]);
        var samus = new SamusState { Pose = SamusPoseIds.MorphBallGroundRightPose, XPosition = 59, YPosition = 80 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.Kinematics.XSubposition = 0xF000;
        samus.Kinematics.YSubacceleration = 0x4000;
        samus.RequestMorphedBombJump(3);
        SamusBombJumpMovement.Start(bus, samus);
        var result = SamusBombJumpMovement.Step(bus, level, samus, 0, new RoomPlmSystem());
        AssertTrue(result.Horizontal is { Collided: true }, $"diagonal bomb jump hits the shaft wall (X={samus.Kinematics.XFixed:X8}, radius={samus.Kinematics.XRadius}, move={result.Horizontal})");
        AssertTrue(result.Vertical is { Collided: false }, "shaft wall leaves vertical travel clear");
        // Original $90:E032 bytes: GrapplePoseAudit bomb-wall. A horizontal collision
        // must not end the special handler; the native branch tests the later Y result.
        AssertTrue(!result.Ended && samus.BombJumpActive, "wall-only collision retains native bomb-jump handler");
        AssertEqual(0x0803, samus.BombJumpDirection, "native bomb-wall direction remains armed");
        AssertEqual(59, samus.XPosition, "native bomb-wall X endpoint");
        AssertEqual(77, samus.YPosition, "native bomb-wall Y endpoint");
        AssertEqual(0x4000, samus.Kinematics.YSubposition, "native bomb-wall Y fraction");
        AssertEqual(2, samus.Kinematics.YSpeed, "native bomb-wall next speed");
        AssertEqual(0x8000, samus.Kinematics.YSubspeed, "native bomb-wall next subspeed");
        Console.WriteLine("  Bomb jump: wall-only collision preserves native ascent and handler lifetime.");
        VerifyCarriedMorphCeiling(bus);
    }

    private static void VerifyCarriedMorphCeiling(SuperMetroidAddressSpace bus)
    {
        var words = new ushort[32 * 16];
        words[3 * 32 + 23] = 0x8000;
        var level = new RoomLevelData(32, 16, words, new byte[words.Length], new ushort[words.Length], new byte[8]);
        var samus = new SamusState { Pose = SamusPoseIds.MorphBallGroundLeftPose, XPosition = 371, YPosition = 71 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.Kinematics.ExtraYDisplacement = 0xFFFF;
        var hit = SamusMorphBallMovement.StepGrounded(bus, level, samus, 0, new RoomPlmSystem());
        AssertTrue(hit.Vertical.Collided && hit.HitCeiling, "upward platform carry publishes a ceiling collision, not a floor contact");
        AssertEqual(2, samus.Kinematics.YDirection, "native ceiling pose command selects down even from grounded carry");
        AssertEqual(71, samus.YPosition, "carried Morph Ball remains at the ceiling boundary");
        samus.Kinematics.ExtraYDisplacement = 0;
        samus.Kinematics.YSubacceleration = 0x1C00;
        SamusMorphBallMovement.StepGrounded(bus, level, samus, 1, new RoomPlmSystem());
        AssertEqual(71, samus.YPosition, "following frame moves by old zero speed, not a one-pixel grounding probe");
        AssertEqual(0x1C00, samus.Kinematics.YSubspeed, "following frame stores normal gravity for its successor");

        // The native rising helper restores pose input below one whole pixel/frame,
        // using the speed BEFORE this frame's gravity subtraction, not at the apex.
        var hurt = new SamusState { Pose = SamusPoseIds.KnockbackRightPose, XPosition = 32, YPosition = 120 };
        hurt.RefreshCollisionRadii(bus);
        hurt.InitializeAnimation(bus);
        hurt.PublishBombJumpDirection(2);
        AssertTrue(hurt.TrySetupPublishedBombJump(bus, level, false, 0), "hurt pose admits bomb start");
        SamusBombJumpMovement.Start(bus, hurt);
        hurt.Kinematics.YSpeed = 1;
        hurt.Kinematics.YSubspeed = 0;
        hurt.Kinematics.YSubacceleration = 0x4000;
        SamusBombJumpMovement.Step(bus, level, hurt, 0, new RoomPlmSystem());
        AssertTrue(hurt.BombJumpPoseInputLocked, "one-pixel speed keeps input locked despite crossing threshold this frame");
        SamusBombJumpMovement.Step(bus, level, hurt, 1, new RoomPlmSystem());
        AssertTrue(!hurt.BombJumpPoseInputLocked, "fractional upward speed restores next frame's input");
        AssertTrue(hurt.BombJumpActive, "restoring input does not prematurely end upward movement");
        AssertEqual(SamusPoseIds.KnockbackRightPose, hurt.Pose, "restoring input retains damaged pose until an actual transition");
        hurt.PublishBombJumpDirection(2);
        AssertTrue(hurt.TrySetupPublishedBombJump(bus, level, false, 0), "fresh bomb command rearms retained hurt pose");
        AssertTrue(hurt.BombJumpPoseInputLocked, "fresh bomb command locks input again");

        // Damage may interrupt a humanoid bomb start before its first moving frame.
        // Native owns independent movement/input pointers: replace only the former
        // until the common hurt-expiry command restores both.
        hurt.Pose = SamusPoseIds.NormalJumpForwardRightPose;
        hurt.RefreshCollisionRadii(bus);
        AssertTrue(SamusKnockbackMovement.Start(bus, hurt, 0, 0), "normal hurt interrupts humanoid bomb start");
        AssertTrue(!hurt.BombJumpStarting && !hurt.BombJumpActive, "hurt replaces the bomb movement owner immediately");
        AssertTrue(hurt.BombJumpPoseInputLocked, "hurt admission preserves independent bomb input lock");
        hurt.KnockbackTimer = 0;
        AssertTrue(SamusKnockbackMovement.TryFinishExpiredHitInterruption(bus, hurt), "hurt expiry consumes its transitional command");
        AssertTrue(!hurt.BombJumpPoseInputLocked, "hurt expiry restores normal pose input");
    }
}
