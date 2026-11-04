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
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Compares Crystal Flash palette timing and actual body/window contributions in a captured frame.</summary>
internal static class CrystalFlashVisualsVerification
{
    /// <summary>
    /// Checks authored body and bubble palette rows, then captures the scenes that isolate the visible body and
    /// color-window pixels: the frame, the frame with Samus muted and, while the color window shows, the frame without it.
    /// Published scenes share no storage with the runtime, so <see cref="Visibility"/> draws them after the route.
    /// </summary>
    internal static CrystalFlashFrameScenes VerifyCrystalFlashVisualFrame(SuperMetroidRuntime runtime, SuperMetroid.AssetExtraction.Cartridge.ISnesAddressSpace bus, int elapsed)
    {
        var samus = runtime.Player.IntroducedSamus;
        var packet = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        if (samus.CrystalFlash.SpecialPaletteKind == SamusSpecialPaletteType.CrystalFlash)
        {
            // Independent timeline lookup: expand authored durations into a cycle
            // and index elapsed calls, rather than reading the production timers.
            int cycleLength = Enumerable.Range(0, CrystalFlash.BodyRecordCount)
                .Sum(i => Word(SamusSourceDefinitions.CrystalFlashBodyRecords.Value + i * CrystalFlash.BodyRecordByteCount + 2));
            int remaining = elapsed % cycleLength, record = 0;
            while (remaining >= Word(SamusSourceDefinitions.CrystalFlashBodyRecords.Value + record * CrystalFlash.BodyRecordByteCount + 2))
                remaining -= Word(SamusSourceDefinitions.CrystalFlashBodyRecords.Value + record++ * CrystalFlash.BodyRecordByteCount + 2);
            int body = SamusSourceDefinitions.PaletteBank.Value | Word(SamusSourceDefinitions.CrystalFlashBodyRecords.Value + record * CrystalFlash.BodyRecordByteCount);
            int bubble = SamusSourceDefinitions.PaletteBank.Value | Word(SamusSourceDefinitions.BubblePointers.Value + elapsed / 5 % CrystalFlash.BubblePaletteCount * 2);
            // Samus names the body and bubble shades (#1512); the installation's shades hold the authored colours.
            RgbColor[] beams = runtime.Player.Colours.ShownBeams(InstalledFixtures.SamusShadeSources);
            for (int color = 0; color < CrystalFlash.BodyColorCount; color++)
                AssertEqual(Word(body + color * 2), beams[color],
                    $"runtime Crystal Flash body palette at frame {elapsed}");
            for (int color = 0; color < CrystalFlash.BubbleColorCount; color++)
                AssertEqual(Word(bubble + color * 2), beams[SamusPaletteDefinitionsCrystalFlashConstants.BubbleCgramStart - SamusPaletteDefinitionsCrystalFlashConstants.BodyCgramStart + color],
                    $"runtime Crystal Flash bubble palette at frame {elapsed}");
        }

        // Samus's draws show her own shading (#1512), not the frame's colours: muting them to black isolates whether
        // her Crystal Flash body shows any pixel.
        FramePaint mutedMemory = packet.Paint.WithShownDraws(MuteSamus(packet.Paint.SpriteDraws));
        int centerX = samus.Position.X.Whole - runtime.Display.DisplayedGameplayScrolls.Layer1Position.X;
        int centerY = samus.Position.Y.Whole - runtime.Display.DisplayedGameplayScrolls.Layer1Position.Y;
        AssertTrue(centerX is >= 32 and <= 223 && centerY is >= 64 and <= 191,
            "visual fixture keeps the entire Crystal Flash sprite in the viewport");
        // Removing only the captured color window keeps identical OAM, palette, terrain and camera, isolating its actual
        // visible contribution.
        LayeredScene? withoutWindow = runtime.BombProjectiles.PowerBombExplosion.Phase is
            PowerBombExplosionPhase.CrystalFlashExplosion or PowerBombExplosionPhase.CrystalFlashAfterglow
            ? packet.Without<ColourAddWindowLayer>() : null;
        return new CrystalFlashFrameScenes(elapsed, packet, new LayeredScene(mutedMemory, packet.Layers, packet.Brightness),
            withoutWindow, centerX, centerY);

        ushort Word(int address) => CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
    }

    /// <summary>
    /// Draws every frame's captured scenes and decides whether its Crystal Flash body and color window show any pixel,
    /// in frame order. The frames are independent, so they are drawn on several threads at once, each with its own
    /// renderer; every frame is drawn and compared exactly as a frame-by-frame check would.
    /// </summary>
    internal static VerifyCrystalFlashVisualFrameResult[] Visibility(IReadOnlyList<CrystalFlashFrameScenes> frames)
    {
        ArgumentNullException.ThrowIfNull(frames);
        var results = new VerifyCrystalFlashVisualFrameResult[frames.Count];
        Parallel.For(0, frames.Count, index => results[index] = Visibility(frames[index]));
        return results;
    }

    /// <summary>Draws one frame's captured scenes and decides whether its body and color window show any pixel.</summary>
    private static VerifyCrystalFlashVisualFrameResult Visibility(CrystalFlashFrameScenes frame)
    {
        RgbaPixel[] pixels = DisplayRendering.Render(frame.Shown);
        RgbaPixel[] mutedBody = DisplayRendering.Render(frame.SamusMuted);
        int centerX = frame.CenterX, centerY = frame.CenterY;
        bool bodyVisible = Enumerable.Range(centerY - 32, 64)
            .Any(y => Enumerable.Range(centerX - 32, 64)
                .Any(x => pixels[y * 256 + x] != mutedBody[y * 256 + x]));
        bool windowVisible = false;
        if (frame.WithoutWindow is { } scene)
        {
            RgbaPixel[] withoutWindow = DisplayRendering.Render(scene);
            AssertTrue(pixels.AsSpan(0, 32 * 256).SequenceEqual(withoutWindow.AsSpan(0, 32 * 256)),
                $"Crystal Flash window preserves HUD pixels at frame {frame.Elapsed}");
            windowVisible = !pixels.SequenceEqual(withoutWindow);
        }
        return new VerifyCrystalFlashVisualFrameResult(bodyVisible, windowVisible);
    }

    /// <summary><paramref name="draws"/> with Samus's own draws, which carry her shading, shown in black.</summary>
    private static ImmutableArray<SpriteDraw> MuteSamus(ImmutableArray<SpriteDraw> draws) =>
        [.. draws.Select(draw => draw is ImageSpriteDraw { Shading: { } shading } image && SamusColourDefinitions.Sources.Contains(draw.Source.Kind)
            ? image with { Shading = shading.With(new ColourFlash(RgbColor.Black)) }
            : draw)];
}
