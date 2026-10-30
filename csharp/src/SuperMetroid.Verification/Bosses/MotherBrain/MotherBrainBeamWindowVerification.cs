using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Checks Mother Brain's rainbow beam cone, colour cycling, and captured render data.</summary>
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
            PresentationColors = MotherBrainRainbowPaletteFile.Read(new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(paletteDocument.ToJsonString()))),
        };
        AssertTrue(beam.Geometry is null, "no geometry is published before the first build");
        AssertEqual(ScreenWindow.Closed, (GameplayEffectCapture.CaptureMotherBrainRainbowBeam(
                new MotherBrainRainbowBeamRenderSnapshot(true, RgbColor.Black, null))
                ?? throw new InvalidOperationException("An active beam captures its window.")).Window,
            "a missing geometry lights nothing");
        void Step() => beam.Step(true, new(100, 95), RotationAngle.QuarterTurn, Width(64));
        Step();
        AssertEqual(MotherBrainBeamDefinitions.InitialColor, beam.Color, "beam first frame uses E767 fixed color");
        AssertEqual(new MotherBrainRainbowBeamGeometry(114, 100, 32, 96, MotherBrainBeamDirection.Right),
            beam.Geometry, "right beam publishes the low-byte mouth X, mouth Y and both wrapped edges");
        // The beam's edges are measured from straight down: 32 and 96 reflect to the screen's 96 and 32, a cone from
        // up-right to down-right of the mouth, below the HUD (#1468).
        AssertEqual(ScreenWindow.Of(new WindowCone(Offset16<ScreenPixels>.Of(114, 100), 32, 96), MotherBrainBeamDefinitions.FirstLine),
            Cone(beam.RenderSnapshot), "right beam is a cone from the mouth between its reflected edges");
        RgbaPixel[] drawn = Draw(beam.RenderSnapshot);
        int[] upperApex = LitColumns(drawn, 99);
        AssertTrue(upperApex.Length != 0 && upperApex[0] is >= 114 and <= 116 && upperApex[^1] == 255,
            "the line above the mouth lights from beside the mouth to the screen's right edge");
        AssertTrue(Enumerable.Range(0, MotherBrainBeamDefinitions.FirstLine).All(line => LitColumns(drawn, line).Length == 0),
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
            AssertEqual(frame + 1, beam.ColorIndex,
                "beam advances one cycle colour per update");
        }
        Step();
        AssertEqual(31, beam.Color, "negative terminator selects entry zero");
        AssertEqual(0, beam.ColorIndex, "reset frame does not increment the cycle colour");
        Step();
        AssertEqual(31, beam.Color, "entry zero repeats after wrap as in E7F5");
        MotherBrainRainbowBeamRenderSnapshot snapshot = beam.RenderSnapshot;
        AssertEqual(new MotherBrainRainbowBeamRenderSnapshot(true, beam.Color, beam.Geometry), snapshot,
            "render snapshot carries the active flag, color and geometry by value");
        var capture = GameplayEffectCapture.CaptureMotherBrainRainbowBeam(snapshot)
            ?? throw new InvalidOperationException("Verification requires capture.");
        AssertEqual(snapshot.Color, capture.Colour, "shared render packet adds the beam's native colour");
        AssertEqual(Cone(snapshot), capture.Window, "shared render packet includes the beam's cone");
        int cursor = beam.ColorIndex;
        _ = GameplayEffectCapture.CaptureMotherBrainRainbowBeam(beam.RenderSnapshot);
        AssertEqual(cursor, beam.ColorIndex, "repainting cannot advance beam simulation");
        beam.Step(false, new(0, 0), default, 0);
        AssertTrue(GameplayEffectCapture.CaptureMotherBrainRainbowBeam(beam.RenderSnapshot) is null,
            "deleted HDMA object emits no render layer");
        AssertEqual(snapshot.Geometry, beam.Geometry, "deactivation keeps the last geometry");
        Step();
        AssertEqual(MotherBrainBeamDefinitions.InitialColor, beam.Color, "new attack resets HDMA color");
        foreach (byte angle in new byte[] { 32, 96, 128, 160, 0, 224 })
        {
            beam.Step(true, new(100, 95), RotationAngle.FromTableIndex(angle), 512);
            bool up = angle is 96 or 128 or 160;
            MotherBrainRainbowBeamGeometry geometry = beam.Geometry ?? throw new InvalidOperationException("The beam resolved its geometry.");
            AssertEqual(ScreenWindow.Of(new WindowCone(Offset16<ScreenPixels>.Of(114, 100), 128 - geometry.EdgeB, 128 - geometry.EdgeA),
                MotherBrainBeamDefinitions.FirstLine), Cone(beam.RenderSnapshot), $"quadrant {angle} reflects its edges into the cone's");
            AssertEqual(0, LitColumns(Draw(beam.RenderSnapshot), up ? 101 : 98).Length,
                $"quadrant {angle} stays on its ray side of the mouth");
        }
        MotherBrainRainbowBeamGeometry? retained = beam.Geometry;
        beam.Step(true, new(100, 95), RotationAngle.ThreeQuarterTurn, 512);
        AssertEqual(retained, beam.Geometry, "native left-axis RTS preserves prior geometry");
        beam.Step(true, new(356, 95), RotationAngle.QuarterTurn, Width(64));
        AssertEqual(114, (beam.Geometry ?? throw new InvalidOperationException("Verification requires beam.Geometry.")).MouthScreenX, "DE20 uses low X byte, not wrapped enemy-header data");
        AssertEqual(ScreenWindow.Of(new WindowCone(Offset16<ScreenPixels>.Of(114, 100), 32, 96), MotherBrainBeamDefinitions.FirstLine),
            Cone(beam.RenderSnapshot), "low-byte mouth X places the cone like the unwrapped head");
        AssertThrows<InvalidDataException>(() => beam.Step(true, new(100, 27), RotationAngle.QuarterTurn, Width(64)),
            "a mouth above the work area fails during the simulation update");
        VerifyRetainedCones(beam.PresentationColors);
        Console.WriteLine("  Mother Brain beam: cones from the mouth, HUD exclusion, retained geometry, color stride/reset and owned capture agree.");
    }

    /// <summary>
    /// The geometry a left-straddling beam keeps is the cone drawn (#1468): in the same active run, and after the beam
    /// is deactivated and starts a new attack.
    /// </summary>
    private static void VerifyRetainedCones(MotherBrainRainbowPalettePresentation colors)
    {
        var beam = new MotherBrainRainbowBeamEffect { PresentationColors = colors };
        beam.Step(true, new(100, 95), RotationAngle.FromTableIndex(128), Width(32));
        ScreenWindow up = Cone(beam.RenderSnapshot);
        beam.Step(true, new(100, 95), RotationAngle.ThreeQuarterTurn, 512);
        AssertEqual(up, Cone(beam.RenderSnapshot), "retain keeps the previous cone");
        beam.Step(false, default, default, 0);
        beam.Step(true, new(40, 150), RotationAngle.ThreeQuarterTurn, 512);
        AssertEqual(up, Cone(beam.RenderSnapshot), "retain after reactivation still shows the cone from the earlier attack");
    }

    /// <summary>Native 8.8 angular-width word for a whole number of 256-unit-turn table units.</summary>
    private static ushort Width(int wholeTableUnits) => (ushort)EightEight.FromWhole(wholeTableUnits);

    /// <summary>The window an active beam is captured with.</summary>
    private static ScreenWindow Cone(MotherBrainRainbowBeamRenderSnapshot beam) =>
        (GameplayEffectCapture.CaptureMotherBrainRainbowBeam(beam)
            ?? throw new InvalidOperationException("An active beam captures its window.")).Window;

    /// <summary>The active beam over a black screen, in a fixed colour, as the display renderer draws it.</summary>
    private static RgbaPixel[] Draw(MotherBrainRainbowBeamRenderSnapshot beam) =>
        DisplayRendering.Render(new LayeredScene(FramePaint.FromNative(new RgbColor[SnesCgram.ColorCount], []),
            new DisplayLayer[] { new ColourAddWindowLayer(Cone(beam), new RgbColor(31, 31, 31)) }, DisplayBrightness.Full));

    /// <summary>The columns a drawn line lights.</summary>
    private static int[] LitColumns(RgbaPixel[] drawn, int line) =>
        Enumerable.Range(0, DisplayScreen.Width).Where(x => drawn[line * DisplayScreen.Width + x].R != 0).ToArray();

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
