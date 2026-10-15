using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks grapple restart after Samus changes aim while the beam is firing.</summary>
internal static class GrapplePoseRefireVerification
{
    /// <summary>Confirms early pose changes refire with a reset endpoint and stop/start sounds, while frame ten cancels and debugger saves retain the remaining window.</summary>
    internal static void VerifyGrapplePoseRefire()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(new(64, 64), new ushort[4096], new byte[4096]);
        foreach (bool left in new[] { false, true })
        foreach (int changeFrame in Enumerable.Range(1, 10))
        {
            var samus = new SamusState {
                Pose = left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)),
            };
            PrepareRetailSamusFixture(samus);
            SamusGrappleMovement.BeginFiring(bus, samus);
            for (int frame = 1; frame < changeFrame; frame++)
                SamusGrappleMovement.StepFiring(bus, level, samus, SnesButton.X);
            samus.Pose = left ? SamusPoseId.StandingAimDiagonalUpLeftPose : SamusPoseId.StandingAimDiagonalUpRightPose;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.StepFiring(bus, level, samus, SnesButton.X);
            if (changeFrame == 10)
            {
                AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "expired direction change cancels in same call");
                continue;
            }
            var grapple = samus.Grapple;
            AssertEqual(GrapplePhase.Firing, grapple.Phase, "early direction change refires");
            AssertEqual(samus.PoseMetadata.ReadShotDirection(bus), grapple.FireDirection, "refire uses current pose direction");
            AssertEqual(10, grapple.PoseChangeAutoFireTimer.RawValue, "refire resets native ten-frame timer");
            AssertTrue(samus.LiquidPhysics.SoundRequests.SequenceEqual(new[] {
                SamusGrappleRomData.Sounds.RestartStop, SamusGrappleRomData.Sounds.Fire }), "refire publishes native stop/start sound order and queue limits");
            AssertEqual(0, grapple.RopeLength, "restart does not extend in same call");
            AssertEqual(0, grapple.EndpointOffset.X.RawValue, "restart clears old X endpoint displacement");
            AssertEqual(0, grapple.EndpointOffset.Y.RawValue, "restart clears old Y endpoint displacement");
            AssertEqual(samus.Position.X.Whole + grapple.OriginOffset.X, grapple.Anchor.X, "restart hand origin X");
            AssertEqual(samus.Position.Y.Whole + grapple.OriginOffset.Y, grapple.Anchor.Y, "restart hand origin Y");
            SamusGrappleMovement.StepFiring(bus, level, samus, SnesButton.X);
            AssertTrue(grapple.Anchor.Y < samus.Position.Y.Whole + grapple.OriginOffset.Y, "restarted beam actually travels up");
            AssertTrue(left ? grapple.EndpointOffset.X.RawValue < 0 : grapple.EndpointOffset.X.RawValue > 0, "restarted beam travels toward aimed side");
        }
        var state = new SamusGrappleState { PoseChangeAutoFireTimer = NativeCountdown.Start(7) };
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, state);
        stream.Position = 0;
        AssertEqual(7, DebuggerObjectGraphSerializer.Deserialize<SamusGrappleState>(stream).PoseChangeAutoFireTimer.RawValue,
            "debugger preserves remaining refire window");
        Console.WriteLine("Grapple pose refire: mirrored frame-1..9 restarts, frame-10 cancellation, endpoint trajectory and timer serialization pass.");
    }
}
