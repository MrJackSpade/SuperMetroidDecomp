using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>Verifies pose-change refiring during the native ten-frame grapple window, including direction, endpoint reset, expiration, and saved timer state.</summary>
    private static void VerifyGrapplePoseRefire()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(64, 64, new ushort[4096], new byte[4096]);
        foreach (bool left in new[] { false, true })
        foreach (int changeFrame in Enumerable.Range(1, 10))
        {
            var samus = new SamusState {
                Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose,
                XPosition = 512, YPosition = 512,
            };
            PrepareRetailSamusFixture(samus);
            SamusGrappleMovement.BeginFiring(bus, samus);
            for (int frame = 1; frame < changeFrame; frame++)
                SamusGrappleMovement.StepFiring(bus, level, samus, (ushort)SnesButton.X);
            samus.Pose = left ? SamusPoseIds.StandingAimDiagonalUpLeftPose : SamusPoseIds.StandingAimDiagonalUpRightPose;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.StepFiring(bus, level, samus, (ushort)SnesButton.X);
            if (changeFrame == 10)
            {
                AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "expired direction change cancels in same call");
                continue;
            }
            var grapple = samus.Grapple;
            AssertEqual(GrapplePhase.Firing, grapple.Phase, "early direction change refires");
            AssertEqual(samus.ReadShotDirection(bus), grapple.FireDirection, "refire uses current pose direction");
            AssertEqual(10, grapple.PoseChangeAutoFireTimer, "refire resets native ten-frame timer");
            AssertTrue(samus.LiquidPhysics.SoundRequests.SequenceEqual(new[] {
                SamusGrappleRomData.Sounds.RestartStop, SamusGrappleRomData.Sounds.Fire }), "refire publishes native stop/start sound order and queue limits");
            AssertEqual(0, grapple.RopeLength, "restart does not extend in same call");
            AssertEqual(0, grapple.EndpointXOffsetFixed, "restart clears old X endpoint displacement");
            AssertEqual(0, grapple.EndpointYOffsetFixed, "restart clears old Y endpoint displacement");
            AssertEqual(samus.XPosition + grapple.OriginXOffset, grapple.AnchorX, "restart hand origin X");
            AssertEqual(samus.YPosition + grapple.OriginYOffset, grapple.AnchorY, "restart hand origin Y");
            SamusGrappleMovement.StepFiring(bus, level, samus, (ushort)SnesButton.X);
            AssertTrue(grapple.AnchorY < samus.YPosition + grapple.OriginYOffset, "restarted beam actually travels up");
            AssertTrue(left ? grapple.EndpointXOffsetFixed < 0 : grapple.EndpointXOffsetFixed > 0, "restarted beam travels toward aimed side");
        }
        var state = new SamusGrappleState { PoseChangeAutoFireTimer = 7 };
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, state);
        stream.Position = 0;
        AssertEqual(7, DebuggerObjectGraphSerializer.Deserialize<SamusGrappleState>(stream).PoseChangeAutoFireTimer,
            "debugger preserves remaining refire window");
        var fields = typeof(SamusGrappleState).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .OrderBy(field => field.MetadataToken).ToArray();
        string[] quarters = ["<XQuarterSubVelocity>k__BackingField", "<XQuarterVelocity>k__BackingField",
            "<YQuarterSubVelocity>k__BackingField", "<YQuarterVelocity>k__BackingField"];
        var preQuarter = DebuggerStateFieldMigrations.WithoutIntroductions(typeof(SamusGrappleState), fields, quarters);
        AssertTrue(preQuarter.SequenceEqual(fields.Where(field => !quarters.Contains(field.Name))),
            "pre-quarter-velocity grapple layout omits only the four quarter words");
        var preSlowScroll = DebuggerStateFieldMigrations.WithoutIntroductions(typeof(SamusGrappleState), fields,
            [.. quarters, "<SlowScrolling>k__BackingField"]);
        AssertTrue(preSlowScroll.SequenceEqual(fields.Where(field =>
                !quarters.Contains(field.Name) && field.Name != "<SlowScrolling>k__BackingField")),
            "pre-slow-scroll grapple layout omits only the slow-scrolling flag and quarter words");
        var legacy = DebuggerStateFieldMigrations.WithoutIntroductions(typeof(SamusGrappleState), fields,
            [.. quarters, "<SlowScrolling>k__BackingField", "<PoseChangeAutoFireTimer>k__BackingField"]);
        AssertTrue(legacy.SequenceEqual(fields.Where(field => !quarters.Contains(field.Name) &&
                field.Name is not ("<PoseChangeAutoFireTimer>k__BackingField" or "<SlowScrolling>k__BackingField"))),
            "legacy grapple migration retains old field identities");
        Console.WriteLine("Grapple pose refire: mirrored frame-1..9 restarts, frame-10 cancellation, endpoint trajectory and timer serialization pass.");
    }
}
