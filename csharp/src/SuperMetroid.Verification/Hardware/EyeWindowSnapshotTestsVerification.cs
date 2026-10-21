using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies.MorphBallEye;
namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks morph-ball eye windows through snapshot encoding and software rendering.</summary>
internal static class EyeWindowSnapshotTestsVerification
{
    /// <summary>Compares intersection arithmetic, native endpoint samples, and backend pixels across angles.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyEyeWindowSnapshots()
    {
        for (long coefficient = -9; coefficient <= 9; coefficient++)
        for (long constant = -9; constant <= 9; constant++)
        for (long threshold = -9; threshold <= 9; threshold++)
        {
            long left = 0, right = 31;
            bool any = IntegerWindowIntersection.IntersectGreaterEqual(coefficient, constant, threshold, ref left, ref right);
            int[] expected = Enumerable.Range(0, 32).Where(x => coefficient * x + constant >= threshold).ToArray();
            AssertEqual(expected.Length != 0, any, "integer window nonempty decision");
            if (any)
            {
                AssertEqual(expected[0], left, "integer window ceiling endpoint");
                AssertEqual(expected[^1], right, "integer window floor endpoint");
            }
        }
        VerifyReportedEyeNativeEndpoints();
        var cgram = new SnesCgram(); cgram.SetColor(0, Bgr555.FromWord(0x392a));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = PpuMemorySnapshotConstants.CaptureFrontend(new SnesVram(), cgram, oam, 3);
        Rgba32[] baseline = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        int samples = 0;
        foreach (ushort width in new ushort[] { 0, 1, 24 })
        for (int angle = 0; angle < SnesAngle.TableUnitsPerTurn; angle++)
            Check(128, 120, angle, width);
        foreach ((int x, int y) in new[] { new VerifyEyeWindowSnapshotsCase(-40, 120), new VerifyEyeWindowSnapshotsCase(300, 120), new VerifyEyeWindowSnapshotsCase(128, -30), new VerifyEyeWindowSnapshotsCase(128, 270), new VerifyEyeWindowSnapshotsCase(-32768, 32767) })
        foreach (int angle in new[] { 0, 1, 63, 64, 65, 127, 128, 191, 192, 193, 255 })
        foreach (ushort width in new ushort[] { 0, 1, 24 })
            Check(x, y, angle, width);
        foreach (MorphBallEyeBeamPhase phase in new[] { MorphBallEyeBeamPhase.Inactive, MorphBallEyeBeamPhase.PendingInitialization })
            AssertTrue(SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(new(phase, Position16<RoomPixels>.Zero, SnesAngle.Zero, 0, 0, 0, 0), new Position16<RoomPixels>(0, 0)) is null, "unpublished eye beam emits no operation");
        Console.WriteLine($"  Eye window snapshots: {samples} backend cases agree; cartridge endpoint assertions also pass.");

        void Check(int x, int y, int angle, ushort width)
        {
            // Nonzero camera coordinates also exercise signed world-to-screen wrapping.
            var beam = new MorphBallEyeBeamRenderSnapshot(MorphBallEyeBeamPhase.Full,
                new(unchecked((ushort)(x + 512)), unchecked((ushort)(y + 768))),
                SnesAngle.FromTableIndex((byte)angle), width, 0x3f, 0x4c, 0x81);
            Rgba32[] expected = baseline.ToArray();
            SnesGameplayFrameRendererConstants.ApplyMorphBallEyeBeamColorMath(expected, beam, new Position16<RoomPixels>(512, 768));
            ScanlineColorAddRenderLayer layer = (SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(beam, new Position16<RoomPixels>(512, 768)) ?? throw new InvalidOperationException("Verification requires SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(beam, new Position16<RoomPixels>...."));
            var packet = new RenderFrameSnapshot(new(++samples, 1, 0), new LayeredRenderSnapshot(memory, [(layer ?? throw new InvalidOperationException("Verification requires layer."))], 3, 15));
            Rgba32[] actual = SoftwareFrameSnapshotRenderer.Render(packet);
            AssertTrue(expected.SequenceEqual(actual), $"eye capture origin {x},{y}; angle {angle}; width {width}");
        }
    }

    /// <summary>Compares selected $88:E987 cartridge endpoints with captured scanline windows.</summary>
    private static void VerifyReportedEyeNativeEndpoints()
    {
        // These endpoints were emitted by executing ROM $88:E987, not by the
        // production builder. Keep this small oracle in the ordinary suite while
        // the native audit independently sweeps all angles and off-screen cases.
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
            var beam = new MorphBallEyeBeamRenderSnapshot(MorphBallEyeBeamPhase.Full,
                new(552, 616), SnesAngle.FromTableIndex(sample.Angle), sample.Width, 31, 31, 0);
            ColorAddWindow row = (SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(beam, new Position16<RoomPixels>(384, 512)) ?? throw new InvalidOperationException("Verification requires SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(beam, new Position16<RoomPixels>....")).Windows[sample.Y];
            for (int x = 0; x < 256; x++)
                AssertEqual(x >= sample.Left && x <= sample.Right, x >= row.Left && x <= row.Right,
                    $"native reported-eye angle {sample.Angle}, width {sample.Width}, pixel {x},{sample.Y}");
        }
    }
}
