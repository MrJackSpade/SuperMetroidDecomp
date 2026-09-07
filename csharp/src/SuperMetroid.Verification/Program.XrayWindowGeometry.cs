using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyXrayWindowGeometry()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (byte pose in new[] { SamusPoseIds.FacingRightNormalPose, SamusPoseIds.FacingLeftNormalPose,
            SamusPoseIds.CrouchingRightPose, SamusPoseIds.CrouchingLeftPose })
        {
            var samus = new SamusState { Pose = pose, XPosition = 184, YPosition = 171 };
            AssertTrue(samus.Xray.TryBegin(bus, samus, samus.ReadMovementType(bus)), "geometry fixture enters native X-ray posture");
            for (int frame = 0; frame < 330; frame++)
            {
                ushort input = (ushort)(SnesButton.B | (frame < 90 ? 0 : frame < 170 ? SnesButton.Up : SnesButton.Down));
                samus.Xray.StepBeam(bus, samus, input);
                VerifyXrayWindowGeometry(bus, samus);
            }
        }
        Console.WriteLine("  X-ray intervals: standing/crouching, both facings, widening, aim sweep and clipped origins match per-pixel reference.");
    }
    private static void VerifyXrayWindowGeometry(ISnesAddressSpace bus, SamusState samus)
    {
        if (samus.Xray.SetupStage != 0 || samus.Xray.BeamPhase is not (XrayBeamPhase.Widening or XrayBeamPhase.Full)) return;
        foreach (var scroll in new[] { (0, 0), (160, 0), (220, 180) })
        {
            var source = new Rgba32(248, 128, 64);
            var reference = Enumerable.Repeat(source, 256 * 224).ToArray();
            SnesGameplayFrameRenderer.ApplyXrayWindowColorMath(reference, bus, samus.Xray, samus,
                (ushort)scroll.Item1, (ushort)scroll.Item2);
            var lines = SnesGameplayFrameRenderer.CaptureXrayWindowLines(bus, samus,
                (ushort)scroll.Item1, (ushort)scroll.Item2);
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
}
