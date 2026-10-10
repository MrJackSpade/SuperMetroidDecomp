using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// #1275 Mother Brain glitch movie. The drained controllers store a pose without
    /// InitializeSamusPose_1, so an airborne Samus keeps her cached spin-jump movement type
    /// (and can wall-jump out); the Baby's latch touch runs before its main AI with the strict
    /// $A0:A07A test; Samus command $19 cancels the pending pose transition.
    /// </summary>
    private static void VerifyMotherBrainGlitchMechanics()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        // $91:E571 writes only $0A1C: the spin jump's $0A1E/$0A1F stay in force.
        var spinning = new SamusState { Pose = SamusPoseId.SpinJumpRightPose, XPosition = 0x80, YPosition = 0x80 };
        spinning.RefreshCollisionRadii(bus);
        spinning.InitializeAnimation(bus);
        spinning.Drained.PutStanding(bus, spinning);
        AssertEqual(SamusPoseId.DrainedStandingRightPose, spinning.Pose, "drained controller installs the standing drained pose");
        AssertEqual(SamusPoseId.SpinJumpRightPose, spinning.InitializedPose, "drained controller skips InitializeSamusPose_1");
        AssertEqual(SamusMovementType.SpinJumping, spinning.ReadMovementType(bus), "cached $0A1F keeps the spin-jump movement type");

        // $90:9DB2 tests only the Screw Attack poses; any other pose rewinds to frame $0A.
        spinning.ApplyWallContactAnimationRewind();
        AssertEqual((ushort)0x0a, spinning.AnimationFrame, "wall-contact rewind runs for the drained pose");
        // $91:EABE is gated by the spin mover, i.e. the cached type, not the live pose.
        spinning.ApplyWallJumpTrigger(bus);
        AssertEqual(SamusPoseId.WallJumpRightPose, spinning.Pose, "a drained spinning Samus wall-jumps out");
        AssertEqual(SamusPoseId.WallJumpRightPose, spinning.InitializedPose, "the wall jump initializes its pose");

        // $90:F3FB returns carry set; Run_Samus_Command then cancels the pending transition.
        var frozen = new SamusState { Pose = SamusPoseId.FacingRightNormalPose };
        SamusDrainedState.FreezeForHyperBeamAcquisition(frozen);
        AssertTrue(frozen.PendingPoseTransitionCancelled, "Samus command $19 cancels the pending pose transition");

        // $A0:A07A: touch requires |dx| - SamusXRadius < enemy X radius (strict), skips an
        // invincible Samus, and uses the Baby's position before this frame's main AI moves it.
        ushort LatchVelocity(int startDx, ushort invincibility)
        {
            var samus = new SamusState { Health = 99, XPosition = 0x0080, YPosition = 0x00a0, InvincibilityTimer = invincibility };
            samus.RefreshCollisionRadii(bus);
            var baby = new BabyMetroidCutsceneState();
            baby.Initialize(inheritedXSubposition: 0, inheritedYSubposition: 0);
            void Set(string name, object value) => typeof(BabyMetroidCutsceneState).GetProperty(name)!.SetValue(baby, value);
            Set(nameof(baby.Phase), BabyMetroidCutscenePhase.LatchOntoSamus);
            Set(nameof(baby.XPosition), (ushort)(0x0080 + startDx));
            Set(nameof(baby.YPosition), (ushort)0x008c);
            // Moving left a whole pixel per frame: after this frame's mover the gap shrinks by one.
            Set(nameof(baby.XVelocity), (ushort)0xff00);
            Set(nameof(baby.YVelocity), (ushort)0);
            var motherBrain = new MotherBrainRainbowBeamAttackSequence();
            baby.Step(bus, samus, motherBrain, HeadOf(motherBrain), layer1X: 0, layer1Y: 0);
            return baby.XVelocity;
        }
        // Samus's X radius is five for every humanoid pose.
        int touchingDx = 5 + BabyMetroidCutsceneState.XHitboxRadius - 1;
        ushort untouched = LatchVelocity(touchingDx, invincibility: 1);
        AssertTrue(LatchVelocity(touchingDx, invincibility: 0) != untouched, "the Baby touches a vulnerable Samus inside the strict gap");
        AssertEqual(untouched, LatchVelocity(touchingDx + 1, invincibility: 0),
            "a gap equal to the summed radii is not a touch, even though the mover then closes it");
        Console.WriteLine("Mother Brain glitch mechanics: uninitialized drained pose, wall-jump escape, command $19 cancel and strict pre-AI Baby touch agree.");
    }
}
