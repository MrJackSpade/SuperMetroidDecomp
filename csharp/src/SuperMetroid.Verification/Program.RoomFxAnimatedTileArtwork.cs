using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
        AssertEqual(0, guarded.ForbiddenReads, "installed animations never read bank-$87 ROM data");
        AssertThrows<InvalidDataException>(() => atlas.TryResolve(
            RoomFxAnimatedTileArtworkDefinitions.LavaFirstSource, 1, out _),
            "artwork cannot change the native transfer byte count");
        AssertThrows<InvalidDataException>(() => RoomFxAnimatedTileAtlas.Load(
            new MemoryStream([1, 2, 3])), "corrupt room-FX PNG fails loudly");
        Console.WriteLine("  Room-FX animation artwork: all 23 PNG frames match native bytes and both ROM-free transfer owners.");
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
        File.Delete(file);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing room-FX override restores stock identity");
        Console.WriteLine("Room-FX animation override: an edited PNG changes live VRAM, then restores stock identity.");
    }

    private sealed class RoomFxArtworkForbiddenBus(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public int ForbiddenReads { get; private set; }
        public byte ReadByte(int address)
        {
            if ((address >> 16) == 0x87)
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed room-FX artwork read ROM ${address:X6}.");
            }
            return inner.ReadByte(address);
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
