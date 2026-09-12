using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    static void VerifyMotherBrainBeamWindow()
    {
        var bus = new TestAddressSpace();
        // A native 64-angle-wide beam selects gradients 32/96, both exactly 1.
        // This retains the indirect-run/carry fixture without replacing engine lookup data.
        WriteTestWord(bus, MotherBrainBeamRomData.ColorTable, 31);
        WriteTestWord(bus, MotherBrainBeamRomData.ColorTable + 2, 123);
        WriteTestWord(bus, MotherBrainBeamRomData.ColorTable + 4, 992);
        WriteTestWord(bus, MotherBrainBeamRomData.ColorTable + 8, ushort.MaxValue);
        var beam = new MotherBrainRainbowBeamHdmaState();
        void Step() => beam.Step(bus, true, 100, 95, SnesAngle.QuarterTurn, 0x4000);
        Step();
        AssertEqual(MotherBrainBeamRomData.InitialColor, beam.Color, "beam first frame uses E767 fixed color");
        AssertEqual((ushort)0xff73, beam.Windows[99], "right beam upper apex accumulates before writing");
        AssertEqual((ushort)0xff73, beam.Windows[100], "right beam lower apex accumulates before writing");
        AssertEqual((ushort)0xff9e, beam.Windows[143], "first indirect run last row");
        AssertEqual((ushort)0xffa3, beam.Windows[144], "second right run preserves native EC table offset");
        AssertTrue(beam.Windows[..32].ToArray().All(word => word == MotherBrainBeamRomData.EmptyWindow),
            "rainbow window excludes HUD");
        Step();
        AssertEqual((ushort)31, beam.Color, "first cycling frame reads entry zero");
        Step();
        AssertEqual((ushort)992, beam.Color, "color cursor skips odd words");
        Step();
        AssertEqual((ushort)31, beam.Color, "negative terminator selects entry zero");
        AssertEqual(0, beam.ColorCursor, "reset frame does not increment color cursor");
        Step();
        AssertEqual((ushort)31, beam.Color, "entry zero repeats after wrap as in E7F5");
        var capture = SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(beam)!;
        AssertEqual(new ColorAddWindow(115, 255, 255, 0, 0), capture.Windows[100],
            "shared render packet includes beam geometry and native color");
        int cursor = beam.ColorCursor;
        _ = SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(beam);
        AssertEqual(cursor, beam.ColorCursor, "repainting cannot advance beam simulation");
        beam.Step(bus, false, 0, 0, default, 0);
        AssertTrue(SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(beam) is null,
            "deleted HDMA object emits no render layer");
        Step();
        AssertEqual(MotherBrainBeamRomData.InitialColor, beam.Color, "new attack resets HDMA color");
        foreach (byte angle in new byte[] { 32, 96, 128, 160, 0, 224 })
        {
            beam.Step(bus, true, 100, 95, SnesAngle.FromTableIndex(angle), 512);
            bool up = angle is 96 or 128 or 160;
            ushort expected = angle switch
            {
                32 or 96 => 0x7372,
                128 or 0 => 0x7271,
                _ => 0x7170,
            };
            AssertEqual(expected, beam.Windows[up ? 99 : 100], $"native quadrant {angle} apex edge signs");
            AssertEqual(MotherBrainBeamRomData.EmptyWindow, beam.Windows[up ? 100 : 99],
                $"native quadrant {angle} stays on its ray side of the mouth");
        }
        ushort[] retained = beam.Windows.ToArray();
        beam.Step(bus, true, 100, 95, SnesAngle.ThreeQuarterTurn, 512);
        AssertTrue(retained.AsSpan().SequenceEqual(beam.Windows), "native left-axis RTS preserves prior HDMA table");
        beam.Step(bus, true, 356, 95, SnesAngle.QuarterTurn, 0x4000);
        AssertEqual((ushort)0xff73, beam.Windows[100], "DE20 uses low X byte, not wrapped enemy-header data");
        Console.WriteLine("  Mother Brain beam: native apex, HDMA split, HUD exclusion, color stride/reset and owned capture agree.");
    }
}
