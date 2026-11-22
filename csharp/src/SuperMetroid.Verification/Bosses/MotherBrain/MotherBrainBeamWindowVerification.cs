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
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Checks Mother Brain's rainbow beam cone, colour cycling, and captured render data.</summary>
internal static class MotherBrainBeamWindowVerification
{
    /// <summary>
    /// Checks the cone the beam publishes (#1626): its apex at the mouth in the room, its two edges either side of its
    /// aim, placed against the frame's camera below the HUD and drawn whichever way it points; and its colour cycle.
    /// </summary>
    internal static void VerifyMotherBrainBeamWindow()
    {
        var paletteDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            RepositoryInstallation.Installation.MapDirectory, MotherBrainBeamColourFile.FileName)));
        var cycle = new JsonArray();
        for (int frame = 0; frame < MotherBrainRainbowPaletteGeometry.BeamCycleColorCount; frame++)
            cycle.Add(frame % 2 == 0 ? "#ff0000" : "#00ff00");
        paletteDocument["cycle"] = cycle;
        MotherBrainRainbowBeamColours installedColours = MotherBrainBeamColourFile.Read(new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(paletteDocument.ToJsonString())));
        var beam = new MotherBrainRainbowBeamEffect();
        AssertTrue(beam.RenderSnapshot is null, "the beam shows nothing before it first aims");
        void Step() => beam.Show(new(100, 95), RotationAngle.QuarterTurn, Width(64), installedColours);
        Step();
        AssertEqual(installedColours.BeamInitialColor, beam.Color, "the beam's first update shows its initial colour");
        AssertEqual(new MotherBrainRainbowBeamGeometry(new(114, 100), new TrigonometryAngle(32), new TrigonometryAngle(96)),
            beam.Geometry, "the beam publishes the mouth's room position and both wrapped edges");
        // The beam's edges are measured from straight down: 32 and 96 reflect to the screen's 96 and 32, a cone from
        // up-right to down-right of the mouth, below the HUD (#1468).
        AssertEqual(ScreenWindow.Of(new WindowCone(Offset16<ScreenPixels>.Of(114, 100), new TrigonometryAngle(32), new TrigonometryAngle(96)), ScreenGeometry.GameplayHudHeightPixels),
            Cone(beam), "right beam is a cone from the mouth between its reflected edges");
        RgbaPixel[] drawn = Draw(Cone(beam));
        int[] upperApex = LitColumns(drawn, 99);
        AssertTrue(upperApex.Length != 0 && upperApex[0] is >= 114 and <= 116 && upperApex[^1] == 255,
            "the line above the mouth lights from beside the mouth to the screen's right edge");
        AssertTrue(Enumerable.Range(0, ScreenGeometry.GameplayHudHeightPixels).All(line => LitColumns(drawn, line).Length == 0),
            "rainbow beam misses the HUD");
        Step();
        AssertEqual(31, beam.Color, "first cycling frame reads entry zero");
        Step();
        AssertEqual(992, beam.Color, "color cursor advances to the second installed sample");
        for (int frame = 2; frame < MotherBrainRainbowPaletteGeometry.BeamCycleColorCount; frame++)
        {
            Step();
            AssertEqual((ushort)(frame % 2 == 0 ? 31 : 992), beam.Color,
                $"beam installed cycle color {frame}, one cycle colour per update");
        }
        Step();
        AssertEqual(31, beam.Color, "past the cycle's end the beam starts over at colour zero");
        Step();
        AssertEqual(992, beam.Color, "colour zero does not repeat: the next update shows colour one (#2251)");
        // The frame boundary latches the beam's colour with the rest of the frame (#2498).
        beam.LatchDisplay();
        MotherBrainRainbowBeamRenderSnapshot snapshot = beam.RenderSnapshot
            ?? throw new InvalidOperationException("The shown beam publishes its cone.");
        AssertEqual(new MotherBrainRainbowBeamRenderSnapshot(beam.Color, new(new(114, 100), new TrigonometryAngle(32), new TrigonometryAngle(96))), snapshot,
            "the shown beam publishes its latched colour and cone by value");
        ColourAddWindowLayer capture = GameplayEffectCapture.CaptureMotherBrainRainbowBeam(snapshot, default);
        AssertEqual(snapshot.Color, capture.Colour, "the captured layer adds the beam's colour");
        _ = GameplayEffectCapture.CaptureMotherBrainRainbowBeam(snapshot, default);
        AssertEqual(snapshot.Color, beam.Color, "repainting cannot advance beam simulation");
        Step();
        AssertEqual(31, beam.Color, "the update after the repaints shows the cycle's next colour");
        AssertEqual(snapshot.Color, (beam.RenderSnapshot ?? throw new InvalidOperationException("The shown beam publishes its cone.")).Color,
            "the update's colour reaches the screen only at the next frame boundary");
        beam.LatchDisplay();
        AssertEqual(beam.Color, (beam.RenderSnapshot ?? throw new InvalidOperationException("The shown beam publishes its cone.")).Color,
            "the next frame boundary shows the update's colour");
        beam.Hide();
        AssertTrue(beam.RenderSnapshot is null, "a hidden beam shows nothing");
        Step();
        AssertEqual(installedColours.BeamInitialColor, beam.Color, "a new attack restarts the colour cycle");
        foreach (byte angle in new byte[] { 32, 96, 128, 160, 0, 224 })
        {
            beam.Show(new(100, 95), RotationAngle.FromSteps(angle), Width(2), installedColours);
            bool up = angle is 96 or 128 or 160;
            MotherBrainRainbowBeamGeometry geometry = beam.Geometry ?? throw new InvalidOperationException("The beam aimed.");
            AssertEqual(ScreenWindow.Of(new WindowCone(Offset16<ScreenPixels>.Of(114, 100), TrigonometryAngle.HalfTurn - geometry.UpperEdge, TrigonometryAngle.HalfTurn - geometry.LowerEdge),
                ScreenGeometry.GameplayHudHeightPixels), Cone(beam), $"quadrant {angle} reflects its edges into the cone's");
            AssertEqual(0, LitColumns(Draw(Cone(beam)), up ? 101 : 98).Length,
                $"quadrant {angle} stays on its ray side of the mouth");
        }
        VerifyLeftwardCone(beam, installedColours);
        beam.Show(new(356, 95), RotationAngle.QuarterTurn, Width(64), installedColours);
        AssertEqual(new Position16<RoomPixels>(370, 100), beam.Geometry?.Mouth, "the mouth stays where it lies in the room");
        AssertEqual(ScreenWindow.Of(new WindowCone(Offset16<ScreenPixels>.Of(114, 100), new TrigonometryAngle(32), new TrigonometryAngle(96)), ScreenGeometry.GameplayHudHeightPixels),
            Cone(beam, new(256, 0)), "a scrolled camera places the cone like the unscrolled head");
        Console.WriteLine("  Mother Brain beam: cones from the mouth in any direction, HUD exclusion, camera placement, color stride/reset and owned capture agree.");
    }

    /// <summary>
    /// A beam aimed across the leftward axis draws its own cone (#1626), never the cone an earlier aim drew: its edges
    /// reflect into the screen's like any other aim and it lights the mouth's line toward the screen's left edge.
    /// </summary>
    private static void VerifyLeftwardCone(MotherBrainRainbowBeamEffect beam, MotherBrainRainbowBeamColours colours)
    {
        beam.Show(new(100, 95), RotationAngle.FromSteps(128), Width(32), colours);
        ScreenWindow up = Cone(beam);
        beam.Show(new(100, 95), RotationAngle.ThreeQuarterTurn, Width(2), colours);
        AssertEqual(new MotherBrainRainbowBeamGeometry(new(114, 100), new TrigonometryAngle(191), new TrigonometryAngle(193)), beam.Geometry,
            "a leftward aim publishes its own edges");
        AssertTrue(Cone(beam) != up, "a leftward aim does not keep the earlier cone");
        AssertEqual(ScreenWindow.Of(new WindowCone(Offset16<ScreenPixels>.Of(114, 100), TrigonometryAngle.Wrapped(128 - 193), TrigonometryAngle.Wrapped(128 - 191)), ScreenGeometry.GameplayHudHeightPixels),
            Cone(beam), "a leftward aim reflects its edges into the cone's");
        int[] mouthLine = LitColumns(Draw(Cone(beam)), 100);
        AssertTrue(mouthLine.Length != 0 && mouthLine[0] == 0 && mouthLine[^1] <= 116,
            "a leftward cone lights the mouth's line from the screen's left edge to the mouth");
    }

    /// <summary>A whole number of 256-unit-turn angle units of the beam's width.</summary>
    private static RotationAngle Width(int steps) => RotationAngle.FromSteps(steps);

    /// <summary>The window the shown beam is captured with, against <paramref name="camera"/>.</summary>
    private static ScreenWindow Cone(MotherBrainRainbowBeamEffect beam, Position16<RoomPixels> camera = default) =>
        GameplayEffectCapture.CaptureMotherBrainRainbowBeam(beam.RenderSnapshot
            ?? throw new InvalidOperationException("The shown beam publishes its cone."), camera).Window;

    /// <summary>A window over a black screen, in a fixed colour, as the display renderer draws it.</summary>
    private static RgbaPixel[] Draw(ScreenWindow window) =>
        DisplayRendering.Render(new LayeredScene(FramePaint.FromNative(new RgbColor[SnesCgram.ColorCount], []),
            new DisplayLayer[] { new ColourAddWindowLayer(window, new RgbColor(255, 255, 255)) }, DisplayBrightness.Full));

    /// <summary>The columns a drawn line lights.</summary>
    private static int[] LitColumns(RgbaPixel[] drawn, int line) =>
        Enumerable.Range(0, ScreenGeometry.ScreenWidthPixels).Where(x => drawn[line * ScreenGeometry.ScreenWidthPixels + x].R != 0).ToArray();

    /// <summary>Runs the focused Mother Brain beam window and color check.</summary>
    [VerificationCommand("--mother-brain-beam-fixture")]
    internal static int MotherBrainBeamFixtureCommand()
    {
     VerifyMotherBrainBeamWindow(); return 0;
    }
}
