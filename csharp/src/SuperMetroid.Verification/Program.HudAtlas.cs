using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyHudAtlasIntegration(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original, AreaMapCartridgeData[] rules)
    {
        byte[] source = RomDataReader.ReadFixedBank(bus, HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount);
        AssertTrue(source.AsSpan().SequenceEqual(original.Resolve(VramAssetId.StandardHudTiles).Span), "HUD PNG and compiled padding match all native transfer bytes");
        var guard = new MapDataGuard(bus, rules);
        var native = new SuperMetroidRuntime(bus);
        var installed = new SuperMetroidRuntime(guard) { MapPresentation = original };
        Initialize(native); Initialize(installed);
        byte[] beforeNmi = installed.Vram.Bytes.ToArray();
        installed.RunNmi(0, mainLoopRequestedNmi: false);
        AssertTrue(beforeNmi.AsSpan().SequenceEqual(installed.Vram.Bytes), "lag NMI does not publish installed HUD artwork");
        native.RunNmi(0, mainLoopRequestedNmi: true);
        installed.RunNmi(0, mainLoopRequestedNmi: true);
        AssertTrue(native.Vram.Bytes.SequenceEqual(installed.Vram.Bytes), "accepted NMI matches all VRAM bytes with HUD artwork ROM blocked");
        Rgba32[] baseline = Render(native);
        AssertTrue(baseline.AsSpan().SequenceEqual(Render(installed)), "stock installed HUD/minimap renders identical pixels");

        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, HudTileAtlasFormat.FileName);
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(stock, HudTileAtlasFormat.FileName)))
            image = IndexedPng.Read(input, MapTileAtlasFormat.Width, MapTileAtlasFormat.Height);
        for (int i = 0; i < image.Pixels.Length; i++) image.Pixels[i] = (byte)((image.Pixels[i] + 1) % 4);
        using (var output = File.Create(replacement)) IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        var pending = new SuperMetroidRuntime(guard) { MapPresentation = original };
        Initialize(pending);
        using var snapshot = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(snapshot, pending);
        snapshot.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidRuntime>(snapshot);
        restored.MapPresentation = edited;
        restored.RunNmi(0, mainLoopRequestedNmi: true);
        AssertTrue(edited.Resolve(VramAssetId.StandardHudTiles).Span.SequenceEqual(
            restored.Vram.Bytes.Slice(HudTileAtlasFormat.DestinationWord * 2, HudTileAtlasFormat.TransferByteCount)),
            "restored pending HUD upload uses newly bound PNG, not captured old pixels");
        Rgba32[] modified = Render(restored);
        AssertTrue(Enumerable.Range(8, 24).Any(y => Enumerable.Range(216, 40).Any(x => baseline[y * 256 + x] != modified[y * 256 + x])),
            "edited HUD PNG changes rendered minimap region after accepted NMI");
        var legacy = new SuperMetroidRuntime(bus);
        Initialize(legacy);
        legacy.MapPresentation = edited;
        AssertTrue(legacy.VramWrites.Entries.Any(entry => entry.AssetId == VramAssetId.StandardHudTiles), "binding converts known legacy bus HUD upload in place");
        legacy.RunNmi(0, true);
        AssertTrue(Render(legacy).AsSpan().SequenceEqual(modified), "legacy pending bus upload follows current installed artwork after rebind");
        File.WriteAllText(replacement, "broken HUD PNG");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "broken HUD override has no silent fallback");
        Console.WriteLine("HUD PNG: exact native NMI/VRAM/pixel parity, lag gating, visible minimap edit and current-content pending-state rebind pass.");

        void Initialize(SuperMetroidRuntime runtime)
        {
            // No room palette has been installed in this focused runtime fixture.
            // Give every BG3 color a distinct grayscale value so a changed index is
            // observable, rather than accepting parity between two black HUDs.
            for (int i = 0; i < 32; i++) runtime.Cgram.SetColor(i, (ushort)(i * 0x421));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.Hud.UpdateMinimap(runtime.MapPresentation is null ? bus : guard, runtime.System,
                AreaId.Crateria, 5, 5, 16, 16, 128, 128, 8, MapRevealMode.Secret, runtime.MapPresentation?.Get(AreaId.Crateria));
            runtime.Hud.QueueUpload(runtime.MapPresentation is null ? bus : guard, runtime.VramWrites);
        }
        static Rgba32[] Render(SuperMetroidRuntime runtime) => SnesBgTilemapRenderer.Render2Bpp(runtime.Vram, runtime.Cgram,
            SnesPpuLayout.GameplayHudTilemapWord, HudTileAtlasFormat.DestinationWord, 4);
    }
}
