using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks bomb-jump wall contact and carried Morph Ball ceiling response.</summary>
internal static class BombJumpWallVerification
{
    /// <summary>Confirms an X-only wall hit clears horizontal speed while retaining the bomb-jump handler and ascent.</summary>
    internal static void VerifyBombJumpWallContact()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var words = new ushort[16 * 16];
        for (int row = 0; row < 10; row++) words[row * 16 + 4] = 0x8000;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);
        var samus = new SamusState { Pose = SamusPoseId.MorphBallGroundRightPose, Position = new(new(59, 0), new(80, 0))};
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0xF000);
        samus.Kinematics.YSubacceleration = 0x4000;
        samus.RequestMorphedBombJump(3);
        SamusBombJumpMovement.Start(samus);
        var result = SamusBombJumpMovement.Step(level, samus, 0, new RoomPlmSystem());
        // Only the X mover's wall hit clears the base speed the bomb-jump calculator just accelerated.
        AssertTrue(samus.HorizontalSpeed.BaseFixed == 0, $"diagonal bomb jump hits the shaft wall (X={samus.Kinematics.Position.X.RawValue:X8}, radius={samus.Kinematics.Radius.X})");
        AssertTrue(result.Vertical is { Collided: false }, "shaft wall leaves vertical travel clear");
        // Original $90:E032 bytes: GrapplePoseAudit bomb-wall. A horizontal collision
        // must not end the special handler; the native branch tests the later Y result.
        AssertTrue(samus.BombJumpActive, "wall-only collision retains native bomb-jump handler");
        AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "bomb-jump wall hit clears horizontal momentum without cancelling ascent");
        AssertEqual(0x0803, samus.BombJumpDirection, "native bomb-wall direction remains armed");
        AssertEqual(59, samus.Position.X.Whole, "native bomb-wall X endpoint");
        AssertEqual(77, samus.Position.Y.Whole, "native bomb-wall Y endpoint");
        AssertEqual(0x4000, samus.Kinematics.Position.Y.Fraction, "native bomb-wall Y fraction");
        AssertEqual(2, samus.Kinematics.YSpeed, "native bomb-wall next speed");
        AssertEqual(0x8000, samus.Kinematics.YSubspeed, "native bomb-wall next subspeed");
        Console.WriteLine("  Bomb jump: wall-only collision preserves native ascent and handler lifetime.");
        Suite(nameof(VerifyCarriedMorphCeiling), () => VerifyCarriedMorphCeiling());
    }

    /// <summary>Checks upward platform carry, deferred ceiling pose response, and hurt interruption of bomb input.</summary>
    private static void VerifyCarriedMorphCeiling()
    {
        var words = new ushort[32 * 16];
        words[3 * 32 + 23] = 0x8000;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 16), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);
        var samus = new SamusState { Pose = SamusPoseId.MorphBallGroundLeftPose, Position = new(new(371, 0), new(71, 0))};
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(-1);
        var hit = SamusMorphBallMovement.StepGrounded(level, samus, 0, new RoomPlmSystem());
        AssertTrue(hit.Vertical.Collided && hit.HitCeiling, "upward platform carry publishes a ceiling collision, not a floor contact");
        AssertEqual(SamusVerticalDirection.None, samus.Kinematics.YDirection, "movement leaves ceiling response deferred until pose priority is resolved");
        samus.PoseTransitions.ApplySolidCeilingCollision();
        AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.YDirection, "native ceiling pose command selects down even from grounded carry");
        AssertEqual(71, samus.Position.Y.Whole, "carried Morph Ball remains at the ceiling boundary");
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(0);
        samus.Kinematics.YSubacceleration = 0x1C00;
        SamusMorphBallMovement.StepGrounded(level, samus, 1, new RoomPlmSystem());
        AssertEqual(71, samus.Position.Y.Whole, "following frame moves by old zero speed, not a one-pixel grounding probe");
        AssertEqual(0x1C00, samus.Kinematics.YSubspeed, "following frame stores normal gravity for its successor");

        // The native rising helper restores pose input below one whole pixel/frame,
        // using the speed BEFORE this frame's gravity subtraction, not at the apex.
        var hurt = new SamusState { Pose = SamusPoseId.KnockbackRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(32, 120)) };
        hurt.PoseMetadata.RefreshCollisionRadii();
        hurt.Presentation.InitializeAnimation();
        hurt.PoseMetadata.PublishBombJumpDirection(2);
        AssertTrue(hurt.PoseMetadata.TrySetupPublishedBombJump(level, false, 0, controllerNewInput: 0), "hurt pose admits bomb start");
        SamusBombJumpMovement.Start(hurt);
        hurt.Kinematics.YSpeed = 1;
        hurt.Kinematics.YSubspeed = 0;
        hurt.Kinematics.YSubacceleration = 0x4000;
        SamusBombJumpMovement.Step(level, hurt, 0, new RoomPlmSystem());
        AssertTrue(hurt.BombJumpPoseInputLocked, "one-pixel speed keeps input locked despite crossing threshold this frame");
        SamusBombJumpMovement.Step(level, hurt, 1, new RoomPlmSystem());
        AssertTrue(!hurt.BombJumpPoseInputLocked, "fractional upward speed restores next frame's input");
        AssertTrue(hurt.BombJumpActive, "restoring input does not prematurely end upward movement");
        AssertEqual(SamusPoseId.KnockbackRightPose, hurt.Pose, "restoring input retains damaged pose until an actual transition");
        hurt.PoseMetadata.PublishBombJumpDirection(2);
        AssertTrue(hurt.PoseMetadata.TrySetupPublishedBombJump(level, false, 0, controllerNewInput: 0), "fresh bomb command rearms retained hurt pose");
        AssertTrue(hurt.BombJumpPoseInputLocked, "fresh bomb command locks input again");

        // Damage may interrupt a humanoid bomb start before its first moving frame.
        // Native owns independent movement/input pointers: replace only the former
        // until the common hurt-expiry command restores both.
        hurt.Pose = SamusPoseId.NormalJumpForwardRightPose;
        hurt.PoseMetadata.RefreshCollisionRadii();
        AssertTrue(SamusKnockbackMovement.Start(hurt, 0, new KnockbackLaunch { KnockbackXDirection = KnockbackHorizontalDirection.Left }), "normal hurt interrupts humanoid bomb start");
        AssertTrue(!hurt.BombJumpStarting && !hurt.BombJumpActive, "hurt replaces the bomb movement owner immediately");
        AssertTrue(hurt.BombJumpPoseInputLocked, "hurt admission preserves independent bomb input lock");
        hurt.KnockbackTimer = NativeCountdown.Start(0);
        AssertTrue(SamusKnockbackMovement.TryFinishExpiredHitInterruption(hurt), "hurt expiry consumes its transitional command");
        AssertTrue(!hurt.BombJumpPoseInputLocked, "hurt expiry restores normal pose input");
    }

    /// <summary>Runs the wall-contact and carried-ceiling bomb-jump checks.</summary>
    [VerificationCommand("--bomb-wall")]
    internal static int BombWallCommand()
    {
        Suite(nameof(VerifyBombJumpWallContact), () => VerifyBombJumpWallContact());
        return 0;
    }
}
