using System.Collections.Immutable;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Core.Samus.Palettes.SamusPaletteDefinitions;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Samus.Artwork;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Compares Crystal Flash palette timing and actual body/window contributions in a captured frame.</summary>
internal static class CrystalFlashVisualsVerification
{
    /// <summary>
    /// Checks authored body and bubble palette rows, then captures the scenes that isolate the visible body and
    /// color-window pixels: the frame, the frame with Samus muted and, while the color window shows, the frame without it.
    /// The caller keeps them and draws every frame's with <see cref="Visibility"/> once its route has run.
    /// </summary>
    internal static CrystalFlashFrameScenes VerifyCrystalFlashVisualFrame(SuperMetroidRuntime runtime, SuperMetroid.AssetExtraction.Cartridge.ISnesAddressSpace bus, int elapsed)
    {
        var samus = runtime.Player.IntroducedSamus;
        var packet = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        if (samus.SpecialColouring.Holds(SamusSpecialPaletteType.CrystalFlash))
        {
            // Independent timeline lookup: expand authored durations into a cycle
            // and index elapsed calls, rather than reading the production timers.
            int cycleLength = Enumerable.Range(0, CrystalFlash.BodyStepCount)
                .Sum(i => Word(SamusSourceDefinitions.CrystalFlashBodyRecords.Value + i * CrystalFlash.BodyRecordByteCount + 2));
            int remaining = elapsed % cycleLength, record = 0;
            while (remaining >= Word(SamusSourceDefinitions.CrystalFlashBodyRecords.Value + record * CrystalFlash.BodyRecordByteCount + 2))
                remaining -= Word(SamusSourceDefinitions.CrystalFlashBodyRecords.Value + record++ * CrystalFlash.BodyRecordByteCount + 2);
            int body = SamusSourceDefinitions.PaletteBank.Value | Word(SamusSourceDefinitions.CrystalFlashBodyRecords.Value + record * CrystalFlash.BodyRecordByteCount);
            int bubble = SamusSourceDefinitions.PaletteBank.Value | Word(SamusSourceDefinitions.BubblePointers.Value + elapsed / 5 % CrystalFlash.BubblePaletteCount * 2);
            // Samus names the body and bubble shades (#1512); the installation's shades hold the authored colours.
            RgbColor[] beams = runtime.Player.Colours.ShownBeams(InstalledFixtures.SamusShadeSources);
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                AssertEqual(Word(body + color * 2), beams[color],
                    $"runtime Crystal Flash body palette at frame {elapsed}");
            for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                AssertEqual(Word(bubble + color * 2), beams[SamusPaletteDefinitionsCrystalFlashConstants.BubbleCgramStart - SamusPaletteDefinitionsCrystalFlashConstants.BodyCgramStart + color],
                    $"runtime Crystal Flash bubble palette at frame {elapsed}");
        }

        // Her Crystal Flash body is drawn in her beam inks, the colour row the native check clears (#1457); muting all of
        // her would count the frames she shows in her suit's inks after the body ends. Samus's draws show her own shading (#1512), so laying two
        // different single colours over her beam inks isolates whether any pixel of the body shows: only a beam-ink pixel
        // that shows differs between the two.
        RgbColor dark = InstalledFixtures.SamusShadeSources.Death.WhiteoutColor(DarkBeamsStep);
        RgbColor light = InstalledFixtures.SamusShadeSources.Death.WhiteoutColor(LightBeamsStep);
        AssertTrue(dark != light, "the two whiteout shades that isolate the Crystal Flash body show different colours");
        FramePaint beamsDark = packet.Paint.WithShownDraws(ShadeSamus(packet.Paint.SpriteDraws, SamusShades.DeathWhiteout(DarkBeamsStep)));
        FramePaint beamsLight = packet.Paint.WithShownDraws(ShadeSamus(packet.Paint.SpriteDraws, SamusShades.DeathWhiteout(LightBeamsStep)));
        int centerX = samus.Position.X.Whole - runtime.Display.DisplayedGameplayScrolls.ForegroundPosition.X;
        int centerY = samus.Position.Y.Whole - runtime.Display.DisplayedGameplayScrolls.ForegroundPosition.Y;
        AssertTrue(centerX is >= 32 and <= 223 && centerY is >= 64 and <= 191,
            "visual fixture keeps the entire Crystal Flash sprite in the viewport");
        // Removing only the captured color window keeps identical OAM, palette, terrain and camera, isolating its actual
        // visible contribution.
        LayeredScene? withoutWindow = runtime.BombProjectiles.PowerBombExplosion.Phase is
            PowerBombExplosionPhase.CrystalFlashExplosion or PowerBombExplosionPhase.CrystalFlashAfterglow
            ? packet.Without<ColourAddWindowLayer>() : null;
        return new CrystalFlashFrameScenes(elapsed, packet, new LayeredScene(beamsDark, packet.Layers, packet.Brightness),
            new LayeredScene(beamsLight, packet.Layers, packet.Brightness), withoutWindow, centerX, centerY);

        ushort Word(int address) => CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
    }

    /// <summary>
    /// Draws every frame's captured scenes and decides, in frame order, whether each frame's body and color window show
    /// any pixel, checking that each frame's window preserves its HUD pixels. The frames are independent, so they are
    /// drawn at once, each frame on one companion thread with its own renderer (<see cref="ConcurrentSceneDrawing"/>);
    /// every frame is drawn and compared exactly as when each was drawn before the runtime advanced, and the checks are
    /// made in frame order with the same failures.
    /// </summary>
    internal static VerifyCrystalFlashVisualFrameResult[] Visibility(IReadOnlyList<CrystalFlashFrameScenes> frames)
    {
        ArgumentNullException.ThrowIfNull(frames);
        CrystalFlashDrawnFrame[] drawn = ConcurrentSceneDrawing.DrawEach(frames, Draw);
        var visibility = new VerifyCrystalFlashVisualFrameResult[frames.Count];
        for (int index = 0; index < frames.Count; index++)
        {
            AssertTrue(drawn[index].HudPreserved, $"Crystal Flash window preserves HUD pixels at frame {frames[index].Elapsed}");
            visibility[index] = drawn[index].Visibility;
        }
        return visibility;
    }

    /// <summary>Draws one frame's captured scenes and finds whether its body and color window show any pixel.</summary>
    private static CrystalFlashDrawnFrame Draw(CrystalFlashFrameScenes frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        // Two scenes that describe the same frame show the same pixels, so they are drawn only when their descriptions
        // differ: a frame whose Samus draws carry no shading her beam inks take shows no body without drawing either.
        bool bodyVisible = !ScenePixels.SameDescription(new DisplayPacket(0, frame.BeamsDark), new DisplayPacket(0, frame.BeamsLight))
            && BodyRegionDiffers(frame, DisplayRendering.Render(frame.BeamsDark), DisplayRendering.Render(frame.BeamsLight));
        bool windowVisible = false, hudPreserved = true;
        // The shown frame is drawn only to compare it with the frame without the color window, while one shows and the
        // two describe different frames.
        if (frame.WithoutWindow is { } withoutWindow &&
            !ScenePixels.SameDescription(new DisplayPacket(0, frame.Shown), new DisplayPacket(0, withoutWindow)))
        {
            RgbaPixel[] shown = DisplayRendering.Render(frame.Shown), withoutWindowPixels = DisplayRendering.Render(withoutWindow);
            hudPreserved = shown.AsSpan(0, 32 * 256).SequenceEqual(withoutWindowPixels.AsSpan(0, 32 * 256));
            windowVisible = !shown.SequenceEqual(withoutWindowPixels);
        }
        return new CrystalFlashDrawnFrame(new VerifyCrystalFlashVisualFrameResult(bodyVisible, windowVisible), hudPreserved);
    }

    /// <summary>
    /// Whether any pixel of the region around Samus differs between <paramref name="beamsDark"/> and
    /// <paramref name="beamsLight"/>, the frame drawn with her beam inks in the two whiteout shades.
    /// </summary>
    private static bool BodyRegionDiffers(CrystalFlashFrameScenes frame, RgbaPixel[] beamsDark, RgbaPixel[] beamsLight)
    {
        int centerX = frame.CenterX, centerY = frame.CenterY;
        return Enumerable.Range(centerY - 32, 64)
            .Any(y => Enumerable.Range(centerX - 32, 64)
                .Any(x => beamsDark[y * 256 + x] != beamsLight[y * 256 + x]));
    }

    /// <summary>The whiteout ramp's first step, whose single colour the dark scene lays over her beam inks.</summary>
    private const int DarkBeamsStep = 0;

    /// <summary>The whiteout ramp's last step, whose single colour the light scene lays over her beam inks.</summary>
    private const int LightBeamsStep = SamusPaletteDefinitions.Death.WhiteoutShadeCount - 1;

    /// <summary>
    /// <paramref name="draws"/> with <paramref name="beams"/>, a shade colouring all her beam inks in one colour, laid over
    /// Samus's own draws, which carry her shading.
    /// </summary>
    private static ImmutableArray<SpriteDraw> ShadeSamus(ImmutableArray<SpriteDraw> draws, PictureShade beams) =>
        [.. draws.Select(draw => draw is ImageSpriteDraw { Shading: { } shading } image && SamusColourDefinitions.Sources.Contains(draw.Source.Kind)
            ? image with { Shading = shading.With(beams) }
            : draw)];
}
