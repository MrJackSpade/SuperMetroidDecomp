using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledRoomPlmPopulationDefinitions(
        SuperMetroidAddressSpace rom)
    {
        ushort[] expectedPointers = RoomStateDefinitions.All
            .Select(state => state.PlmPointer).Distinct().Order().ToArray();
        ushort[] actualPointers = RoomPlmPopulationDefinitions.Pointers
            .Order().ToArray();
        AssertEqual(RoomPlmPopulationDefinitions.RetailPopulationCount,
            actualPointers.Length, "compiled PLM population count");
        AssertTrue(expectedPointers.SequenceEqual(actualPointers),
            "compiled PLM populations cover precisely the retail room states");

        int recordCount = 0;
        foreach (ushort pointer in actualPointers)
        {
            ReadOnlySpan<byte> bytes = RoomPlmPopulationDefinitions.Get(pointer).Span;
            for (int offset = 0; offset < bytes.Length; offset++)
                AssertEqual(rom.ReadByte(0x8f0000 | (pointer + offset)),
                    bytes[offset],
                    $"PLM population $8F:{pointer:X4} byte {offset} matches ROM");
            recordCount += (bytes.Length - 2) / 6;
        }
        AssertEqual(RoomPlmPopulationDefinitions.RetailRecordCount,
            recordCount, "compiled PLM record count");
        AssertThrows<InvalidDataException>(
            () => RoomPlmPopulationDefinitions.Get(0xffff),
            "unknown PLM population fails loudly");

        RoomPlmHeaderDefinition[] headers = RoomPlmHeaderDefinitions.All.ToArray();
        AssertEqual(RoomPlmHeaderDefinitions.RetailHeaderCount,
            headers.Length, "compiled retail PLM header count");
        var referencedHeaders = new HashSet<ushort>();
        foreach (ushort pointer in actualPointers)
        {
            ReadOnlySpan<byte> bytes = RoomPlmPopulationDefinitions.Get(pointer).Span;
            for (int offset = 0; offset < bytes.Length - 2; offset += 6)
                referencedHeaders.Add((ushort)(bytes[offset] | bytes[offset + 1] << 8));
        }
        AssertTrue(referencedHeaders.SetEquals(headers.Select(header => header.Header)),
            "compiled header metadata covers exactly the retail PLM population headers");
        foreach (RoomPlmHeaderDefinition header in headers)
        {
            AssertEqual((ushort)(rom.ReadByte(0x840000 | header.Header) |
                    rom.ReadByte(0x840000 | (header.Header + 1)) << 8),
                header.Setup, $"PLM header $84:{header.Header:X4} setup matches ROM");
            AssertEqual((ushort)(rom.ReadByte(0x840000 | (header.Header + 2)) |
                    rom.ReadByte(0x840000 | (header.Header + 3)) << 8),
                header.InitialInstruction,
                $"PLM header $84:{header.Header:X4} initial list matches ROM");
        }
        AssertThrows<InvalidDataException>(
            () => RoomPlmHeaderDefinitions.Get(0xffff),
            "unknown retail PLM header fails loudly");

        const ushort populatedPointer = 0x8000;
        int sourceLength = RoomPlmPopulationDefinitions.Get(populatedPointer).Length;
        var guarded = new RoomPlmPopulationReadGuard(rom, populatedPointer,
            sourceLength, forbidHeaders: true);
        (int compiledCount, RoomPlmSlotSnapshot[] compiledSlots) = Load(guarded,
            populatedPointer, useCompiled: true);
        (int nativeCount, RoomPlmSlotSnapshot[] nativeSlots) = Load(rom,
            populatedPointer, useCompiled: false);
        AssertEqual(nativeCount, compiledCount,
            "compiled retail PLM load preserves sequential spawn count");
        AssertTrue(nativeSlots.SequenceEqual(compiledSlots),
            "compiled retail PLM load preserves native slots and setup results");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "compiled retail PLM loader never reads bank-$8F population bytes");
        AssertEqual(0, guarded.ForbiddenHeaderReadAttempts,
            "compiled retail PLM loader never reads bank-$84 header metadata");

        (int emptyCount, RoomPlmSlotSnapshot[] emptySlots) = Load(
            new RoomPlmPopulationReadGuard(rom, 0x8058, 2),
            0x8058, useCompiled: true);
        AssertEqual(0, emptyCount, "compiled empty PLM population terminates");
        AssertEqual(0, emptySlots.Length, "compiled empty PLM population leaves no slots");
        VerifyCompiledRoomScrollPrograms(rom);
        VerifyCompiledDynamicCollectibleGraphics(rom);
        Console.WriteLine(
            "  PLM populations: 284 sources/941 records, 70 headers, 173 scroll programs, and 17 collectible uploads match ROM; guarded sequential load preserves slot state.");
    }

    private static (int Count, RoomPlmSlotSnapshot[] Slots) Load(
        ISnesAddressSpace bus, ushort pointer, bool useCompiled)
    {
        const int width = 64;
        const int height = 64;
        var level = new RoomLevelData(width, height,
            new ushort[width * height], new byte[width * height],
            new ushort[width * height], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        int count = plms.LoadRoomPopulation(bus, level,
            level.CreateBackgroundStreamer(), new SnesVram(), pointer,
            new Bank80SystemState(), AreaId.Crateria,
            () => new SamusState(), () => false,
            hasAreaBossBit: _ => false,
            hasEvent: _ => false,
            setEvent: _ => { },
            useCompiledRetailPopulation: useCompiled);
        return (count, plms.PopulationSlots.ToArray());
    }

    private sealed class RoomPlmPopulationReadGuard(
        ISnesAddressSpace source, ushort pointer, int length,
        bool forbidHeaders = false) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }
        internal int ForbiddenHeaderReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int first = 0x8f0000 | pointer;
            if (address >= first && address < first + length)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Retail PLM population was reread at ${address:X6}.");
            }
            if (forbidHeaders)
            {
                foreach (RoomPlmHeaderDefinition header in RoomPlmHeaderDefinitions.All)
                {
                    int headerAddress = 0x840000 | header.Header;
                    if (address >= headerAddress && address < headerAddress + 4)
                    {
                        ForbiddenHeaderReadAttempts++;
                        throw new InvalidOperationException(
                            $"Retail PLM header was reread at ${address:X6}.");
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
