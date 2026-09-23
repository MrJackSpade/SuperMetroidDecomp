using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyRoomFxLayer3Tilemaps()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Room-FX BG3 tilemaps: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        byte[] json = RoomFxLayer3TilemapExtractor.Extract(rom);
        RoomFxLayer3TilemapCatalog catalog = RoomFxLayer3TilemapCatalog.Load(new MemoryStream(json));
        foreach (RoomFxType type in RoomFxLayer3TilemapFormat.Types)
        {
            int source = RoomFxLayer3TilemapFormat.SourceAddress(type);
            byte[] native = RomDataReader.ReadFixedBank(rom, source,
                RoomFxLayer3TilemapFormat.PageByteCount);
            AssertTrue(catalog.Resolve(type).Span.SequenceEqual(native),
                $"room-FX {type} preserves every bank-$8A tilemap word");
            ushort pointer = RomDataReader.ReadWordFixedBank(rom,
                RoomFxRomData.Tables.Layer3TilemapPointers + (int)type);
            AssertEqual(source, RoomFxRomData.Banks.Tilemaps | pointer,
                $"room-FX {type} compiled page identity matches the native pointer table");
            if (type == RoomFxType.Spores) continue;
            SnesVram vram = LoadConstructedRoomFxTilemap(type, catalog,
                out ForbiddenRoomFxTilemapBus guarded);
            int destination = RoomFxRomData.Layer3.TilemapDestinationWord * 2;
            for (int index = 0; index < native.Length; index++)
                AssertEqual(native[index], vram.ReadByte(destination + index),
                    $"room-FX {type} installed VRAM byte {index}");
            AssertEqual(0, guarded.ForbiddenReads,
                $"room-FX {type} installed load reads no tilemap ROM or pointer table");
        }
        AssertThrows<InvalidDataException>(() => RoomFxLayer3TilemapCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt room-FX BG3 tilemap JSON fails loudly");
        Console.WriteLine("  Room-FX BG3 tilemaps: six complete native pages and five guarded installed loads pass.");
    }

    private static void VerifyRoomFxLayer3TilemapOverride(
        string stock, string overrides, AreaMapPresentationCatalog baseline)
    {
        string path = Path.Combine(overrides, RoomFxLayer3TilemapFormat.FileName);
        string stockPath = Path.Combine(stock, RoomFxLayer3TilemapFormat.FileName);
        RoomFxLayer3TilemapDocument document = JsonSerializer.Deserialize<RoomFxLayer3TilemapDocument>(
            File.ReadAllBytes(stockPath), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            }) ?? throw new InvalidDataException("Stock room-FX BG3 document is null.");
        RoomBackgroundTilemapCell originalCell = document.Pages[nameof(RoomFxType.Lava)][0];
        document.Pages[nameof(RoomFxType.Lava)][0] = originalCell with
        {
            TileColumn = (originalCell.TileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns,
        };
        using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            RoomFxLayer3TilemapCatalog.Write(output, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "room-FX BG3 edit changes installed content identity");
        AssertTrue(!edited.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Lava).Span.SequenceEqual(
            baseline.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Lava).Span),
            "room-FX BG3 edit changes the selected tilemap words");
        SnesVram vram = LoadConstructedRoomFxTilemap(RoomFxType.Lava,
            edited.RoomFxLayer3Tilemaps, out ForbiddenRoomFxTilemapBus guarded);
        ushort editedWord = BitConverter.ToUInt16(
            edited.RoomFxLayer3Tilemaps.Resolve(RoomFxType.Lava).Span[..sizeof(ushort)]);
        AssertEqual(editedWord, vram.ReadWord(RoomFxRomData.Layer3.TilemapDestinationWord),
            "edited lava tile cell reaches production BG3 VRAM");
        AssertEqual(0, guarded.ForbiddenReads,
            "edited lava page loads without native art or pointer reads");
        AssertThrows<InvalidDataException>(() =>
        {
            document.Pages[nameof(RoomFxType.Lava)][0] = originalCell with { TileColumn = 32 };
            using var invalid = new MemoryStream();
            RoomFxLayer3TilemapCatalog.Write(invalid, document);
        }, "invalid room-FX BG3 tile index fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing room-FX BG3 override restores stock content identity");
        Console.WriteLine("Room-FX BG3 override: edited lava tile reaches VRAM and removal restores stock.");
    }

    private static SnesVram LoadConstructedRoomFxTilemap(RoomFxType type,
        RoomFxLayer3TilemapCatalog catalog, out ForbiddenRoomFxTilemapBus guarded)
    {
        const ushort record = 0x9400;
        var memory = new TestAddressSpace();
        int recordAddress = RoomFxRomData.Banks.RoomDefinitions | record;
        WriteTestWord(memory, recordAddress + RoomFxRomData.Record.DoorPointerOffset, 0);
        memory.WriteByte(recordAddress + RoomFxRomData.Record.TypeOffset, (byte)type);
        memory.WriteByte(recordAddress + RoomFxRomData.Record.Layer3LayerBlendConfigurationOffset,
            (byte)LayerBlendingConfiguration.NormalGameplay);
        guarded = new ForbiddenRoomFxTilemapBus(memory);
        var state = new RoomLayer3FxState { Layer3Tilemaps = catalog };
        var vram = new SnesVram();
        state.Load(guarded, vram, new SnesCgram(), record, doorPointer: 0, randomNumber: 0);
        AssertEqual(type, state.Type, "installed room-FX BG3 page preserves the selected FX type");
        return vram;
    }

    private sealed class ForbiddenRoomFxTilemapBus(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public int ForbiddenReads { get; private set; }
        public byte ReadByte(int address)
        {
            if ((address >> 16) == 0x8a ||
                address >= RoomFxRomData.Tables.Layer3TilemapPointers &&
                address < RoomFxRomData.Tables.Layer3TilemapPointers + 14)
            {
                ForbiddenReads++;
                throw new InvalidOperationException(
                    $"Installed room-FX BG3 tilemap read ROM ${address:X6}.");
            }
            return inner.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
