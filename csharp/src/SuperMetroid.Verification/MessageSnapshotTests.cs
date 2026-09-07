using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyMessageSnapshots()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var vram = new SnesVram();
        byte[] graphics = new byte[SnesPpuLayout.VramByteCount];
        new Random(32107).NextBytes(graphics);
        vram.LoadBytes(0, graphics);
        var cgram = new SnesCgram();
        for (int i = 0; i < SnesCgram.ColorCount; i++) cgram.SetColor(i, (ushort)(i * 101 & 32767));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = PpuMemorySnapshot.Capture(vram, cgram, oam);
        Rgba32[] baseline = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        int samples = 0;
        foreach (GameplayMessageId id in Enum.GetValues<GameplayMessageId>())
        {
            if (id == GameplayMessageId.None) continue;
            var state = new GameplayMessageBoxState();
            AssertTrue(GameplayMessageBoxRenderer.Capture(state) is null, "inactive message has no overlay");
            state.Begin(bus, id);
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
                    GameplayMessageBoxRenderer.Composite(expected, state, vram, cgram);
                    MessageBoxRenderLayer? layer = GameplayMessageBoxRenderer.Capture(state);
                    RenderLayer[] layers = layer is null ? [] : [layer];
                    var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)tick),
                        new LayeredRenderSnapshot(memory, layers, 3, 15));
                    AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(packet))),
                        $"message {id}, radius {state.RadiusPixels}");
                    if (layer is not null)
                    {
                        AssertTrue(layer.Tilemap.SequenceEqual(state.Tilemap), "captured message words match source");
                        if (held is null) { held = packet; heldPixels = expected; }
                    }
                }
                ushort input = state.Phase == GameplayMessageBoxPhase.AwaitingInput
                    ? tick % 3 == 0 ? (ushort)SnesButton.Right : tick % 3 == 2 ? (ushort)SnesButton.A : (ushort)0
                    : (ushort)0;
                if (id == GameplayMessageIds.SaveConfirmation && state.Phase == GameplayMessageBoxPhase.AwaitingInput)
                {
                    if (!toggledSave) { input = (ushort)SnesButton.Right; toggledSave = true; }
                    else if (!releasedSave) { input = 0; releasedSave = true; }
                    else input = (ushort)SnesButton.A;
                }
                state.Step(input);
            }
            AssertTrue(!state.IsActive, $"message {id} completes reveal/close fixture");
            // Reuse the same live owner after closing, replacing its tilemap and radius.
            state.Begin(bus, GameplayMessageIds.SaveCompleted);
            AssertTrue(held is not null && heldPixels!.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(held)),
                "retained message survives closing and reuse of the source owner");
        }

        // Independent geometric oracle: solid color-one glyphs must paint exactly the
        // intersection of the art's centered extent and the half-open reveal window.
        vram = new SnesVram();
        for (int y = 0; y < 8; y++) vram.LoadBytes(GameplayMessageRomData.Layout.CharacterBaseWord * 2 + y * 2, [255, 0]);
        memory = PpuMemorySnapshot.Capture(vram, cgram, oam);
        for (int rows = 3; rows <= 6; rows++)
        for (int radius = 0; radius <= 24; radius++)
        {
            ushort[] tiles = Enumerable.Repeat((ushort)(6 << 10), rows * 32).ToArray();
            var layer = new MessageBoxRenderLayer(tiles, radius);
            Array.Clear(tiles);
            var packet = new RenderFrameSnapshot(new(++samples, 1, 0), new LayeredRenderSnapshot(memory, [layer], 3, 15));
            Rgba32[] actual = SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(packet));
            int top = 124 - Math.Min(radius, rows * 4), bottom = 124 + Math.Min(radius, rows * 4);
            Rgba32 ink = SnesGraphics.DecodeBgr555Color(GameplayMessageRomData.Palette.TemporaryLightColor);
            for (int y = 0; y < 224; y++)
                AssertTrue(actual.AsSpan(y * 256, 256).ToArray().All(p => p == (y >= top && y < bottom ? ink : baseline[y * 256])),
                    $"message geometry {rows} rows, radius {radius}, line {y}");
            if (rows == 3 && radius == 0)
            {
                byte[] bytes = RenderFrameSnapshotCodec.Serialize(packet);
                AssertThrows<IOException>(() => RenderFrameSnapshotCodec.Deserialize(bytes[..^1]), "truncated message rejected");
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(RenderPacketFormat.Signature.Length),
                    RenderPacketFormat.ScanlineColorLayerVersion);
                AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(bytes), "older format rejects message operation");
            }
        }
        AssertThrows<ArgumentException>(() => new MessageBoxRenderLayer(new ushort[95], 1), "partial message row rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new MessageBoxRenderLayer(new ushort[96], 25), "invalid reveal radius rejected");
        Console.WriteLine($"  Message snapshots: {samples} retail/glyph-window comparisons and ownership/codec checks agree.");
    }
}
