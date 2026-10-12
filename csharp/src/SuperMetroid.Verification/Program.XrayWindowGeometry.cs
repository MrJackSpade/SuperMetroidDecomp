using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;

/// <summary>Represents xray window geometry verification.</summary>
internal static class XrayWindowGeometryVerification
{
    // Per-pixel reference raster, refilled for every scroll the interval check compares.
    /// <summary>Stores xray geometry reference.</summary>
    internal static readonly Rgba32[] xrayGeometryReference = new Rgba32[256 * 224];

    /// <summary>Verifies xray window geometry.</summary>
    internal static void VerifyXrayWindowGeometry()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (SamusPoseId pose in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.CrouchingRightPose, SamusPoseId.CrouchingLeftPose })
        {
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(184, 171)) };
            AssertTrue(samus.Xray.TryBegin(bus, samus, samus.PoseMetadata.ReadMovementType(bus), new XrayBeginConditions()), "geometry fixture enters native X-ray posture");
            for (int frame = 0; frame < 330; frame++)
            {
                ushort input = (ushort)(SnesButton.B | (frame < 90 ? 0 : frame < 170 ? SnesButton.Up : SnesButton.Down));
                samus.Xray.StepBeam(bus, samus, input);
                VerifyXrayWindowGeometry(bus, samus, xrayGeometryReference);
            }
        }
        Console.WriteLine("  X-ray intervals: standing/crouching, both facings, widening, aim sweep and clipped origins match per-pixel reference.");
    }
    /// <summary>Verifies xray window geometry.</summary>
    internal static void VerifyXrayWindowGeometry(ISnesAddressSpace bus, SamusState samus, Rgba32[] reference)
    {
        if (samus.Xray.SetupStage != XraySetupStage.Complete || samus.Xray.BeamPhase is not (XrayBeamPhase.Widening or XrayBeamPhase.Full)) return;
        foreach (var scroll in new[] { (0, 0), (160, 0), (220, 180) })
        {
            var source = new Rgba32(248, 128, 64);
            Array.Fill(reference, source);
            SnesGameplayFrameRenderer.ApplyXrayWindowColorMath(reference, bus, samus.Xray, samus,
                new Position16<RoomPixels>((ushort)scroll.Item1, (ushort)scroll.Item2));
            var lines = SnesGameplayFrameRenderer.CaptureXrayWindowLines(bus, samus,
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

    /// <summary>Xray window geometry command.</summary>
    [VerificationCommand("--xray-window-geometry")]
    internal static int XrayWindowGeometryCommand()
    {
        Suite(nameof(VerifyXrayWindowGeometry), () => VerifyXrayWindowGeometry());
        return 0;
    }
}
