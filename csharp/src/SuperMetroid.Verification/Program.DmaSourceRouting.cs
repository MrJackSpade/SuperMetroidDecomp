using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Checks the actual PPU transfer paths, including a fixed-bank DMA wrap that changes
    /// mapped source type between consecutive bytes. A generic bus read throws here.
    /// </summary>
    private static void VerifyDmaSourceRouting()
    {
        var source = new DmaReadRoutingBus();
        source.SetCartridge(0x80fffe, 0x11);
        source.SetCartridge(0x80ffff, 0x22);
        source.SetWorkRam(0x800000, 0x33);
        source.SetWorkRam(0x800001, 0x44);
        source.SetSaveRam(0x700010, 0x55);
        source.SetSaveRam(0x700011, 0x66);

        var vram = new SnesVram();
        AssertThrows<InvalidOperationException>(
            () => vram.ExecuteQueuedMemoryWrite(source, 0x80fffe, 4, 0),
            "runtime VRAM rejects cartridge transfer sources");
        vram.ExecuteQueuedMemoryWrite(source, 0x800000, 2, 0);
        AssertEqual((byte)0x33, vram.ReadByte(0), "DMA reads WRAM mirror low byte");
        AssertEqual((byte)0x44, vram.ReadByte(1), "DMA reads WRAM mirror high byte");
        vram.ExecuteQueuedMemoryWrite(source, 0x700010, 2, 2);
        AssertEqual((byte)0x55, vram.ReadByte(4), "DMA reads SRAM low byte");
        AssertEqual((byte)0x66, vram.ReadByte(5), "DMA reads SRAM high byte");

        var cgram = new SnesCgram();
        SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x80ffff, colorCount: 1);
        AssertEqual((ushort)0x3322, cgram.Colors[0],
            "CGRAM word straddles cartridge and WRAM mirror");
        SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x700010, colorCount: 1, destinationIndex: 1);
        AssertEqual((ushort)0x6655, cgram.Colors[1], "CGRAM reads SRAM word");

        AssertEqual(1, source.CartridgeReads, "only import-time palette transfer reads cartridge");
        AssertEqual(3, source.WorkRamReads, "only WRAM windows use mutable reads");
        AssertEqual(4, source.SaveRamReads, "both PPU paths use SRAM reads");
        AssertThrows<InvalidOperationException>(
            () => vram.ExecuteQueuedMemoryWrite(source, 0x806000, 1, 0),
            "VRAM rejects unmapped expansion source");
        AssertThrows<InvalidOperationException>(
            () => SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x802000, colorCount: 1),
            "CGRAM rejects unmapped hardware source");

        Console.WriteLine("  PPU DMA sources: runtime rejects ROM, importer resolves ROM/WRAM wrap, and SRAM uses typed routes.");
    }

    /// <summary>Routes fixture bytes through distinct cartridge, work-RAM, and save-RAM read APIs.</summary>
    private sealed class DmaReadRoutingBus : ISnesAddressSpace, ISnesMutableMemory,
        IImportCartridgeSource
    {
        /// <summary>Fixture bytes keyed by cartridge bus address.</summary>
        private readonly Dictionary<int, byte> cartridge = [];
        /// <summary>Fixture bytes keyed by work-RAM CPU address.</summary>
        private readonly Dictionary<int, byte> workRam = [];
        /// <summary>Fixture bytes keyed by save-RAM CPU address.</summary>
        private readonly Dictionary<int, byte> saveRam = [];

        /// <summary>Number of typed cartridge reads performed by the fixture.</summary>
        internal int CartridgeReads { get; private set; }
        /// <summary>Number of work-RAM reads performed by the fixture.</summary>
        internal int WorkRamReads { get; private set; }
        /// <summary>Number of save-RAM reads performed by the fixture.</summary>
        internal int SaveRamReads { get; private set; }

        /// <summary>Adds a byte that can be returned by a typed cartridge read.</summary>
        /// <param name="address">Cartridge bus address.</param>
        /// <param name="value">Byte to return at that address.</param>
        internal void SetCartridge(int address, byte value) => cartridge.Add(address, value);

        /// <summary>Adds a byte that can be returned by a work-RAM read.</summary>
        /// <param name="address">Work-RAM CPU address.</param>
        /// <param name="value">Byte to return at that address.</param>
        internal void SetWorkRam(int address, byte value) => workRam.Add(address, value);

        /// <summary>Adds a byte that can be returned by a save-RAM read.</summary>
        /// <param name="address">Save-RAM CPU address.</param>
        /// <param name="value">Byte to return at that address.</param>
        internal void SetSaveRam(int address, byte value) => saveRam.Add(address, value);

        /// <summary>Fails any untyped CPU-bus read so callers must select the mapped memory source.</summary>
        /// <param name="address">CPU address whose generic read was attempted.</param>
        /// <returns>This fixture never returns a value; it always throws.</returns>
        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"PPU DMA used an untyped CPU read at ${address:X6}.");

        /// <summary>Returns the fixture cartridge byte and records use of the cartridge route.</summary>
        /// <param name="address">Cartridge bus address.</param>
        /// <returns>The byte stored for that address.</returns>
        public byte ReadCartridgeByte(int address)
        {
            CartridgeReads++;
            return cartridge[address];
        }

        /// <summary>Returns the fixture work-RAM byte and records use of the work-RAM route.</summary>
        /// <param name="address">Work-RAM CPU address.</param>
        /// <returns>The byte stored for that address.</returns>
        public byte ReadWorkRamByte(int address)
        {
            WorkRamReads++;
            return workRam[address];
        }

        /// <summary>Returns the fixture save-RAM byte and records use of the save-RAM route.</summary>
        /// <param name="address">Save-RAM CPU address.</param>
        /// <returns>The byte stored for that address.</returns>
        public byte ReadSaveRamByte(int address)
        {
            SaveRamReads++;
            return saveRam[address];
        }

        /// <summary>Fails writes because this DMA-routing fixture models read-only source memory.</summary>
        /// <param name="address">CPU address targeted by the unsupported write.</param>
        /// <param name="value">Byte requested for storage.</param>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            "The PPU DMA routing fixture is read-only.");
    }
}
