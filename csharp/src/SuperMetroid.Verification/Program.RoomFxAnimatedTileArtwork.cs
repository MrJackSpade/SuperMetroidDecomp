using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Exercises every retail frame through both native transfer owners.</summary>
    private static void VerifyRoomFxAnimatedTileArtwork()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Room-FX animation artwork: retail comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        byte[] png = RoomFxAnimatedTileAtlasExtractor.Extract(rom);
        RoomFxAnimatedTileAtlas atlas = RoomFxAnimatedTileAtlas.Load(new MemoryStream(png));
        var guarded = new RoomFxArtworkForbiddenBus(rom);
        var provider = new RoomFxArtworkTestProvider(atlas);
        int frameCount = 0;
        foreach (RoomFxAnimatedTileObjectDefinition definition in
                 RoomFxAnimatedTileMechanicsDefinitions.All)
        {
            var direct = new RoomFxAnimatedTilesState();
            var queued = new RoomFxAnimatedTilesState();
            var directVram = new SnesVram();
            var queuedVram = new SnesVram();
            var writes = new VramWriteQueue();
            direct.LoadDefinition(guarded, definition.ObjectPointer);
            queued.LoadDefinition(guarded, definition.ObjectPointer);
            for (int frameIndex = 0; frameIndex < definition.Frames.Count; frameIndex++)
            {
                RoomFxAnimatedTileFrameDefinition frame = definition.Frames[frameIndex];
                int source = RoomFxAnimatedTileArtworkDefinitions.SourceAddress(
                    definition, frame.InstructionPointer);
                byte[] native = RomDataReader.ReadFixedBank(rom, source,
                    definition.TransferByteCount);
                AssertTrue(atlas.TryResolve(source, native.Length, out ReadOnlyMemory<byte> installed) &&
                    installed.Span.SequenceEqual(native),
                    $"room-FX frame $87:{frame.InstructionPointer:X4} preserves every native art byte");
                int ticksUntilFrame = frameIndex == 0 ? 1 : definition.Frames[frameIndex - 1].Duration;
                for (int tick = 0; tick < ticksUntilFrame; tick++)
                {
                    direct.Step(guarded, directVram, artwork: atlas);
                    queued.Step(guarded, queuedVram, writes);
                }
                AssertEqual(source, direct.LastSourceAddress,
                    $"room-FX frame $87:{frame.InstructionPointer:X4} selects its compiled art source");
                AssertEqual(source, queued.LastSourceAddress,
                    $"queued room-FX frame $87:{frame.InstructionPointer:X4} selects its compiled art source");
                AssertEqual(1, writes.Entries.Count,
                    $"room-FX frame $87:{frame.InstructionPointer:X4} queues exactly one native DMA");
                writes.DrainTo(queuedVram, guarded, provider);
                int destination = definition.EncodedVramDestination * 2;
                for (int index = 0; index < native.Length; index++)
                {
                    AssertEqual(native[index], directVram.ReadByte(destination + index),
                        $"direct room-FX frame $87:{frame.InstructionPointer:X4} pixel byte {index}");
                    AssertEqual(native[index], queuedVram.ReadByte(destination + index),
                        $"queued room-FX frame $87:{frame.InstructionPointer:X4} pixel byte {index}");
                }
                frameCount++;
            }
        }
        AssertEqual(23, frameCount, "all simple room-FX frames are covered");
        foreach (WreckedShipTreadmillDirection direction in
                 Enum.GetValues<WreckedShipTreadmillDirection>())
        {
            var treadmill = new WreckedShipTreadmillAnimatedTilesState();
            var treadmillVram = new SnesVram();
            var treadmillWrites = new VramWriteQueue();
            treadmill.Start(guarded, direction);
            treadmill.Step(guarded, areaBossDefeated: false, treadmillWrites);
            AssertEqual(0, treadmillWrites.Entries.Count,
                $"{direction} waits for Phantoon's area-boss bit before presenting art");
            for (int frame = 0; frame < RoomFxAnimatedTileAtlasFormat.TreadmillFrameCount;
                 frame++)
            {
                int nativeFrame = direction == WreckedShipTreadmillDirection.Rightwards
                    ? frame : 3 - frame;
                int source = WreckedShipTreadmillRomData.FrameSource(nativeFrame);
                byte[] native = RomDataReader.ReadFixedBank(rom, source,
                    WreckedShipTreadmillRomData.TransferByteCount);
                AssertTrue(atlas.TryResolve(source, native.Length,
                        out ReadOnlyMemory<byte> installed) &&
                    installed.Span.SequenceEqual(native),
                    $"{direction} treadmill frame {frame} preserves every cartridge pixel byte");
                treadmill.Step(guarded, areaBossDefeated: true, treadmillWrites);
                AssertEqual(source, treadmill.LastSourceAddress,
                    $"{direction} frame {frame} selects the compiled artwork identity");
                AssertEqual(1, treadmillWrites.Entries.Count,
                    $"{direction} frame {frame} queues exactly one native DMA");
                treadmillWrites.DrainTo(treadmillVram, guarded, provider);
                int destination = WreckedShipTreadmillRomData.EncodedVramDestination * 2;
                for (int index = 0; index < native.Length; index++)
                    AssertEqual(native[index], treadmillVram.ReadByte(destination + index),
                        $"{direction} frame {frame} installed NMI pixel byte {index}");
            }
        }
        AssertEqual(0, guarded.ForbiddenReads, "installed animations never read bank-$87 ROM data");
        AssertThrows<InvalidDataException>(() => atlas.TryResolve(
            RoomFxAnimatedTileArtworkDefinitions.LavaFirstSource, 1, out _),
            "artwork cannot change the native transfer byte count");
        AssertThrows<InvalidDataException>(() => RoomFxAnimatedTileAtlas.Load(
            new MemoryStream([1, 2, 3])), "corrupt room-FX PNG fails loudly");
        Console.WriteLine("  Room-FX animation artwork: 23 simple and four treadmill PNG frames match native bytes and ROM-free NMI transfers.");
    }

    private static void VerifyRoomFxAnimatedTileArtworkOverride(ISnesAddressSpace rom,
        string stock, string overrides, AreaMapPresentationCatalog baseline)
    {
        string file = Path.Combine(overrides, RoomFxAnimatedTileAtlasFormat.FileName);
        byte[] source = File.ReadAllBytes(Path.Combine(stock, RoomFxAnimatedTileAtlasFormat.FileName));
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(source),
            RoomFxAnimatedTileAtlasFormat.Width, RoomFxAnimatedTileAtlasFormat.Height);
        byte[] editedPixels = image.Pixels.ToArray();
        editedPixels[0] = (byte)((editedPixels[0] + 1) % RoomFxAnimatedTileAtlasFormat.ColorCount);
        editedPixels[RoomFxAnimatedTileAtlasFormat.LegacyWidth] = (byte)(
            (editedPixels[RoomFxAnimatedTileAtlasFormat.LegacyWidth] + 1) %
            RoomFxAnimatedTileAtlasFormat.ColorCount);
        editedPixels[RoomFxAnimatedTileAtlasFormat.PreStatueWidth] = (byte)(
            (editedPixels[RoomFxAnimatedTileAtlasFormat.PreStatueWidth] + 1) %
            RoomFxAnimatedTileAtlasFormat.ColorCount);
        using (var output = new FileStream(file, FileMode.CreateNew, FileAccess.Write))
            IndexedPng.Write(output, image.Width, image.Height, editedPixels,
                SnesGraphics.DiagnosticPalette(RoomFxAnimatedTileAtlasFormat.ColorCount));
        AreaMapPresentationCatalog changed = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(changed.ContentIdentity != baseline.ContentIdentity,
            "room-FX PNG edit changes installed content identity");
        int sourceAddress = RoomFxAnimatedTileArtworkDefinitions.MaridiaSandCeilingFirstSource;
        AssertTrue(changed.RoomFxAnimatedTiles.TryResolve(sourceAddress, 0x40,
            out ReadOnlyMemory<byte> edited), "edited room-FX atlas resolves first frame");
        AssertTrue(baseline.RoomFxAnimatedTiles.TryResolve(sourceAddress, 0x40,
            out ReadOnlyMemory<byte> original), "stock room-FX atlas resolves first frame");
        AssertTrue(!edited.Span.SequenceEqual(original.Span),
            "edited room-FX PNG changes the selected frame bytes");
        var state = new RoomFxAnimatedTilesState();
        var vram = new SnesVram();
        var guarded = new RoomFxArtworkForbiddenBus(rom);
        state.LoadDefinition(guarded, AnimatedTileObjectPointers.MaridiaSandCeiling);
        state.Step(guarded, vram, artwork: changed.RoomFxAnimatedTiles);
        AssertEqual(edited.Span[0], vram.ReadByte(0x1000 * 2),
            "edited room-FX pixel reaches the active animation transfer");
        AssertEqual(0, guarded.ForbiddenReads, "edited room-FX transfer remains ROM-free");
        int treadmillSource = WreckedShipTreadmillRomData.Frame0Source;
        AssertTrue(changed.RoomFxAnimatedTiles.TryResolve(treadmillSource,
                WreckedShipTreadmillRomData.TransferByteCount,
                out ReadOnlyMemory<byte> editedTreadmill),
            "edited room-FX atlas resolves the Wrecked Ship treadmill frame");
        AssertTrue(baseline.RoomFxAnimatedTiles.TryResolve(treadmillSource,
                WreckedShipTreadmillRomData.TransferByteCount,
                out ReadOnlyMemory<byte> stockTreadmill),
            "stock room-FX atlas resolves the Wrecked Ship treadmill frame");
        AssertTrue(!editedTreadmill.Span.SequenceEqual(stockTreadmill.Span),
            "edited treadmill pixels compile to a distinct installed NMI frame");
        var runtime = new SuperMetroidRuntime(guarded) { MapPresentation = changed };
        runtime.WreckedShipTreadmill.Start(guarded,
            WreckedShipTreadmillDirection.Rightwards);
        runtime.WreckedShipTreadmill.Step(guarded, areaBossDefeated: true,
            runtime.VramWrites);
        runtime.VramWrites.DrainTo(runtime.Vram, guarded, runtime);
        AssertEqual(editedTreadmill.Span[0], runtime.Vram.ReadByte(
                WreckedShipTreadmillRomData.EncodedVramDestination * 2),
            "edited treadmill pixel reaches the production runtime NMI destination");
        AssertEqual(0, guarded.ForbiddenReads,
            "edited treadmill animation and queued upload remain ROM-free");
        int statueSource = TourianStatueAnimatedTileArtworkDefinitions.FirstSource;
        AssertTrue(baseline.RoomFxAnimatedTiles.TryResolve(statueSource, 0x80,
                out ReadOnlyMemory<byte> stockStatue),
            "stock Tourian statue frame is installed");
        AssertTrue(changed.RoomFxAnimatedTiles.TryResolve(statueSource, 0x80,
                out ReadOnlyMemory<byte> editedStatue) &&
            !editedStatue.Span.SequenceEqual(stockStatue.Span),
            "edited Tourian statue pixels compile to a distinct installed frame");
        var statueVram = new SnesVram();
        var statueWrites = new VramWriteQueue();
        statueWrites.Enqueue(0x80, statueSource, 0x7800);
        statueWrites.DrainTo(statueVram, guarded,
            new RoomFxArtworkTestProvider(changed.RoomFxAnimatedTiles));
        AssertEqual(editedStatue.Span[0], statueVram.ReadByte(0x7800 * 2),
            "edited Tourian statue pixel reaches the native NMI destination");
        File.Delete(file);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing room-FX override restores stock identity");

        // The immediately preceding 97-character format keeps its edits and
        // inherits only the newly added Tourian statue strip from current stock.
        byte[] preStatuePixels = new byte[
            RoomFxAnimatedTileAtlasFormat.PreStatueWidth * RoomFxAnimatedTileAtlasFormat.Height];
        for (int row = 0; row < RoomFxAnimatedTileAtlasFormat.Height; row++)
            image.Pixels.AsSpan(row * RoomFxAnimatedTileAtlasFormat.Width,
                    RoomFxAnimatedTileAtlasFormat.PreStatueWidth)
                .CopyTo(preStatuePixels.AsSpan(row * RoomFxAnimatedTileAtlasFormat.PreStatueWidth,
                    RoomFxAnimatedTileAtlasFormat.PreStatueWidth));
        preStatuePixels[0] = (byte)((preStatuePixels[0] + 1) %
            RoomFxAnimatedTileAtlasFormat.ColorCount);
        using (var output = new FileStream(file, FileMode.CreateNew, FileAccess.Write))
            IndexedPng.Write(output, RoomFxAnimatedTileAtlasFormat.PreStatueWidth,
                RoomFxAnimatedTileAtlasFormat.Height, preStatuePixels,
                SnesGraphics.DiagnosticPalette(RoomFxAnimatedTileAtlasFormat.ColorCount));
        AreaMapPresentationCatalog migratedPreStatue =
            AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(migratedPreStatue.RoomFxAnimatedTiles.TryResolve(sourceAddress, 0x40,
                out ReadOnlyMemory<byte> migratedPreStatueSimple) &&
            !migratedPreStatueSimple.Span.SequenceEqual(original.Span),
            "pre-statue override retains its edited room-FX pixels");
        AssertTrue(migratedPreStatue.RoomFxAnimatedTiles.TryResolve(statueSource, 0x80,
                out ReadOnlyMemory<byte> migratedStatue) &&
            migratedStatue.Span.SequenceEqual(stockStatue.Span),
            "pre-statue override inherits checked stock statue pixels");
        File.Delete(file);

        // A pre-treadmill 89-character override must keep its original edits after an
        // update, with only the newly added four frames inherited from checked stock.
        byte[] legacyPixels = new byte[
            RoomFxAnimatedTileAtlasFormat.LegacyWidth * RoomFxAnimatedTileAtlasFormat.Height];
        for (int row = 0; row < RoomFxAnimatedTileAtlasFormat.Height; row++)
            image.Pixels.AsSpan(row * RoomFxAnimatedTileAtlasFormat.Width,
                    RoomFxAnimatedTileAtlasFormat.LegacyWidth)
                .CopyTo(legacyPixels.AsSpan(row * RoomFxAnimatedTileAtlasFormat.LegacyWidth,
                    RoomFxAnimatedTileAtlasFormat.LegacyWidth));
        legacyPixels[0] = (byte)((legacyPixels[0] + 1) %
            RoomFxAnimatedTileAtlasFormat.ColorCount);
        using (var output = new FileStream(file, FileMode.CreateNew, FileAccess.Write))
            IndexedPng.Write(output, RoomFxAnimatedTileAtlasFormat.LegacyWidth,
                RoomFxAnimatedTileAtlasFormat.Height, legacyPixels,
                SnesGraphics.DiagnosticPalette(RoomFxAnimatedTileAtlasFormat.ColorCount));
        AreaMapPresentationCatalog migrated = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(migrated.RoomFxAnimatedTiles.TryResolve(sourceAddress, 0x40,
                out ReadOnlyMemory<byte> migratedSimple) &&
            !migratedSimple.Span.SequenceEqual(original.Span),
            "old-size room-FX override retains its edited liquid/sand pixels");
        AssertTrue(migrated.RoomFxAnimatedTiles.TryResolve(treadmillSource,
                WreckedShipTreadmillRomData.TransferByteCount,
                out ReadOnlyMemory<byte> migratedTreadmill) &&
            migratedTreadmill.Span.SequenceEqual(stockTreadmill.Span),
            "old-size room-FX override receives only new treadmill frames from stock");
        File.Delete(file);
        Console.WriteLine("Room-FX animation override: edited simple/treadmill/statue pixels reach VRAM; both legacy sheet sizes retain edits.");
    }

    private sealed class RoomFxArtworkForbiddenBus(ISnesAddressSpace inner)
        : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public int ForbiddenReads { get; private set; }
        public byte ReadByte(int address)
        {
            RejectArtworkRead(address);
            return inner.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectArtworkRead(address);
            return CartridgeImportSource.Require(inner).ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (inner as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Room-FX verification source requires WRAM.")).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (inner as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Room-FX verification source requires SRAM.")).ReadSaveRamByte(address);

        private void RejectArtworkRead(int address)
        {
            if ((address >> 16) == 0x87)
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed room-FX artwork read ROM ${address:X6}.");
            }
        }
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }

    private sealed class RoomFxArtworkTestProvider(RoomFxAnimatedTileAtlas atlas) :
        IVramAssetProvider, IRomArtworkSource
    {
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException($"Room-FX test did not expect typed asset {asset}.");
        public bool TryResolve(int sourceAddress, int byteCount,
            out ReadOnlyMemory<byte> data) => atlas.TryResolve(sourceAddress, byteCount, out data);
    }
}
