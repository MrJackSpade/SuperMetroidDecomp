using System.Collections.Immutable;
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
            for (int color = 0; color < CrystalFlash.BodyColorCount; color++)
                AssertEqual(Word(body + color * 2), packet.Paint.Colors[SamusPaletteDefinitionsCrystalFlashConstants.BodyCgramStart + color],
                    $"runtime Crystal Flash body palette at frame {elapsed}");
            for (int color = 0; color < CrystalFlash.BubbleColorCount; color++)
                AssertEqual(Word(bubble + color * 2), packet.Paint.Colors[SamusPaletteDefinitionsCrystalFlashConstants.BubbleCgramStart + color],
                    $"runtime Crystal Flash bubble palette at frame {elapsed}");
        }

        var pixels = DisplayRendering.Render(packet);
        var colors = packet.Paint.Colors.ToArray();
        Array.Clear(colors, SamusPaletteDefinitionsCrystalFlashConstants.BodyCgramStart, Common.ColoursPerRow);
        // Samus's draws show her own colours, resolved when the frame is published (#1405), not the frame's: the body's
        // row is muted where she holds it, her beam ink row, as the native check mutes its colour row (#1457).
        var mutedMemory = new FramePaint(colors, MuteBeamInkRow(packet.Paint.SpriteDraws, runtime.Player.Colours));
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
            var withoutWindow = DisplayRendering.Render(packet.Without<ScanlineColorAddRenderLayer>());
            AssertTrue(pixels.AsSpan(0, 32 * 256).SequenceEqual(withoutWindow.AsSpan(0, 32 * 256)),
                "Crystal Flash window preserves HUD pixels");
            windowVisible = !pixels.SequenceEqual(withoutWindow);
        }
        return new VerifyCrystalFlashVisualFrameResult(bodyVisible, windowVisible);

        ushort Word(int address) => CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
    }

    /// <summary><paramref name="draws"/> with Samus's own draws resolved from <paramref name="samus"/>'s colours but her beam ink row blank.</summary>
    private static ImmutableArray<SpriteDraw> MuteBeamInkRow(ImmutableArray<SpriteDraw> draws, SamusColours samus)
    {
        var muted = new MutedBeamInkRow(samus);
        return [.. draws.Select(draw => draw is ImageSpriteDraw { Colours: not null } image && SamusColourDefinitions.Sources.Contains(draw.Source.Kind)
            ? image with { Colours = DrawColours.Owned(image with { Colours = null }, muted) }
            : draw)];
    }

    /// <summary>Samus's colours with her beam ink row, which the Crystal Flash's body and bubble show, blank.</summary>
    private sealed class MutedBeamInkRow(SamusColours samus) : IInkRowColours
    {
        /// <inheritdoc />
        public void Overlay(int inkRow, Span<RgbColor> colours)
        {
            samus.Overlay(inkRow, colours);
            if (inkRow == SamusColourDefinitions.BeamInkRow)
                colours.Clear();
        }
    }
}
