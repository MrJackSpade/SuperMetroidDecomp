using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Compares captured X-ray scanline intervals with the per-pixel reference raster.</summary>
internal static class XrayWindowGeometryVerification
{
    /// <summary>Reusable per-pixel raster, refilled for each compared camera scroll.</summary>
    internal static readonly Rgba32[] xrayGeometryReference = new Rgba32[256 * 224];

    /// <summary>Checks standing and crouching poses in both facings through beam widening and aim sweeps.</summary>
    internal static void VerifyXrayWindowGeometry()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (SamusPoseId pose in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.CrouchingRightPose, SamusPoseId.CrouchingLeftPose })
        {
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(184, 171)) };
            AssertTrue(samus.Xray.TryBegin(bus, samus, samus.PoseMetadata.ReadMovementType(bus), new XrayBeginConditions { GameState = (ushort)SuperMetroidGameState.MainGameplay }), "geometry fixture enters native X-ray posture");
            for (int frame = 0; frame < 330; frame++)
            {
                SnesButton input = (SnesButton.B | (frame < 90 ? 0 : frame < 170 ? SnesButton.Up : SnesButton.Down));
                samus.Xray.StepBeam(bus, samus, input);
                VerifyXrayWindowGeometry(bus, samus, xrayGeometryReference);
            }
        }
        Console.WriteLine("  X-ray intervals: standing/crouching, both facings, widening, aim sweep and clipped origins match per-pixel reference.");
    }
    /// <summary>Compares every gameplay pixel's X-ray membership for three camera scroll positions.</summary>
    internal static void VerifyXrayWindowGeometry(ISnesAddressSpace bus, SamusState samus, Rgba32[] reference)
    {
        if (samus.Xray.SetupStage != XraySetupStage.Complete || samus.Xray.BeamPhase is not (XrayBeamPhase.Widening or XrayBeamPhase.Full)) return;
        foreach (var scroll in new[] { (0, 0), (160, 0), (220, 180) })
        {
            var source = new Rgba32(248, 128, 64);
            Array.Fill(reference, source);
            SnesGameplayFrameRenderer.ApplyXrayWindowColorMath(reference, bus, samus.Xray, samus,
                new Position16<RoomPixels>((ushort)scroll.Item1, (ushort)scroll.Item2));
            var lines = SnesGameplayFrameRenderer.CaptureXrayWindowLines(
                SamusXrayDisplaySnapshot.Capture(samus, null, bus) ?? throw new InvalidOperationException("Active X-ray has no display snapshot."),
                new Position16<RoomPixels>((ushort)scroll.Item1, (ushort)scroll.Item2));
            for (int y = 32; y < 224; y++)
            for (int x = 0; x < 256; x++)
            {
                bool referenceInside = reference[y * 256 + x] == source;
                bool intervalInside = x >= lines[y].Left && x <= lines[y].Right;
                if (referenceInside != intervalInside)
                    throw new InvalidOperationException($"X-ray interval disagrees with per-pixel reference at ({x},{y}), angle {samus.Xray.Angle}, scroll {scroll}.");
            }
        }
    }

    /// <summary>Runs the X-ray interval comparison from the verification command line.</summary>
    [VerificationCommand("--xray-window-geometry")]
    internal static int XrayWindowGeometryCommand()
    {
        Suite(nameof(VerifyXrayWindowGeometry), () => VerifyXrayWindowGeometry());
        return 0;
    }
}
