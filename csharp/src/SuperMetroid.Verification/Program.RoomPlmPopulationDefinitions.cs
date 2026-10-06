using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledRoomPlmPopulationDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyRetailPopulationMappings), () => VerifyRetailPopulationMappings(rom));

        Suite(nameof(VerifyRetailPlmHeaderSetups), () => VerifyRetailPlmHeaderSetups(rom));
        Suite(nameof(VerifyRetailPlmHeaderInstructions), () => VerifyRetailPlmHeaderInstructions(rom));
        Suite(nameof(VerifyCompiledRoomPlmHeaderLoad), () => VerifyCompiledRoomPlmHeaderLoad(rom));
        Suite(nameof(VerifyCompiledRoomScrollPrograms), () => VerifyCompiledRoomScrollPrograms(rom));
        Suite(nameof(VerifyCompiledDynamicCollectibleGraphics), () => VerifyCompiledDynamicCollectibleGraphics(rom));
        Console.WriteLine("  PLM populations, headers, scroll programs and collectible uploads match ROM.");
    }

    private static void VerifyCompiledRoomPlmHeaderLoad(SuperMetroidAddressSpace rom)
    {
        const ushort populatedPointer = 0x8000;
        int sourceLength = RoomPlmPopulationDefinition.FromCompiled(populatedPointer).Placements.Length * 6 + 2;
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
            level.CreateBackgroundStreamer(), new SnesVram(), (useCompiled ? RoomPlmPopulationDefinition.FromCompiled(pointer) : RoomPlmPopulationImporter.Read(bus, pointer)),
            new Bank80SystemState(), AreaId.Crateria,
            () => new SamusState(), () => false,
            hasAreaBossBit: _ => false,
            hasEvent: _ => false,
            setEvent: _ => { });
        return (count, plms.PopulationSlots.ToArray());
    }

    private sealed class RoomPlmPopulationReadGuard(
        ISnesAddressSpace source, ushort pointer, int length,
        bool forbidHeaders = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }
        internal int ForbiddenHeaderReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

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
