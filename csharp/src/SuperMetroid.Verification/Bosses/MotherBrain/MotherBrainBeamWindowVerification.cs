using SuperMetroid.Tooling;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain's rainbow beam lines, colour cycling, and captured render data.</summary>
internal static class MotherBrainBeamWindowVerification
{
    /// <summary>Checks apex geometry, HUD exclusion, native color stride, quadrant signs, and paint purity.</summary>
    internal static void VerifyMotherBrainBeamWindow()
    {
        AssertOwnsNoScanlineTable(typeof(MotherBrainRainbowBeamEffect));
        // A native 64-angle-wide beam selects gradients 32/96, both exactly 1.
        // This retains the indirect-run/carry fixture without replacing engine lookup data.
        var paletteDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            RepositoryInstallation.Installation.MapDirectory, MotherBrainRainbowPaletteFormat.FileName)));
        var cycle = new JsonArray();
        for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.BeamCycleColorCount; frame++)
            cycle.Add(new JsonObject { ["red"] = frame % 2 == 0 ? 31 : 0,
                ["green"] = frame % 2 == 0 ? 0 : 31, ["blue"] = 0 });
        paletteDocument["beamCycle"] = cycle;
        var beam = new MotherBrainRainbowBeamEffect
        {
            PresentationColors = MotherBrainRainbowPalettePresentation.Load(new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(paletteDocument.ToJsonString()))),
        };
        AssertTrue(beam.Geometry is null, "no geometry is published before the first build");
        AssertTrue(RainbowBeamWindowBuilder.Build(null).All(line => line == MotherBrainBeamLine.None),
            "a missing geometry misses every line");
        void Step() => beam.Step(true, new(100, 95), SnesAngle.QuarterTurn, Width(64));
        Step();
        AssertEqual(MotherBrainBeamRomData.InitialColor, beam.Color, "beam first frame uses E767 fixed color");
        AssertEqual(new MotherBrainRainbowBeamGeometry(114, 100, 32, 96, MotherBrainBeamRomData.Direction.Right),
            beam.Geometry, "right beam publishes the low-byte mouth X, mouth Y and both wrapped edges");
        MotherBrainBeamLine[] windows = RainbowBeamWindowBuilder.Build(beam.Geometry);
        AssertEqual(new MotherBrainBeamLine(0x73, 0xff), windows[99], "right beam upper apex accumulates before writing");
        AssertEqual(new MotherBrainBeamLine(0x73, 0xff), windows[100], "right beam lower apex accumulates before writing");
        AssertEqual(new MotherBrainBeamLine(0x9e, 0xff), windows[143], "first indirect run last row");
        AssertEqual(new MotherBrainBeamLine(0xa3, 0xff), windows[144], "second right run preserves native EC table offset");
        AssertTrue(windows.Take(32).All(line => line == MotherBrainBeamLine.None),
            "rainbow beam misses the HUD");
        Step();
        AssertEqual(31, beam.Color, "first cycling frame reads entry zero");
        Step();
        AssertEqual(992, beam.Color, "color cursor advances to the second installed sample");
        for (int frame = 2; frame < MotherBrainRainbowPaletteFormat.BeamCycleColorCount; frame++)
        {
            Step();
            AssertEqual((ushort)(frame % 2 == 0 ? 31 : 992), beam.Color,
                $"beam installed cycle color {frame}");
            AssertEqual((frame + 1) * MotherBrainBeamRomData.ColorStride, beam.ColorCursor,
                "beam retains native byte-cursor stride");
        }
        Step();
        AssertEqual(31, beam.Color, "negative terminator selects entry zero");
        AssertEqual(0, beam.ColorCursor, "reset frame does not increment color cursor");
        Step();
        AssertEqual(31, beam.Color, "entry zero repeats after wrap as in E7F5");
        MotherBrainRainbowBeamRenderSnapshot snapshot = beam.RenderSnapshot;
        AssertEqual(new MotherBrainRainbowBeamRenderSnapshot(true, beam.Color, beam.Geometry), snapshot,
            "render snapshot carries the active flag, color and geometry by value");
        var capture = SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(snapshot);
        AssertEqual(new ColorAddWindow(115, 255, 255, 0, 0), (capture ?? throw new InvalidOperationException("Verification requires capture.")).Windows[100],
            "shared render packet includes beam geometry and native color");
        int cursor = beam.ColorCursor;
        _ = SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(beam.RenderSnapshot);
        AssertEqual(cursor, beam.ColorCursor, "repainting cannot advance beam simulation");
        beam.Step(false, new(0, 0), default, 0);
        AssertTrue(SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(beam.RenderSnapshot) is null,
            "deleted HDMA object emits no render layer");
        AssertEqual(snapshot.Geometry, beam.Geometry, "deactivation keeps the last geometry");
        Step();
        AssertEqual(MotherBrainBeamRomData.InitialColor, beam.Color, "new attack resets HDMA color");
        foreach (byte angle in new byte[] { 32, 96, 128, 160, 0, 224 })
        {
            beam.Step(true, new(100, 95), SnesAngle.FromTableIndex(angle), 512);
            bool up = angle is 96 or 128 or 160;
            MotherBrainBeamLine expected = angle switch
            {
                32 or 96 => new MotherBrainBeamLine(0x72, 0x73),
                128 or 0 => new MotherBrainBeamLine(0x71, 0x72),
                _ => new MotherBrainBeamLine(0x70, 0x71),
            };
            MotherBrainBeamLine[] quadrant = RainbowBeamWindowBuilder.Build(beam.Geometry);
            AssertEqual(expected, quadrant[up ? 99 : 100], $"native quadrant {angle} apex edge signs");
            AssertEqual(MotherBrainBeamLine.None, quadrant[up ? 100 : 99],
                $"native quadrant {angle} stays on its ray side of the mouth");
        }
        MotherBrainRainbowBeamGeometry? retained = beam.Geometry;
        beam.Step(true, new(100, 95), SnesAngle.ThreeQuarterTurn, 512);
        AssertEqual(retained, beam.Geometry, "native left-axis RTS preserves prior geometry");
        beam.Step(true, new(356, 95), SnesAngle.QuarterTurn, Width(64));
        AssertEqual(114, (beam.Geometry ?? throw new InvalidOperationException("Verification requires beam.Geometry.")).MouthScreenX, "DE20 uses low X byte, not wrapped enemy-header data");
        AssertEqual(new MotherBrainBeamLine(0x73, 0xff), RainbowBeamWindowBuilder.Build(beam.Geometry)[100],
            "low-byte mouth X rasterizes like the unwrapped head");
        AssertThrows<InvalidDataException>(() => beam.Step(true, new(100, 27), SnesAngle.QuarterTurn, Width(64)),
            "a mouth above the work area fails during the simulation update");
        VerifyGoldenWindows(beam.PresentationColors);
        Console.WriteLine("  Mother Brain beam: native apex, HDMA split, HUD exclusion, retained geometry, color stride/reset, owned capture and golden windows agree.");
    }

    /// <summary>
    /// Line-for-line goldens of the native rasterization, which moved verbatim from the simulation
    /// to the renderer: one per dispatcher, the down-left zero sentinel, the rightward overflow
    /// break and both retain cases.
    /// </summary>
    private static void VerifyGoldenWindows(MotherBrainRainbowPalettePresentation colors)
    {
        var beam = new MotherBrainRainbowBeamEffect { PresentationColors = colors };
        string Build(int x, int y, byte angle, ushort width)
        {
            beam.Step(false, default, default, 0);
            beam.Step(true, new(unchecked((ushort)x), (ushort)y), SnesAngle.FromTableIndex(angle), width);
            return Digest(RainbowBeamWindowBuilder.Build(beam.Geometry));
        }
        AssertEqual("481275D4FA0BB34E", Build(100, 95, 64, Width(64)), "right dispatcher golden");
        AssertEqual("BB8664C54614AA03", Build(100, 95, 128, Width(32)), "up dispatcher golden");
        AssertEqual("6E2CA9E2B685191B", Build(150, 60, 0, Width(32)), "down dispatcher golden");
        string downLeft = Build(16, 80, 224, Width(32));
        AssertEqual("CFBC8396E8E49C1E", downLeft, "down-left zero-sentinel golden");
        AssertEqual(MotherBrainBeamRomData.Direction.Down, (beam.Geometry ?? throw new InvalidOperationException("Verification requires beam.Geometry.")).Direction, "down-left fixture uses the down dispatcher");
        AssertTrue(beam.Geometry.Value.EdgeA >= 128 && beam.Geometry.Value.EdgeB >= 128, "down-left fixture has both edges negative");
        MotherBrainBeamLine[] downLeftWindows = RainbowBeamWindowBuilder.Build(beam.Geometry);
        AssertTrue(downLeftWindows[beam.Geometry.Value.MouthY + 1] != MotherBrainBeamLine.None
            && downLeftWindows[^1] == MotherBrainBeamLine.None,
            "down-left rows whose clamped edges reach the zero sentinel are empty");
        string overflow = Build(226, 120, 64, Width(8));
        AssertEqual("8A62CA68461C8B8F", overflow, "rightward overflow-break golden");
        MotherBrainBeamLine[] overflowWindows = RainbowBeamWindowBuilder.Build(beam.Geometry);
        AssertTrue(overflowWindows.Any(line => line != MotherBrainBeamLine.None)
            && overflowWindows[MotherBrainBeamRomData.FirstLine] == MotherBrainBeamLine.None,
            "overflow fixture stops writing once an edge passes $FFFF");

        // Retain after a prior build, in the same active run.
        beam.Step(false, default, default, 0);
        beam.Step(true, new(100, 95), SnesAngle.FromTableIndex(128), Width(32));
        beam.Step(true, new(100, 95), SnesAngle.ThreeQuarterTurn, 512);
        AssertEqual("BB8664C54614AA03", Digest(RainbowBeamWindowBuilder.Build(beam.Geometry)), "retain keeps the previous build");
        // Retain after deactivation and reactivation.
        beam.Step(false, default, default, 0);
        beam.Step(true, new(40, 150), SnesAngle.ThreeQuarterTurn, 512);
        AssertEqual("BB8664C54614AA03", Digest(RainbowBeamWindowBuilder.Build(beam.Geometry)),
            "retain after reactivation still shows the build from the earlier attack");
    }

    /// <summary>Native 8.8 angular-width word for a whole number of 256-unit-turn table units.</summary>
    private static ushort Width(int wholeTableUnits) => (ushort)SnesEightEight.FromWhole(wholeTableUnits);

    /// <summary>Hashes each line's left and right columns, in line order, for a stable comparison.</summary>
    private static string Digest(MotherBrainBeamLine[] lines)
    {
        var bytes = new byte[lines.Length * 2];
        for (int i = 0; i < lines.Length; i++)
        {
            bytes[i * 2] = checked((byte)lines[i].Left);
            bytes[i * 2 + 1] = checked((byte)lines[i].Right);
        }
        return Convert.ToHexString(SHA256.HashData(bytes))[..16];
    }

    /// <summary>Asserts that a simulation effect owner stores no per-scanline array.</summary>
    [ReflectionAudit("metadata shape is the subject: a simulation effect owner must hold no per-scanline register table")]
    internal static void AssertOwnsNoScanlineTable(Type owner)
    {
        foreach (FieldInfo field in owner.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            AssertTrue(!field.FieldType.IsArray, $"{owner.Name}.{field.Name} stores a per-scanline table in the simulation");
    }

    /// <summary>Runs the focused Mother Brain beam window and color check.</summary>
    [VerificationCommand("--mother-brain-beam-fixture")]
    internal static int MotherBrainBeamFixtureCommand()
    {
     VerifyMotherBrainBeamWindow(); return 0;
    }
}
