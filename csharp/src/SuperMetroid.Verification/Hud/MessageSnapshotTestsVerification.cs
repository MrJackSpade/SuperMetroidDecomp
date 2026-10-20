using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Hud;

/// <summary>Checks gameplay message snapshots, reveal geometry and frame ownership.</summary>
internal static class MessageSnapshotTestsVerification
{
    /// <summary>Compares every retail message's changing overlay with captured packets and checks retained and malformed packets.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native PPU geometry, native VRAM words")]
    internal static void VerifyMessageSnapshots()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var vram = new SnesVram();
        byte[] graphics = new byte[SnesPpuLayout.VramByteCount];
        new Random(32107).NextBytes(graphics);
        vram.LoadBytes(0, graphics);
        var cgram = new SnesCgram();
        for (int i = 0; i < SnesCgram.ColorCount; i++) cgram.SetColor(i, Bgr555.FromWord((ushort)(i * 101 & 32767)));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = PpuMemorySnapshotConstants.CaptureFrontend(vram, cgram, oam, 3);
        Rgba32[] baseline = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        int samples = 0;
        foreach (GameplayMessageId id in Enum.GetValues<GameplayMessageId>())
        {
            if (id == GameplayMessageId.None) continue;
            var state = CreateGameplayMessageFixture();
            AssertTrue(GameplayMessageBoxRendererConstants.Capture(state, vram) is null, "inactive message has no overlay");
            state.Begin(id, 0);
            int previousRadius = -1;
            bool previousYes = true;
            bool toggledSave = false, releasedSave = false;
            RenderFrameSnapshot? held = null;
            Rgba32[]? heldPixels = null;
            for (int tick = 0; tick < 600 && state.IsActive; tick++)
            {
                if (previousRadius != state.RadiusPixels || previousYes != state.ConfirmationSelectionYes)
                {
                    previousRadius = state.RadiusPixels;
                    previousYes = state.ConfirmationSelectionYes;
                    Rgba32[] expected = baseline.ToArray();
                    GameplayMessageBoxRendererConstants.Composite(expected, state, vram, cgram);
                    MessageBoxRenderLayer? layer = GameplayMessageBoxRendererConstants.Capture(state, vram);
                    RenderLayer[] layers = layer is null ? [] : [layer];
                    var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick),
                        new LayeredRenderSnapshot(memory, layers, 3, 15));
                    AssertTrue(expected.SequenceEqual(RenderForComparison(packet)),
                        $"message {id}, radius {state.RadiusPixels}");
                    if (layer is not null)
                    {
                        AssertTrue(layer.Text.Cells.SequenceEqual(state.Tilemap.SelectToArray(word => BgCell.FromTilemapWord(new SnesBgTilemapWord(word)))),
                            "captured message cells match the source words");
                        if (held is null) { held = packet; heldPixels = expected; }
                    }
                }
                SnesButton input = state.Phase == GameplayMessageBoxPhase.AwaitingInput
                    ? tick % 3 == 0 ? SnesButton.Right : tick % 3 == 2 ? SnesButton.A : 0
                    : 0;
                if (id == GameplayMessageIds.SaveConfirmation && state.Phase == GameplayMessageBoxPhase.AwaitingInput)
                {
                    if (!toggledSave) { input = SnesButton.Right; toggledSave = true; }
                    else if (!releasedSave) { input = 0; releasedSave = true; }
                    else input = SnesButton.A;
                }
                state.Step(input);
            }
            AssertTrue(!state.IsActive, $"message {id} completes reveal/close fixture");
            // Reuse the same live owner after closing, replacing its tilemap and radius.
            state.Begin(GameplayMessageIds.SaveCompleted, 0);
            AssertTrue(held is not null && heldPixels.SequenceEqual(RenderForComparison(held)),
                "retained message survives closing and reuse of the source owner");
        }

        // Independent geometric oracle: solid color-one glyphs must paint exactly the
        // intersection of the art's centered extent and the half-open reveal window.
        vram = new SnesVram();
        for (int y = 0; y < 8; y++) vram.LoadBytes(GameplayVramLayout.HudCharacterWord * 2 + y * 2, [255, 0]);
        memory = PpuMemorySnapshotConstants.CaptureFrontend(vram, cgram, oam, 3);
        for (int rows = 3; rows <= 6; rows++)
        for (int radius = 0; radius <= 24; radius++)
        {
            ushort[] tiles = Enumerable.Repeat((ushort)(6 << 10), rows * 32).ToArray();
            var layer = GameplayMessageBoxRendererConstants.Layer(tiles, radius, vram.CaptureImage());
            Array.Clear(tiles);
            var packet = new RenderFrameSnapshot(new(++samples, 1, 0), new LayeredRenderSnapshot(memory, [layer], 3, 15));
            Rgba32[] actual = SoftwareFrameSnapshotRenderer.Render(packet);
            int top = 124 - Math.Min(radius, rows * 4), bottom = 124 + Math.Min(radius, rows * 4);
            Rgba32 ink = GameplayMessageRomData.Palette.TemporaryLightColor.ToRgba32();
            for (int y = 0; y < 224; y++)
                AssertTrue(actual.AsSpan(y * 256, 256).All(p => p == (y >= top && y < bottom ? ink : baseline[y * 256])),
                    $"message geometry {rows} rows, radius {radius}, line {y}");
            if (rows == 3 && radius == 0)
            {
            }
        }
        AssertThrows<ArgumentException>(() => GameplayMessageBoxRendererConstants.Layer(new ushort[95], 1, vram.CaptureImage()), "partial message row rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => GameplayMessageBoxRendererConstants.Layer(new ushort[96], 25, vram.CaptureImage()), "invalid reveal radius rejected");
        Console.WriteLine($"  Message snapshots: {samples} retail/glyph-window comparisons and ownership checks agree.");
    }
}
