using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks typed DMA source routing across cartridge, WRAM and unmapped regions, SRAM windows included.</summary>
internal static class DmaSourceRoutingVerification
{
    /// <summary>
    /// Checks the actual PPU transfer paths, including a fixed-bank DMA wrap that changes
    /// mapped source type between consecutive bytes. A generic bus read throws here.
    /// </summary>
    internal static void VerifyDmaSourceRouting()
    {
        var source = new DmaReadRoutingBus();
        source.SetCartridge(0x80fffe, 0x11);
        source.SetCartridge(0x80ffff, 0x22);
        source.SetWorkRam(0x800000, 0x33);
        source.SetWorkRam(0x800001, 0x44);

        var cgram = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x80ffff, colorCount: 1);
        AssertEqual(0x3322, cgram.Colors[0],
            "CGRAM word straddles cartridge and WRAM mirror");
        AssertThrows<InvalidOperationException>(
            () => SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x700010, colorCount: 1, destinationIndex: 1),
            "CGRAM rejects an SRAM window source");

        AssertEqual(1, source.CartridgeReads, "only import-time palette transfer reads cartridge");
        AssertEqual(1, source.WorkRamReads, "only WRAM windows use mutable reads");
        AssertThrows<InvalidOperationException>(
            () => SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x802000, colorCount: 1),
            "CGRAM rejects unmapped hardware source");

        Console.WriteLine("  Import palette sources: the importer resolves the ROM/WRAM wrap, and SRAM and hardware windows are unmapped.");
    }

    /// <summary>Sparse region stores and read counters reveal which source API each PPU transfer uses.</summary>
    internal sealed class DmaReadRoutingBus : ISnesMemoryBus,
        IImportCartridgeSource
    {
        /// <summary>Cartridge bytes available only to import-time palette transfer.</summary>
        private readonly Dictionary<int, byte> cartridge = [];
        /// <summary>Work RAM bytes supplied through typed mutable-memory reads.</summary>
        private readonly Dictionary<int, byte> workRam = [];

        /// <summary>Counts cartridge reads made by the palette importer.</summary>
        internal int CartridgeReads { get; private set; }
        /// <summary>Counts work RAM reads made by the transfer paths.</summary>
        internal int WorkRamReads { get; private set; }

        /// <summary>Sets a cartridge source byte for an import-time DMA scenario.</summary>
        internal void SetCartridge(int address, byte value) => cartridge.Add(address, value);
        /// <summary>Sets a work RAM source byte for a runtime DMA scenario.</summary>
        internal void SetWorkRam(int address, byte value) => workRam.Add(address, value);

        /// <summary>Fails if a PPU path bypasses the typed cartridge or mutable-memory route.</summary>
        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"PPU DMA used an untyped CPU read at ${address:X6}.");

        /// <summary>Returns a cartridge byte and records import-time source access.</summary>
        public byte ReadCartridgeByte(int address)
        {
            CartridgeReads++;
            return cartridge[address];
        }

        /// <summary>Returns a work RAM byte and records mutable-memory source access.</summary>
        public byte ReadWorkRamByte(int address)
        {
            WorkRamReads++;
            return workRam[address];
        }

        /// <summary>Rejects writes because the routing witness only supplies DMA source bytes.</summary>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            "The PPU DMA routing fixture is read-only.");
    }
}
