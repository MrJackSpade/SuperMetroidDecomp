using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;

using SuperMetroid.Core.Rooms;
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
        // The message images are pictured over the installed HUD characters (#1392), which the oracle reads from VRAM.
        RetailPresentationFixture().HudTiles.LoadTo(vram);
        MessageBoxSheet messages = RepositoryInstallation.Installation.LoadRoomBlockSheets().MessageBoxes.Values.First();
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
            AssertTrue(GameplayMessageBoxRenderer.Capture(state, messages) is null, "inactive message has no overlay");
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
                    MessageBoxRenderLayer? layer = GameplayMessageBoxRenderer.Capture(state, messages);
                    RenderLayer[] layers = layer is null ? [] : [layer];
                    var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick),
                        new LayeredRenderSnapshot(memory, layers, 15));
                    AssertTrue(expected.SequenceEqual(RenderForComparison(packet)),
                        $"message {id}, radius {state.RadiusPixels}");
                    if (layer is not null)
                    {
                        AssertTrue(ReferenceEquals(layer.Picture, messages.Picture(state.Tilemap)),
                            "captured message shows the image of the words shown");
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

        // Reveal geometry: each image the sheet holds of each height paints, at every radius, exactly what the oracle
        // paints of its words: the intersection of the image's centered extent and the half-open reveal window.
        foreach (IGrouping<int, KeyValuePair<string, System.Collections.Immutable.ImmutableArray<ushort>>> height in
                 messages.Messages.GroupBy(message => message.Value.Length / GameplayMessageRomData.Layout.TilemapWidth))
        {
            ushort[] tiles = [.. height.First().Value];
            for (int radius = 0; radius <= GameplayMessageRomData.Timing.MaximumRadiusPixels; radius++)
            {
                var layer = MessageBoxRenderLayer.FromTilemap(tiles, radius, messages);
                Rgba32[] expected = baseline.ToArray();
                GameplayMessageBoxRendererConstants.CompositeTilemap(expected, tiles, height.Key, radius, vram, cgram);
                var packet = new RenderFrameSnapshot(new(++samples, 1, 0), new LayeredRenderSnapshot(memory, [layer], 15));
                AssertTrue(expected.SequenceEqual(RenderForComparison(packet)), $"message geometry {height.Key} rows, radius {radius}");
            }
        }
        SpriteSheetFrame wrongSize = RepositoryInstallation.Installation.LoadRoomBlockSheets().Bg3Words.Values.First()
            .Sheet.Frame("Fill");
        AssertThrows<ArgumentException>(() => new MessageBoxRenderLayer(wrongSize, 1), "an image of no whole message rows rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => MessageBoxRenderLayer.FromTilemap(messages.Messages.Values.First().AsSpan(), 25, messages),
            "invalid reveal radius rejected");
        Console.WriteLine($"  Message snapshots: {samples} retail/glyph-window comparisons and ownership checks agree.");
    }
}
