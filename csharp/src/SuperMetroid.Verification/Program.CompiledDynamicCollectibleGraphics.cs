using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies that room loading uses compiled collectible graphics instead of rereading their retail payloads.</summary>
    /// <param name="rom">Retail address space used for fixture setup and guarded room loading.</param>
    private static void VerifyCompiledDynamicCollectibleGraphics(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDynamicCollectibleSourcePointers), () => VerifyDynamicCollectibleSourcePointers(rom));
        Suite(nameof(VerifyDynamicCollectibleKindIdentity), () => VerifyDynamicCollectibleKindIdentity());
        Suite(nameof(VerifyDynamicCollectiblePaletteSelectors), () => VerifyDynamicCollectiblePaletteSelectors(rom));

        // Retail population $8F:83FE contains a Chozo-orb Bombs item. Ban every
        // compiled graphics payload and every retail item-list upload from the bus,
        // then exercise the real sequential room loader and assert its resulting VRAM.
        var guarded = new RetailCollectibleGraphicsReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(width, 64,
            new ushort[width * 64], new byte[width * 64],
            new ushort[width * 64], new byte[0x400 * 8]);
        var vram = new SnesVram();
        var plms = new RoomPlmSystem();
        plms.LoadRoomPopulation(guarded, level,
            level.CreateBackgroundStreamer(), vram, RoomPlmPopulationDefinition.FromCompiled(0x83fe),
            new Bank80SystemState(), AreaId.Crateria,
            () => new SamusState(), () => false);
        AssertTrue(plms.Collectibles.Any(item =>
                item.Kind == InWorldCollectibleKind.Bombs),
            "retail Bombs item remains allocated after compiled graphics upload");
        ReadOnlySpan<byte> expected =
            RoomPlmDynamicCollectibleGraphicsDefinitions.Get(
                InWorldCollectibleKind.Bombs).Tiles.Span;
        for (int offset = 0; offset < expected.Length; offset++)
            AssertEqual(expected[offset], vram.ReadByte(0x3e00 * 2 + offset),
                $"retail Bombs item VRAM character byte {offset}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail collectible loader does not reread compiled character or palette bytes");
        Suite(nameof(VerifyInstalledDynamicCollectibleArt), () => VerifyInstalledDynamicCollectibleArt(rom));
    }

    /// <summary>Reads a little-endian word from bank $84 for resolving a retail collectible instruction list.</summary>
    /// <param name="bus">Address space containing the retail header bytes.</param>
    /// <param name="address">Low-byte address of the word within bank $84.</param>
    /// <returns>The two bytes combined with the lower-address byte first.</returns>
    private static ushort ReadCollectibleGraphicsWord(
        ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(0x840000 | address) |
        bus.ReadByte(0x840000 | (address + 1)) << 8));

    /// <summary>Wraps retail memory and rejects collectible graphics or item-list reads replaced by compiled data.</summary>
    /// <param name="source">Underlying cartridge address space for permitted reads and all writes.</param>
    private sealed class RetailCollectibleGraphicsReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempted reads from compiled collectible graphics or upload instruction bytes.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Checks importer reads against the same forbidden ranges as ordinary address-space reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte if the address is outside all guarded ranges.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects accesses to compiled collectible payloads and retail upload lists, delegating other reads.</summary>
        /// <param name="address">Cartridge address to check and, if allowed, read.</param>
        /// <returns>The underlying byte for a permitted address.</returns>
        public byte ReadByte(int address)
        {
            foreach (RoomPlmDynamicCollectibleGraphic graphic in
                     RoomPlmDynamicCollectibleGraphicsDefinitions.All)
            {
                int first = 0x890000 | graphic.GraphicsPointer;
                if (address >= first && address < first + graphic.Tiles.Length)
                    return Reject(address);
                int kind = (int)graphic.Kind;
                foreach (ushort baseHeader in new ushort[]
                         { RoomPlmHeaders.ExposedEnergyTank,
                           RoomPlmHeaders.ChozoEnergyTank,
                           RoomPlmHeaders.ShotBlockEnergyTank })
                {
                    ushort header = checked((ushort)(baseHeader + kind * 4));
                    ushort instruction = ReadCollectibleGraphicsWord(source, header + 2);
                    int instructionAddress = 0x840000 | instruction;
                    if (address >= instructionAddress &&
                        address < instructionAddress + 12)
                        return Reject(address);
                }
            }
            return source.ReadByte(address);
        }

        /// <summary>Records a forbidden retail read attempt and fails the verification immediately.</summary>
        /// <param name="address">Address that should have been supplied by compiled collectible data.</param>
        /// <returns>This method never returns.</returns>
        private byte Reject(int address)
        {
            ForbiddenReadAttempts++;
            throw new InvalidOperationException(
                $"Retail collectible reread compiled graphics byte ${address:X6}.");
        }

        /// <summary>Passes writes directly to the wrapped address space; the guard restricts reads only.</summary>
        /// <param name="address">Destination cartridge address.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
