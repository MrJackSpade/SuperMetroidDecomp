using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks typed import palette routing across cartridge, work-RAM and unmapped regions, SRAM windows included.</summary>
internal static class DmaSourceRoutingVerification
{
    /// <summary>
    /// Checks the import palette path, including a fixed-bank DMA wrap that changes mapped source type between
    /// consecutive bytes. A generic bus read throws here.
    /// </summary>
    /// <remarks>
    /// The wrap from $80:FFFF into the bank's work-RAM mirror once read a work-RAM byte. The import maps no work RAM
    /// (#1688), so the check now asserts that the wrapped transfer is rejected after its cartridge byte.
    /// </remarks>
    internal static void VerifyDmaSourceRouting()
    {
        var source = new DmaReadRoutingBus();
        source.SetCartridge(0x80fffd, 0x11);
        source.SetCartridge(0x80fffe, 0x22);
        source.SetCartridge(0x80ffff, 0x33);

        var cgram = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x80fffd, colorCount: 1);
        AssertEqual(0x2211, cgram.Colors[0], "CGRAM word reads two cartridge bytes");
        AssertThrows<InvalidOperationException>(
            () => SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x80ffff, colorCount: 1, destinationIndex: 1),
            "CGRAM rejects a word that wraps from the cartridge into the work-RAM mirror");
        AssertThrows<InvalidOperationException>(
            () => SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x700010, colorCount: 1, destinationIndex: 1),
            "CGRAM rejects an SRAM window source");

        AssertEqual(3, source.CartridgeReads, "only import-time palette transfer reads cartridge");
        AssertThrows<InvalidOperationException>(
            () => SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(cgram, source, 0x802000, colorCount: 1),
            "CGRAM rejects unmapped hardware source");

        Console.WriteLine("  Import palette sources: the importer reads the cartridge, and work-RAM, SRAM and hardware windows are rejected.");
    }

    /// <summary>A sparse cartridge store and read counter reveal which source API the palette import uses.</summary>
    internal sealed class DmaReadRoutingBus : ISnesAddressSpace,
        IImportCartridgeSource
    {
        /// <summary>Cartridge bytes available only to import-time palette transfer.</summary>
        private readonly Dictionary<int, byte> cartridge = [];

        /// <summary>Counts cartridge reads made by the palette importer.</summary>
        internal int CartridgeReads { get; private set; }

        /// <summary>Sets a cartridge source byte for an import-time DMA scenario.</summary>
        internal void SetCartridge(int address, byte value) => cartridge.Add(address, value);

        /// <summary>Fails if a PPU path bypasses the typed cartridge or mutable-memory route.</summary>
        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"PPU DMA used an untyped CPU read at ${address:X6}.");

        /// <summary>Returns a cartridge byte and records import-time source access.</summary>
        public byte ReadCartridgeByte(int address)
        {
            CartridgeReads++;
            return cartridge[address];
        }

    }
}
