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

    private sealed class DmaReadRoutingBus : ISnesAddressSpace, ISnesMutableMemory,
        IImportCartridgeSource
    {
        private readonly Dictionary<int, byte> cartridge = [];
        private readonly Dictionary<int, byte> workRam = [];
        private readonly Dictionary<int, byte> saveRam = [];

        internal int CartridgeReads { get; private set; }
        internal int WorkRamReads { get; private set; }
        internal int SaveRamReads { get; private set; }

        internal void SetCartridge(int address, byte value) => cartridge.Add(address, value);
        internal void SetWorkRam(int address, byte value) => workRam.Add(address, value);
        internal void SetSaveRam(int address, byte value) => saveRam.Add(address, value);

        public byte ReadByte(int address) => throw new InvalidOperationException(
            $"PPU DMA used an untyped CPU read at ${address:X6}.");

        public byte ReadCartridgeByte(int address)
        {
            CartridgeReads++;
            return cartridge[address];
        }

        public byte ReadWorkRamByte(int address)
        {
            WorkRamReads++;
            return workRam[address];
        }

        public byte ReadSaveRamByte(int address)
        {
            SaveRamReads++;
            return saveRam[address];
        }

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            "The PPU DMA routing fixture is read-only.");
    }
}
