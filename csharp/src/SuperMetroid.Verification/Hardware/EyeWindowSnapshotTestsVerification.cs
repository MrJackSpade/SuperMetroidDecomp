using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks the morph-ball eye's beam: the cone it is captured as, and the cone the backend draws.</summary>
internal static class EyeWindowSnapshotTestsVerification
{
    /// <summary>
    /// The beam is a cone the backend draws per pixel (#1468), not a per-line table: checks the captured cone's apex,
    /// edges and band of lines across origins on and off the screen, angles and widths, then draws reported cartridge
    /// samples and checks the drawn lines against them.
    /// </summary>
    internal static void VerifyEyeWindowSnapshots()
    {
        int samples = 0;
        foreach (ushort width in new ushort[] { 0, 1, 4 })
        for (int angle = 0; angle < RotationAngle.TableUnitsPerTurn; angle++)
            Check(128, 120, angle, width);
        foreach ((int x, int y) in new[] { new VerifyEyeWindowSnapshotsCase(-40, 120), new VerifyEyeWindowSnapshotsCase(300, 120), new VerifyEyeWindowSnapshotsCase(128, -30), new VerifyEyeWindowSnapshotsCase(128, 270), new VerifyEyeWindowSnapshotsCase(-32768, 32767) })
        foreach (int angle in new[] { 0, 1, 63, 64, 65, 127, 128, 191, 192, 193, 255 })
        foreach (ushort width in new ushort[] { 0, 1, 4 })
            Check(x, y, angle, width);
        foreach (MorphBallEyeBeamPhase phase in new[] { MorphBallEyeBeamPhase.Inactive, MorphBallEyeBeamPhase.PendingInitialization })
            AssertTrue(GameplayEffectCapture.CaptureMorphBallEyeBeam(new(phase, Position16<RoomPixels>.Zero, RotationAngle.Zero, 0, RgbColor.Black), new Position16<RoomPixels>(0, 0)) is null, "unpublished eye beam emits no operation");
        VerifyReportedEyeNativeEndpoints();
        Console.WriteLine($"  Eye window snapshots: {samples} cones across origins, angles and widths; drawn cartridge samples also pass.");

        void Check(int x, int y, int angle, ushort width)
        {
            // Nonzero camera coordinates also exercise signed world-to-screen wrapping.
            var beam = new MorphBallEyeBeamCone(MorphBallEyeBeamPhase.Full,
                new(unchecked((ushort)(x + 512)), unchecked((ushort)(y + 768))),
                RotationAngle.FromTableIndex((byte)angle), width, new RgbColor(31, 12, 1));
            ColourAddWindowLayer layer = GameplayEffectCapture.CaptureMorphBallEyeBeam(beam, new Position16<RoomPixels>(512, 768))
                ?? throw new InvalidOperationException($"eye capture origin {x},{y}; angle {angle}; width {width}");
            var apex = Offset16<ScreenPixels>.Of(unchecked((short)x), unchecked((short)(y - 1)));
            AssertEqual(ScreenWindow.Of(new WindowCone(apex, angle - width, angle + width), DisplayScreen.HudHeight), layer.Window,
                $"eye cone from the line above the eye, origin {x},{y}; angle {angle}; width {width}");
            AssertEqual(new RgbColor(31, 12, 1), layer.Colour, "eye cone adds the beam's colour");
            samples++;
        }
    }

    /// <summary>
    /// Draws cartridge-reported beam lines and checks each drawn line against them: a line the cartridge leaves empty
    /// is empty, and a lit line's drawn pixels lie within one pixel of the cartridge's edges. Exact per-line edges are
    /// a presentation quirk the port does not keep.
    /// </summary>
    private static void VerifyReportedEyeNativeEndpoints()
    {
        // These endpoints were emitted by executing the cartridge's beam routine.
        EyeWindowSnapshotTestsExpected[] expected =
        [
            new EyeWindowSnapshotTestsExpected(160, 0, 32, 255, 0), new EyeWindowSnapshotTestsExpected(160, 0, 103, 168, 168),
            new EyeWindowSnapshotTestsExpected(160, 0, 104, 167, 167), new EyeWindowSnapshotTestsExpected(160, 0, 223, 48, 48),
            new EyeWindowSnapshotTestsExpected(160, 4, 104, 166, 167), new EyeWindowSnapshotTestsExpected(160, 4, 140, 123, 137),
            new EyeWindowSnapshotTestsExpected(160, 4, 223, 22, 69), new EyeWindowSnapshotTestsExpected(208, 0, 80, 112, 112),
            new EyeWindowSnapshotTestsExpected(208, 0, 104, 255, 0), new EyeWindowSnapshotTestsExpected(208, 4, 32, 0, 35),
            new EyeWindowSnapshotTestsExpected(208, 4, 102, 164, 166), new EyeWindowSnapshotTestsExpected(224, 4, 80, 140, 149),
            new EyeWindowSnapshotTestsExpected(224, 4, 102, 166, 167), new EyeWindowSnapshotTestsExpected(224, 4, 104, 255, 0),
        ];
        foreach (var sample in expected)
        {
            var beam = new MorphBallEyeBeamCone(MorphBallEyeBeamPhase.Full,
                new(552, 616), RotationAngle.FromTableIndex(sample.Angle), sample.Width, new RgbColor(31, 31, 0));
            ColourAddWindowLayer layer = GameplayEffectCapture.CaptureMorphBallEyeBeam(beam, new Position16<RoomPixels>(384, 512))
                ?? throw new InvalidOperationException("A full eye beam captures its cone.");
            // The beam over a black screen, as the display renderer draws it.
            RgbaPixel[] drawn = DisplayRendering.Render(new LayeredScene(FramePaint.FromNative(new RgbColor[SnesCgram.ColorCount], []),
                new DisplayLayer[] { layer }, DisplayBrightness.Full));
            int[] lit = Enumerable.Range(0, DisplayScreen.Width)
                .Where(x => drawn[sample.Y * DisplayScreen.Width + x].R != 0).ToArray();
            string context = $"reported eye angle {sample.Angle}, width {sample.Width}, line {sample.Y}";
            if (sample.Left > sample.Right)
            {
                AssertEqual(0, lit.Length, $"{context}: the cartridge's empty line is drawn empty");
                continue;
            }
            AssertTrue(lit.Length != 0, $"{context}: the cartridge's lit line is drawn lit");
            AssertTrue(lit[0] >= sample.Left - 1 && lit[^1] <= sample.Right + 1,
                $"{context}: drawn pixels {lit[0]}-{lit[^1]} lie within one pixel of {sample.Left}-{sample.Right}");
        }
    }
}
