using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Source-selected spores FX acceptance, not a gameplay or read-discovery probe.</summary>
internal static partial class InstalledSporesTests
{
    internal static void Run(string installationRoot, string romPath, string nativeArtworkDirectory)
    {
        string maps = Path.Combine(Path.GetFullPath(installationRoot), "game", "maps");
        AreaMapPresentationCatalog stock = AreaMapPresentationCatalog.Load(maps, null);
        CheckArtwork(stock.RoomFxAnimatedTiles, maps, romPath, nativeArtworkDirectory);
        var packets = new List<RenderFrameSnapshot>();
        string overrides = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "installed-spores-" + Guid.NewGuid().ToString("N")));
        try
        {
            Directory.CreateDirectory(overrides);
            WriteOverrides(maps, overrides);
            AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(maps, overrides);
            Require(stock.ContentIdentity != edited.ContentIdentity, "Spore overrides did not affect selected content identity.");
            CheckOwners(stock, edited, packets);
            CheckPixels(packets);
            foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
            {
                using var device = new D3D11RenderDevice(kind);
                using var renderer = new D3D11FrameRenderer(device);
                foreach (RenderFrameSnapshot packet in packets)
                {
                    RenderFrameSnapshot restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                    PixelComparison.Verify(restored, SoftwareFrameSnapshotRenderer.Render(restored),
                        renderer.RenderForReadback(restored), $"{kind}: spores case {packet.Identity}");
                }
                Console.WriteLine($"{kind}: {packets.Count} spore/source-priority packets match software exactly after round trip.");
            }
            CheckFileFailures(maps, overrides);
            foreach (string file in Directory.GetFiles(overrides)) File.Delete(file);
            Require(AreaMapPresentationCatalog.Load(maps, overrides).ContentIdentity == stock.ContentIdentity,
                "Removing the overrides did not restore stock content identity.");
        }
        finally
        {
            string owner = Path.GetFullPath(Path.Combine("csharp", "test-temp")) + Path.DirectorySeparatorChar;
            if (!overrides.StartsWith(owner, StringComparison.OrdinalIgnoreCase) ||
                !Path.GetFileName(overrides).StartsWith("installed-spores-", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing to remove a non-fixture directory.");
            if (Directory.Exists(overrides)) Directory.Delete(overrides, true);
        }
        Console.WriteLine("Spore FX: installed-only timing, freeze, scroll, editing, legacy artwork and native layer exclusions verified. No gameplay/read discovery or player data used.");
    }

    private static void CheckOwners(AreaMapPresentationCatalog stock, AreaMapPresentationCatalog edited,
        List<RenderFrameSnapshot> packets)
    {
        var mechanics = RoomFxAnimatedTileMechanicsDefinitions.All.Single(
            value => value.ObjectPointer == AnimatedTileObject.Spores);
        Require(mechanics.InstructionPointer == 0x82ed && mechanics.TransferByteCount == 48 &&
            mechanics.EncodedVramDestination == 0x4280 && mechanics.GotoInstructionPointer == 0x82f9 &&
            mechanics.Frames.Count == 3 && mechanics.Frames.All(frame => frame.Duration == 10),
            "Compiled spores controls differ from pinned $87:82ED-$8302.");
        int records = 0, comparisons = 0, alteredTransfers = 0;
        foreach (var definition in CoreAccess.AllRoomFxRecords().Where(record => record.Type == (byte)RoomFxType.Spores))
        {
            records++;
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            byte[] initialRam = memory.WorkRam.ToArray(), initialSram = memory.SaveRam.ToArray();
            var baselineVram = new SnesVram(); var editedVram = new SnesVram();
            var colors = new SnesCgram(); var editedColors = new SnesCgram();
            RoomLayer3FxState baseline = Create(stock), replacement = Create(edited);
            baseline.Load(memory, baselineVram, colors, definition.Pointer, definition.DoorPointer, 0);
            replacement.Load(memory, editedVram, editedColors, definition.Pointer, definition.DoorPointer, 0);
            Require(baseline.Type == RoomFxType.Spores && baseline.CaptureForDisplay() is not null,
                "The cartridge's spores FX has no display capture.");
            Require(baseline.LayerBlendConfiguration == LayerBlendingConfiguration.Spores,
                "A compiled spores record selects incompatible blending.");
            Require(baselineVram.Bytes.Slice(RoomFxRomData.Layer3.TilemapDestinationWord * 2,
                RoomFxLayer3TilemapFormat.PageByteCount).SequenceEqual(stock.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Spores).Span),
                "Spore tilemap was not loaded at the native destination.");
            int tick = 0;
            for (int frame = 0; frame < 1050; frame++)
            {
                bool frozen = frame is >= 37 and < 45;
                ushort cameraX = unchecked((ushort)(frame * 79)), cameraY = unchecked((ushort)(65520 + frame * 3));
                RoomLayer3FxRenderSnapshot before = baseline.CaptureForDisplay()!.Value;
                baseline.Step(memory, baselineVram, cameraX, cameraY, frozen, mainGameLoopCarry: false);
                replacement.Step(memory, editedVram, cameraX, cameraY, frozen, mainGameLoopCarry: false);
                var capture = baseline.CaptureForDisplay()!.Value;
                if (frozen) Require(capture == before, "Frozen spores changed scroll or phase.");
                else
                {
                    ushort accumulator = unchecked((ushort)(-tick * 64));
                    Require(capture.HorizontalScroll == cameraX && capture.VerticalScroll ==
                        unchecked((ushort)(cameraY + (sbyte)(accumulator >> 8))), "Spores lost native signed 8.8 camera anchoring.");
                    int source = RoomFxAnimatedTileArtworkDefinitions.SourceAddress(mechanics,
                        mechanics.Frames[tick / 10 % 3].InstructionPointer);
                    Require(stock.RoomFxAnimatedTiles.TryResolve(source, 48, out var expected), "Missing stock spores frame.");
                    Require(edited.RoomFxAnimatedTiles.TryResolve(source, 48, out var altered), "Missing edited spores frame.");
                    Require(baselineVram.Bytes.Slice(0x4280 * 2, 48).SequenceEqual(expected.Span), "Stock spore cadence/DMA differs.");
                    Require(editedVram.Bytes.Slice(0x4280 * 2, 48).SequenceEqual(altered.Span), "Edited spore cadence/DMA differs.");
                    if (!expected.Span.SequenceEqual(altered.Span)) alteredTransfers++;
                    tick++;
                }
                Require(capture == replacement.CaptureForDisplay(), "Presentation overrides changed scroll/capture mechanics.");
                if (frame is < 50 or 510 or 511 or 512 or 1024 or 1049)
                {
                    Require(Graph(baseline).AsSpan().SequenceEqual(Graph(replacement)), "Presentation edits changed serialized FX control state.");
                    var liquid = new SamusLiquidPhysicsState(); replacement.ApplyToSamusLiquidPhysics(liquid);
                    Require(liquid.FxType == RoomFxType.Spores && liquid.FxYPosition == ushort.MaxValue &&
                        liquid.LavaAcidYPosition == ushort.MaxValue, "Spore artwork activated liquid physics.");
                }
                Require(baseline.SoundRequests.Count == 0 && replacement.SoundRequests.Count == 0 &&
                    baseline.EarthquakeRequest is null && replacement.EarthquakeRequest is null,
                    "Spore presentation produced audio or earthquake side effects.");
                if (frame is 0 or 10 or 20 or 30 or 44 or 511 or 1024)
                {
                    var gameplay = new OrdinaryGameplayRenderLayer(new(0, 0, 0, 0, 64, 32,
                        SnesPpuLayout.GameplayBg2TilemapWord, 0, 0, 0x4000, SnesMainScreenLayers.Bg2));
                    var layer = SnesGameplayFrameRenderer.CaptureSpores(gameplay, replacement.CaptureForDisplay()!.Value);
                    Require(!layer.RevealBlocks && layer.ColorMath == (SnesColorMathControl)0x32 &&
                        layer.Lines.ToArray().All(line => line.Left > line.Right) && layer.Subscreen is not null,
                        "Spore capture does not use native source-aware $32 blending.");
                    packets.Add(new(new(packets.Count + 1, 1, 0), new LayeredRenderSnapshot(
                        new(editedVram.Bytes, editedColors.Colors, new byte[SnesPpuLayout.OamUploadByteCount], 0), [layer], 0, 15)));
                }
                comparisons++;
            }
            Require(memory.WorkRam.SequenceEqual(initialRam) && memory.SaveRam.SequenceEqual(initialSram),
                "Spore presentation mutated active game RAM.");
        }
        Require(records > 0 && alteredTransfers > 0, "Source-selected spore records or edits were not exercised.");
        Console.WriteLine($"{records} compiled spores records: {comparisons} stock/edited control comparisons; {alteredTransfers} changed frame transfers, signed wrap and freeze verified.");
    }

    private static RoomLayer3FxState Create(AreaMapPresentationCatalog presentation) => new()
    {
        AnimatedTileArtwork = presentation.RoomFxAnimatedTiles,
        Layer3Tilemaps = presentation.RoomFxLayer3Tilemaps,
        PaletteBlendColors = presentation.RoomFxPaletteBlends,
    };

    private static byte[] Graph(object value)
    {
        using var bytes = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(bytes, value);
        return bytes.ToArray();
    }

    private static void Require(bool condition, string context)
    {
        if (!condition) throw new InvalidOperationException(context);
    }
}
