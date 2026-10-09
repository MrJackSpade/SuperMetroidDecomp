using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks compiled room PLM populations and related native header and scroll data against the ROM.</summary>
    /// <param name="rom">Retail cartridge address space used as the source of the comparison data.</param>
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

    /// <summary>Confirms compiled PLM population loading matches native spawns without rereading population or header bytes.</summary>
    /// <param name="rom">Retail cartridge address space used for native and guarded load comparisons.</param>
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

    /// <summary>Loads one PLM population into an isolated room fixture using either imported or compiled placement data.</summary>
    /// <param name="bus">Address space supplied to the room PLM loader.</param>
    /// <param name="pointer">Pointer identifying the room's PLM population.</param>
    /// <param name="useCompiled"><see langword="true"/> to use compiled placements; otherwise read placements from the cartridge.</param>
    /// <returns>The number of populated slots and snapshots of the resulting slot state.</returns>
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

    /// <summary>Detects runtime reads of population data and optionally room-header metadata during a compiled load.</summary>
    /// <param name="source">Underlying address space for reads outside the guarded ranges.</param>
    /// <param name="pointer">Start pointer of the PLM population whose bytes should remain unread.</param>
    /// <param name="length">Byte length of that population, including its terminator.</param>
    /// <param name="forbidHeaders">Whether room-header metadata reads should also be counted and rejected.</param>
    private sealed class RoomPlmPopulationReadGuard(
        ISnesAddressSpace source, ushort pointer, int length,
        bool forbidHeaders = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempts to read bytes in the guarded PLM population.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Number of attempts to read guarded room-header metadata.</summary>
        internal int ForbiddenHeaderReadAttempts { get; private set; }

        /// <summary>Routes cartridge-byte requests through the guarded byte-read path.</summary>
        /// <param name="address">Cartridge byte address to read.</param>
        /// <returns>The underlying byte when the address is outside guarded ranges.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of the selected population and optionally header metadata, forwarding other reads.</summary>
        /// <param name="address">CPU-visible byte address to read.</param>
        /// <returns>The byte returned by the underlying address space when allowed.</returns>
        /// <exception cref="InvalidOperationException">The address falls in a guarded population or header range.</exception>
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
                foreach (RoomPlmHeaderDefinition header in RoomPlmHeaderDefinitionsTooling.All)
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

        /// <summary>Forwards a byte write to the underlying address space.</summary>
        /// <param name="address">CPU-visible byte address to write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
