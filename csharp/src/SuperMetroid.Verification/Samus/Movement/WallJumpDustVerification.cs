using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Fx;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Samus.Movement.WallJumpExpansionVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the atmospheric dust produced by normal and grapple wall jumps.</summary>
internal static class WallJumpDustVerification
{
    /// <summary>Compares facing, liquid suppression, timer and position with native output, and checks the dust holds nothing up.</summary>
    internal static void VerifyWallJumpDust()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyWallJumpExpansion), () => VerifyWallJumpExpansion());
        foreach (bool grapple in new[] { false, true })
        foreach (bool facingRight in new[] { false, true })
        for (int medium = 0; medium < 5; medium++)
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)) };
            // Grapple contact faces the wall; its accepted wall jump reverses that pose.
            samus.Pose = grapple
                ? facingRight ? SamusPoseId.GrappleWallContactRightPose : SamusPoseId.GrappleWallContactLeftPose
                : facingRight ? SamusPoseId.SpinJumpRightPose : SamusPoseId.SpinJumpLeftPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            var liquid = samus.LiquidPhysics;
            liquid.WaterSurfaceY = medium is 1 or 3 ? 100 : medium == 4 ? 114 : null;
            liquid.LavaAcidSurfaceY = medium == 2 ? 100 : null;
            liquid.IgnoresWaterSurface = medium == 3;
            var earlier = new SamusParticle(SamusParticleKind.SurfaceSplash, 1, SamusParticleStart.Shown(9), new(55, 66), Offset16<RoomPixels>.Zero);
            samus.Presentation.Particles.Add(earlier);
            if (grapple) SamusAerialPoseTransitions.ApplyGrappleWallJump(samus);
            else SamusAerialPoseTransitions.ApplyWallJumpTrigger(samus);

            bool suppressed = medium is 1 or 2;
            IReadOnlyList<SamusParticle> particles = samus.Presentation.Particles.All;
            string context = $"wall dust grapple={grapple} right={facingRight} medium={medium}";
            AssertEqual(suppressed ? 1 : 2, particles.Count, context + " particles");
            AssertTrue(ReferenceEquals(earlier, particles[0]) && earlier.AnimationStep == 1, context + " leaves the earlier particle");
            AssertTrue(liquid.EffectWaits.BubbleCanForm && liquid.EffectWaits.SprayCanStart, context + " holds nothing up");
            if (suppressed)
                continue;
            // These exact native outputs are independently checked by the
            // cartridge-byte fixture: GrapplePoseAudit audit.exe ROM wall-jump-dust.
            SamusParticle dust = particles[1];
            AssertEqual(SamusParticleKind.Dust, dust.Kind, context + " kind");
            AssertEqual(0, dust.AnimationStep, context + " frame");
            AssertEqual(3, dust.StepUpdatesLeft, context + " timer");
            AssertEqual(facingRight ? 122 : 134, dust.Position.X, context + " X");
            AssertEqual(114, dust.Position.Y, context + " Y");
        }
        Console.WriteLine("Wall-jump dust: both launch paths match native facing, liquid boundary, timer and position outputs.");
    }

    /// <summary>Runs the focused wall-jump dust regression.</summary>
    [VerificationCommand("--wall-jump-dust")]
    internal static int WallJumpDustCommand()
    {
        Suite(nameof(VerifyWallJumpDust), () => VerifyWallJumpDust());
        return 0;
    }
}
