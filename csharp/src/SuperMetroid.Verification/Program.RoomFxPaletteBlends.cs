using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyRoomFxPaletteBlends()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Room-FX blend palettes: cartridge comparison skipped (private ROM absent).");
            return;
        }
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        RoomFxPaletteBlendCatalog catalog = RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(RoomFxPaletteBlendExtractor.Extract(rom)));
        foreach (byte id in RoomFxPaletteBlendDefinitions.Ids)
        {
            byte[] source = RomDataReader.ReadFixedBank(rom,
                RoomFxPaletteBlendDefinitions.SourceAddress(id),
                RoomFxRomData.Layer3.PaletteBlendColorCount * sizeof(ushort));
            ReadOnlySpan<ushort> compiled = catalog.Resolve(id);
            for (int index = 0; index < compiled.Length; index++)
                AssertEqual(BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(index * sizeof(ushort))),
                    compiled[index], $"room-FX blend {id:X2} native color {index}");

            (RoomLayer3FxState state, ForbiddenRoomFxPaletteBus bus, SnesCgram cgram) =
                ConstructBlendLoad(catalog, id);
            for (int index = 0; index < compiled.Length; index++)
                AssertEqual(compiled[index], cgram.Colors[RoomFxRomData.Layer3.PaletteBlendDestinationIndex + index],
                    $"room-FX blend {id:X2} installed load color {index}");
            AssertEqual(0, bus.ForbiddenReads, $"room-FX blend {id:X2} load does not read bank-$89");

            const ushort reloadRecord = 0x9410;
            bus.Inner.WriteByte(RoomFxRomData.Banks.RoomDefinitions | reloadRecord +
                RoomFxRomData.Record.PaletteBlendOffset, id);
            _ = state.ApplyEntry(bus, cgram, reloadRecord);
            for (int index = 0; index < compiled.Length; index++)
                AssertEqual(compiled[index], cgram.Colors[RoomFxRomData.Layer3.PaletteBlendDestinationIndex + index],
                    $"room-FX blend {id:X2} installed FX-entry color {index}");
            AssertEqual(0, bus.ForbiddenReads, $"room-FX blend {id:X2} FX entry does not read bank-$89");
        }
        (RoomLayer3FxState emptyState, ForbiddenRoomFxPaletteBus emptyBus, SnesCgram emptyCgram) =
            ConstructBlendLoad(catalog, 0);
        _ = emptyState;
        AssertEqual((ushort)0x1234, emptyCgram.Colors[25], "zero blend preserves color 25");
        AssertEqual((ushort)0x2345, emptyCgram.Colors[26], "zero blend preserves color 26");
        AssertEqual((ushort)0, emptyCgram.Colors[27], "zero blend clears only color 27");
        AssertEqual(0, emptyBus.ForbiddenReads, "zero blend does not read bank-$89");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt room-FX blend resource fails loudly");
        string duplicate = "{\"version\":1,\"version\":1,\"blends\":{}}";
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(duplicate))),
            "duplicate room-FX blend property fails loudly");
        Console.WriteLine("  Room-FX blend palettes: all 24 native words and guarded load/reload paths pass.");
    }

    private static void VerifyRoomFxPaletteBlendOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline)
    {
        string path = Path.Combine(overrides, RoomFxPaletteBlendDefinitions.FileName);
        RoomFxPaletteBlendDocument document = JsonSerializer.Deserialize<RoomFxPaletteBlendDocument>(
            File.ReadAllBytes(Path.Combine(stock, RoomFxPaletteBlendDefinitions.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock room-FX blend document is null.");
        string key = RoomFxPaletteBlendDefinitions.Key(RoomFxPaletteBlendDefinitions.Lava);
        PaletteRgb5 original = document.Blends[key][0];
        document.Blends[key][0] = original with { Red = (original.Red + 1) % 32 };
        File.WriteAllBytes(path, RoomFxPaletteBlendCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "room-FX blend edit changes installed content identity");
        ushort expected = edited.RoomFxPaletteBlends.Resolve(RoomFxPaletteBlendDefinitions.Lava)[0];
        AssertTrue(expected != baseline.RoomFxPaletteBlends.Resolve(RoomFxPaletteBlendDefinitions.Lava)[0],
            "room-FX blend edit changes the authored color");
        (_, ForbiddenRoomFxPaletteBus bus, SnesCgram cgram) = ConstructBlendLoad(
            edited.RoomFxPaletteBlends, RoomFxPaletteBlendDefinitions.Lava);
        AssertEqual(expected, cgram.Colors[RoomFxRomData.Layer3.PaletteBlendDestinationIndex],
            "edited room-FX blend color reaches production CGRAM");
        AssertEqual(0, bus.ForbiddenReads, "edited room-FX blend does not read native palette table");
        AssertThrows<InvalidDataException>(() => RoomFxPaletteBlendCatalog.Write(document with
        {
            Blends = new Dictionary<string, PaletteRgb5[]>(document.Blends)
            {
                [key] = [original with { Red = 32 }, .. document.Blends[key].Skip(1)],
            },
        }), "invalid room-FX blend component fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing room-FX blend override restores stock identity");
        Console.WriteLine("Room-FX blend override: edited color reaches CGRAM and removal restores stock.");
    }

    private static (RoomLayer3FxState State, ForbiddenRoomFxPaletteBus Bus, SnesCgram Cgram)
        ConstructBlendLoad(RoomFxPaletteBlendCatalog catalog, byte selection)
    {
        const ushort record = 0x9400;
        var memory = new TestAddressSpace();
        int recordAddress = RoomFxRomData.Banks.RoomDefinitions | record;
        WriteTestWord(memory, recordAddress + RoomFxRomData.Record.DoorPointerOffset, 0);
        memory.WriteByte(recordAddress + RoomFxRomData.Record.PaletteBlendOffset, selection);
        var bus = new ForbiddenRoomFxPaletteBus(memory);
        var cgram = new SnesCgram();
        cgram.SetColor(25, 0x1234);
        cgram.SetColor(26, 0x2345);
        cgram.SetColor(27, 0x3456);
        var state = new RoomLayer3FxState { PaletteBlendColors = catalog };
        state.Load(bus, new SnesVram(), cgram, record, doorPointer: 0, randomNumber: 0);
        return (state, bus, cgram);
    }

    private sealed class ForbiddenRoomFxPaletteBus(TestAddressSpace inner) : ISnesAddressSpace
    {
        public TestAddressSpace Inner { get; } = inner;
        public int ForbiddenReads { get; private set; }
        public byte ReadByte(int address)
        {
            if ((address >> 16) == 0x89)
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed room-FX blend read ROM ${address:X6}.");
            }
            return Inner.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => Inner.WriteByte(address, value);
    }
}
