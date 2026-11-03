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
    /// <summary>Checks authored body and bubble palette rows, then isolates visible body and color-window pixels.</summary>
    internal static VerifyCrystalFlashVisualFrameResult VerifyCrystalFlashVisualFrame(SuperMetroidRuntime runtime, SuperMetroid.AssetExtraction.Cartridge.ISnesAddressSpace bus, int elapsed)
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

        var pixels = DisplayRendering.Render(packet);
        // Samus's draws show her own shading (#1512), not the frame's colours: muting them to black isolates whether
        // her Crystal Flash body shows any pixel.
        FramePaint mutedMemory = packet.Paint.WithShownDraws(MuteSamus(packet.Paint.SpriteDraws));
        var mutedBody = DisplayRendering.Render(new LayeredScene(mutedMemory,
            packet.Layers, packet.Brightness));
        int centerX = samus.Position.X.Whole - runtime.Display.DisplayedGameplayScrolls.Layer1Position.X;
        int centerY = samus.Position.Y.Whole - runtime.Display.DisplayedGameplayScrolls.Layer1Position.Y;
        AssertTrue(centerX is >= 32 and <= 223 && centerY is >= 64 and <= 191,
            "visual fixture keeps the entire Crystal Flash sprite in the viewport");
        bool bodyVisible = Enumerable.Range(centerY - 32, 64)
            .Any(y => Enumerable.Range(centerX - 32, 64)
                .Any(x => pixels[y * 256 + x] != mutedBody[y * 256 + x]));
        bool windowVisible = false;
        if (runtime.BombProjectiles.PowerBombExplosion.Phase is PowerBombExplosionPhase.CrystalFlashExplosion or PowerBombExplosionPhase.CrystalFlashAfterglow)
        {
            // Remove only the captured color window, retaining identical OAM,
            // palette, terrain and camera to isolate its actual visible contribution.
            var withoutWindow = DisplayRendering.Render(packet.Without<ColourAddWindowLayer>());
            AssertTrue(pixels.AsSpan(0, 32 * 256).SequenceEqual(withoutWindow.AsSpan(0, 32 * 256)),
                "Crystal Flash window preserves HUD pixels");
            windowVisible = !pixels.SequenceEqual(withoutWindow);
        }
        return new VerifyCrystalFlashVisualFrameResult(bodyVisible, windowVisible);

        ushort Word(int address) => CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
    }

    /// <summary><paramref name="draws"/> with Samus's own draws, which carry her shading, shown in black.</summary>
    private static ImmutableArray<SpriteDraw> MuteSamus(ImmutableArray<SpriteDraw> draws) =>
        [.. draws.Select(draw => draw is ImageSpriteDraw { Shading: { } shading } image && SamusColourDefinitions.Sources.Contains(draw.Source.Kind)
            ? image with { Shading = shading.With(new ColourFlash(RgbColor.Black)) }
            : draw)];
}
