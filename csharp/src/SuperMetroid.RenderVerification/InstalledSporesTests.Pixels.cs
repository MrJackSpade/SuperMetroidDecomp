using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;

internal static partial class InstalledSporesTests
{
    private static void CheckPixels(List<RenderFrameSnapshot> packets)
    {
        int cases = 0;
        foreach (int palette in Enumerable.Range(0, 8))
        foreach (int objPriority in Enumerable.Range(0, 4))
        foreach (bool bg1High in new[] { false, true })
        foreach (bool bg2High in new[] { false, true })
        foreach (int enabled in Enumerable.Range(0, 4))
        {
            var vram = new SnesVram(); var colors = new SnesCgram();
            colors.SetColor(0, new Bgr555(2, 2, 2));
            colors.SetColor(1, new Bgr555(10, 0, 0)); colors.SetColor(2, new Bgr555(0, 12, 0)); colors.SetColor(3, new Bgr555(0, 0, 7));
            colors.SetColor(128 + palette * 16 + 1, new Bgr555(18, 2, 0));
            for (int y = 0; y < 8; y++)
            {
                vram.LoadBytes(y * 2, new byte[] { 255, 0 });
                vram.LoadBytes(32 + y * 2, new byte[] { 255, 0 });
                vram.LoadBytes(64 + y * 2, new byte[] { 0, 255 });
                vram.LoadBytes(SnesPpuLayout.GameplayHudCharacterBaseWord * 2 + 48 + y * 2, new byte[] { 255, 255 });
            }
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)(1 | (bg1High ? 0x2000 : 0)), 2048).ToArray(),
                SnesPpuLayout.GameplayBg1TilemapWord, 1);
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)(2 | (bg2High ? 0x2000 : 0)), 2048).ToArray(),
                SnesPpuLayout.GameplayBg2TilemapWord, 1);
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)3, 1024).ToArray(), 0x5c00, 1);
            var oam = new byte[SnesPpuLayout.OamUploadByteCount];
            oam[0] = 16; oam[1] = 64; oam[3] = (byte)(objPriority << 4 | palette << 1);
            var registers = new OrdinaryGameplayRegisters(0, 0, 0, 0, 64, 32,
                SnesPpuLayout.GameplayBg2TilemapWord, 0, 0, SnesPpuLayout.GameplayHudCharacterBaseWord,
                SnesMainScreenLayers.Obj | ((enabled & 1) != 0 ? SnesMainScreenLayers.Bg1 : 0) |
                    ((enabled & 2) != 0 ? SnesMainScreenLayers.Bg2 : 0));
            var layer = SnesGameplayFrameRenderer.CaptureSpores(new(registers),
                new(RoomFxType.Spores, LayerBlendingConfiguration.Spores, 0, 0));
            var scene = new LayeredRenderSnapshot(new(vram.Bytes, colors.Colors, oam, 1), [layer], 0, 15);
            Rgba32[] pixels = SoftwareLayeredSnapshotRenderer.Render(scene);
            // Independent Mode-1 priority and integer BGR555 oracle. A color-only
            // post-process cannot distinguish excluded BG1/low OBJ from BG2 here.
            foreach (bool objectPixel in new[] { false, true })
            {
                int rank = -1; int red = 2, green = 2, blue = 2; bool blend = true;
                void Insert(int candidate, int r, int g, bool eligible)
                {
                    if (candidate < rank) return;
                    rank = candidate; red = r; green = g; blue = 0; blend = eligible;
                }
                if ((enabled & 2) != 0) Insert(bg2High ? 5 : 2, 0, 12, true);
                if ((enabled & 1) != 0) Insert(bg1High ? 6 : 3, 10, 0, false);
                if (objectPixel) Insert(new[] { 0, 1, 4, 7 }[objPriority], 18, 2, palette >= 4);
                byte Expand(int value) => (byte)((value << 3) | (value >> 2));
                var expected = new Rgba32(Expand(red), Expand(green), Expand(blue + (blend ? 7 : 0)));
                int x = objectPixel ? 16 : 64;
                Require(pixels[64 * 256 + x] == expected,
                    $"Spore source exclusion differs: x{x}, expected {expected}, actual {pixels[64 * 256 + x]}; palette {palette}, OBJ {objPriority}, BG priorities {bg1High}/{bg2High}, planes {enabled}.");
            }
            Require(pixels[0] == new Rgba32(16, 16, 16), "Spores blended into the HUD.");
            var packet = new RenderFrameSnapshot(new(packets.Count + 1, 1, 0), scene);
            if (cases == 0) CheckLegacyLayerGraph(layer, scene);
            packets.Add(packet); cases++;
        }
        Console.WriteLine($"{cases} independent spore pixel cases cover BG1 exclusion, BG2/backdrop addition, all OBJ palettes/priorities and HUD preservation.");
    }

    private static void CheckLegacyLayerGraph(GameplayColorMathRenderLayer layer, LayeredRenderSnapshot scene)
    {
        const string oldName = "SuperMetroid.Core.Rendering.XrayGameplayRenderLayer, SuperMetroid.Core";
        Require(DebuggerStateTypeIdentity.Resolve(oldName) == typeof(GameplayColorMathRenderLayer),
            "Historical layer identity no longer resolves.");
        // Rewrite only this exact encoded type identity in a production-written
        // graph. Preserve its single shared reference table, arrays and fields;
        // independently serializing each field would restart object IDs.
        byte[] current = Graph(layer);
        byte[] identity = Encoded(DebuggerStateTypeIdentity.GetSerializedName(typeof(GameplayColorMathRenderLayer)));
        byte[] historical = Encoded(oldName);
        using var bytes = new MemoryStream();
        int cursor = 0, renamed = 0;
        while (cursor < current.Length)
        {
            int relative = current.AsSpan(cursor).IndexOf(identity);
            if (relative < 0) { bytes.Write(current.AsSpan(cursor)); break; }
            bytes.Write(current.AsSpan(cursor, relative)); bytes.Write(historical);
            cursor += relative + identity.Length; renamed++;
        }
        Require(renamed > 1, "Legacy fixture did not rename object and declaring-field identities.");
        bytes.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<GameplayColorMathRenderLayer>(bytes);
        Require(SoftwareLayeredSnapshotRenderer.Render(scene).AsSpan().SequenceEqual(
            SoftwareLayeredSnapshotRenderer.Render(new(scene.Memory, [restored], 0, 15))),
            "Renaming the shared source-aware renderer changed an old debugger graph.");

        static byte[] Encoded(string value)
        {
            using var output = new MemoryStream();
            using (var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true)) writer.Write(value);
            return output.ToArray();
        }
    }
}
