using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.RenderVerification.Harness;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.RenderVerification.Frontend;

/// <summary>
/// Renders the cinematics' colour effects (#1733) on this device: a few steps of the Ceres flight's station lights, the
/// destruction's floor lights, the PLANET ZEBES caption's fades and the old Mother Brain fight's lights each match the
/// reference renderer, and a sprite effect's record changes pixels inside its actors' boxes only, at least one sampled step
/// showing it unlike the actor's drawn colours.
/// </summary>
internal static class CinematicColourEffectTests
{
    /// <summary>The updates into the flight's rear view sampled, after its white flash.</summary>
    private static readonly int[] FlightSamples = [64, 70, 96];
    /// <summary>The updates into the destruction's floor lights sampled.</summary>
    private static readonly int[] FloorSamples = [60, 66];
    /// <summary>The updates into the caption's fade in sampled: its first, fourth and last records.</summary>
    private static readonly int[] FadeInSamples = [0, 9, 21];
    /// <summary>The updates into the caption's fade out sampled: its second record and its black last one, held.</summary>
    private static readonly int[] FadeOutSamples = [3, 30];
    /// <summary>The updates into the fight lights sampled.</summary>
    private static readonly int[] FightSamples = [0, 6, 42];

    /// <summary>Runs each scene, rendering each sampled step on <paramref name="renderer"/>.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        int checks = Flight(device, renderer) + Destruction(device, renderer) + Fight(device, renderer);
        int expected = FlightSamples.Length + FloorSamples.Length + FadeInSamples.Length + FadeOutSamples.Length + FightSamples.Length;
        if (checks != expected)
            throw new InvalidOperationException($"{device.Kind}: the cinematics rendered {checks} colour effect steps, not the {expected} sampled.");
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {checks} cinematic colour effect steps match the WARP reference device, " +
            "the station's lights and the caption's fades recolouring only their actors.");
    }

    /// <summary>The flight to Ceres: its station's lights from the rear view on.</summary>
    private static int Flight(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var audio = AudioQueues.Standalone();
        var flight = new IntroCeresFlightState(audio, InstalledFixtures.IntroArtwork.BoundCatalog.CeresFlight, sheets: InstalledFixtures.SpriteSheets);
        PictureShade station = CeresShades.Drawn(CeresSpriteColouring.Station);
        int? start = null;
        int checks = 0, showing = 0;
        for (int update = 0; !flight.Finished; update++)
        {
            if (update >= 6_000)
                throw new InvalidOperationException("The flight to Ceres did not end.");
            bool rearView = flight.ShowsRearView;
            flight.Step();
            audio.AdvanceFrame(default);
            if (!rearView && flight.ShowsRearView)
                start = update;
            if (start is { } from && FlightSamples.Contains(update - from))
            {
                LayeredScene scene = flight.CaptureRenderSnapshot();
                Verify(device, renderer, update, scene, flight.Render(), $"flight station lights, {update - from} updates in");
                showing += Isolated(device, renderer, update, scene, station, $"flight station lights, {update - from} updates in");
                checks++;
            }
        }
        if (showing == 0)
            throw new InvalidOperationException($"{device.Kind}: no sampled step of the station's lights changed the station's pixels.");
        return checks;
    }

    /// <summary>The Ceres destruction: its floor's lights from the station's setup, then the caption's fades.</summary>
    private static int Destruction(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        CeresDestructionCinematicState scene = RenderRetailFixture.CreateDestruction();
        PictureShade caption = CeresShades.Drawn(CeresSpriteColouring.ZebesTitle);
        int? setUp = null, fadeIn = null, fadeOut = null;
        int checks = 0, showing = 0;
        ColourEffectFrame? previous = null;
        for (int update = 0; !scene.Finished; update++)
        {
            if (update >= 6_000)
                throw new InvalidOperationException("The Ceres destruction did not end.");
            bool wasSetUp = scene.FloorShot is not null;
            scene.Step();
            if (!wasSetUp && scene.FloorShot is not null)
                setUp = update;
            // A fade starts on the update its caption fade first shows a record of its own.
            ColourEffectFrame? shown = scene.ColourEffects.FrameOf(ColourEffectColours.PlanetZebesText);
            if (shown is { } frame && frame.Animation != previous?.Animation)
            {
                if (frame.Animation == ColourEffectAnimation.FadeInPlanetZebesText)
                    fadeIn = update;
                else
                    fadeOut = update;
            }
            previous = shown;
            if (setUp is { } from && !scene.ZebesRevealed && FloorSamples.Contains(update - from))
            {
                Verify(device, renderer, update, scene.CaptureRenderSnapshot(), scene.Render(), $"destruction floor lights, {update - from} updates in");
                checks++;
            }
            if (fadeIn is { } inFrom && fadeOut is null && FadeInSamples.Contains(update - inFrom))
                checks += Caption(update - inFrom, "fade in");
            if (fadeOut is { } outFrom && FadeOutSamples.Contains(update - outFrom))
                checks += Caption(update - outFrom, "fade out");

            int Caption(int into, string fade)
            {
                LayeredScene captured = scene.CaptureRenderSnapshot();
                Verify(device, renderer, update, captured, scene.Render(), $"PLANET ZEBES {fade}, {into} updates in");
                showing += Isolated(device, renderer, update, captured, caption, $"PLANET ZEBES {fade}, {into} updates in");
                return 1;
            }
        }
        if (showing == 0)
            throw new InvalidOperationException($"{device.Kind}: no sampled step of the caption's fades changed the caption's pixels.");
        return checks;
    }

    /// <summary>The intro to page two: the old Mother Brain fight's lights from the end of the crossfade into the fight.</summary>
    private static int Fight(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var audio = AudioQueues.Standalone();
        IntroCinematicState intro = RenderRetailFixture.CreateIntro(audio);
        int? start = null;
        int checks = 0;
        for (int update = 0; intro.CurrentFlashbackMotherBrain?.PageTwoRequested != true; update++)
        {
            if (update >= 20_000)
                throw new InvalidOperationException("The intro did not reach page two.");
            IntroCinematicPhase before = intro.Phase;
            intro.Step(update % 47 == 0 ? ControllerButton.A : ControllerButton.None);
            audio.AdvanceFrame(default);
            if (before == IntroCinematicPhase.MotherBrainCrossfade && intro.Phase == IntroCinematicPhase.MotherBrainFlashback)
                start = update;
            if (start is { } from && FightSamples.Contains(update - from))
            {
                Verify(device, renderer, update, intro.CaptureTranslatedRenderSnapshot(), intro.Render(), $"fight lights, {update - from} updates in");
                checks++;
            }
        }
        return checks;
    }

    /// <summary>Renders <paramref name="scene"/> and compares it with the reference renderer's <paramref name="expected"/>.</summary>
    private static void Verify(D3D11RenderDevice device, D3D11FrameRenderer renderer, int update, LayeredScene scene, RgbaPixel[] expected, string what)
    {
        var packet = new DisplayPacket(update, scene);
        PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: {what} (update {update})");
    }

    /// <summary>
    /// Renders <paramref name="scene"/> again with every draw in <paramref name="drawn"/> shown in its drawn colours alone,
    /// requiring every pixel the effect changed to lie inside those draws' boxes; returns one when the effect changed any.
    /// </summary>
    private static int Isolated(D3D11RenderDevice device, D3D11FrameRenderer renderer, int update, LayeredScene scene, PictureShade drawn, string what)
    {
        var boxes = new List<(int Left, int Top, int Right, int Bottom)>();
        SpriteDraw Unlit(SpriteDraw draw)
        {
            if (draw is not ImageSpriteDraw { Shading: { } shading } image || !shading.Shades.Contains(drawn))
                return draw;
            boxes.Add((image.Corner.X, image.Corner.Y, image.Corner.X + image.Image.Size.X, image.Corner.Y + image.Image.Size.Y));
            return image with { Shading = PictureShading.Shaded(drawn) };
        }
        DisplayLayer[] layers = [.. scene.Layers.ToArray().Select(layer => layer is SpritesLayer sprites
            ? new SpritesLayer([.. sprites.Draws.Select(Unlit)], sprites.Blend, sprites.Tint)
            : layer)];
        if (boxes.Count == 0)
            throw new InvalidOperationException($"{device.Kind}: {what} (update {update}) draws no actor the effect shows on.");
        RgbaPixel[] lit = renderer.RenderForReadback(new DisplayPacket(update, scene));
        var unlitPacket = new DisplayPacket(update, new LayeredScene(scene.Paint, layers, scene.Brightness));
        RgbaPixel[] unlit = renderer.RenderForReadback(unlitPacket);
        int differing = 0;
        for (int pixel = 0; pixel < lit.Length; pixel++)
        {
            if (lit[pixel] == unlit[pixel])
                continue;
            int x = pixel % unlitPacket.Size.X, y = pixel / unlitPacket.Size.X;
            if (!boxes.Any(box => x >= box.Left && x < box.Right && y >= box.Top && y < box.Bottom))
                throw new InvalidOperationException($"{device.Kind}: {what} (update {update}) changed pixel ({x},{y}), outside the actors it shows on.");
            differing++;
        }
        return differing > 0 ? 1 : 0;
    }
}
