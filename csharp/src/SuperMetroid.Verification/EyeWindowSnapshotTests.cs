using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyEyeWindowSnapshots()
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
                AssertEqual((long)expected[0], left, "integer window ceiling endpoint");
                AssertEqual((long)expected[^1], right, "integer window floor endpoint");
            }
        }
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyReportedEyeNativeEndpoints(bus);
        var cgram = new SnesCgram(); cgram.SetColor(0, 0x392a);
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = PpuMemorySnapshot.Capture(new SnesVram(), cgram, oam);
        Rgba32[] baseline = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        int samples = 0;
        foreach (ushort width in new ushort[] { 0, 1, 24 })
        for (int angle = 0; angle < SnesAngle.TableUnitsPerTurn; angle++)
            Check(128, 120, angle, width);
        foreach ((int x, int y) in new[] { (-40, 120), (300, 120), (128, -30), (128, 270), (-32768, 32767) })
        foreach (int angle in new[] { 0, 1, 63, 64, 65, 127, 128, 191, 192, 193, 255 })
        foreach (ushort width in new ushort[] { 0, 1, 24 })
            Check(x, y, angle, width);
        foreach (MorphBallEyeBeamPhase phase in new[] { MorphBallEyeBeamPhase.Inactive, MorphBallEyeBeamPhase.PendingInitialization })
            AssertTrue(SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(bus,
                new(phase, 0, 0, SnesAngle.Zero, 0, 0, 0, 0), 0, 0) is null, "unpublished eye beam emits no operation");
        Console.WriteLine($"  Eye window snapshots: {samples} backend/codec cases agree; cartridge endpoint assertions also pass.");

        void Check(int x, int y, int angle, ushort width)
        {
            // Nonzero camera coordinates also exercise signed world-to-screen wrapping.
            var beam = new MorphBallEyeBeamRenderSnapshot(MorphBallEyeBeamPhase.Full,
                unchecked((ushort)(x + 512)), unchecked((ushort)(y + 768)),
                SnesAngle.FromTableIndex((byte)angle), width, 0x3f, 0x4c, 0x81);
            Rgba32[] expected = baseline.ToArray();
            SnesGameplayFrameRenderer.ApplyMorphBallEyeBeamColorMath(expected, bus, beam, 512, 768);
            ScanlineColorAddRenderLayer layer = SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(bus, beam, 512, 768)!;
            var packet = new RenderFrameSnapshot(new(++samples, 1, 0), new LayeredRenderSnapshot(memory, [layer], 3, 15));
            Rgba32[] actual = SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(packet));
            AssertTrue(expected.AsSpan().SequenceEqual(actual), $"eye capture origin {x},{y}; angle {angle}; width {width}");
        }
    }

    private static void VerifyReportedEyeNativeEndpoints(ISnesAddressSpace bus)
    {
        // These endpoints were emitted by executing ROM $88:E987, not by the
        // production builder. Keep this small oracle in the ordinary suite while
        // the native audit independently sweeps all angles and off-screen cases.
        (byte Angle, ushort Width, int Y, int Left, int Right)[] expected =
        [
            (160, 0, 32, 255, 0), (160, 0, 103, 168, 168),
            (160, 0, 104, 167, 167), (160, 0, 223, 48, 48),
            (160, 4, 104, 166, 167), (160, 4, 140, 123, 137),
            (160, 4, 223, 22, 69), (208, 0, 80, 112, 112),
            (208, 0, 104, 255, 0), (208, 4, 32, 0, 35),
            (208, 4, 102, 164, 166), (224, 4, 80, 140, 149),
            (224, 4, 102, 166, 167), (224, 4, 104, 255, 0),
        ];
        foreach (var sample in expected)
        {
            var beam = new MorphBallEyeBeamRenderSnapshot(MorphBallEyeBeamPhase.Full,
                552, 616, SnesAngle.FromTableIndex(sample.Angle), sample.Width, 31, 31, 0);
            ColorAddWindow row = SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(bus, beam, 384, 512)!.Windows[sample.Y];
            for (int x = 0; x < 256; x++)
                AssertEqual(x >= sample.Left && x <= sample.Right, x >= row.Left && x <= row.Right,
                    $"native reported-eye angle {sample.Angle}, width {sample.Width}, pixel {x},{sample.Y}");
        }
    }
}
