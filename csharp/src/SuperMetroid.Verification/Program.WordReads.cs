using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>
/// #223: the canonical word reads and the background format model. Each read takes the low byte at the
/// word's address and the high byte after it, exactly once each and in that order. At a bank's last byte
/// the linear progression continues into the next bank, the fixed-bank progression wraps to the same
/// bank's first byte, and a failing source's exception reaches the caller unchanged.
/// </summary>
internal static partial class Program
{
    private static void VerifyCanonicalWordReads()
    {
        Suite(nameof(VerifyWordReadByteOrder), () => VerifyWordReadByteOrder());
        Suite(nameof(VerifyWordReadBankBoundary), () => VerifyWordReadBankBoundary());
        Suite(nameof(VerifyWordReadFailurePropagation), () => VerifyWordReadFailurePropagation());
        Suite(nameof(VerifyBackgroundFormatModel), () => VerifyBackgroundFormatModel());
    }

    private static void VerifyWordReadByteOrder()
    {
        var bytes = new RecordingByteSource(new() { [0x7e1234] = 0x34, [0x7e1235] = 0x12 });
        AssertEqual((ushort)0x1234, bytes.ReadWorkRamWord(0x7e1234), "WRAM word: low byte first");
        AssertSequenceEqual(new[] { 0x7e1234, 0x7e1235 }, bytes.Reads, "WRAM word: low then high, once each");

        bytes = new RecordingByteSource(new() { [0x8f8000] = 0x78, [0x8f8001] = 0x56, [0x8f8002] = 0xa1 });
        AssertEqual((ushort)0x5678, bytes.ReadCartridgeWord(0x8f8000), "cartridge word: low byte first");
        AssertSequenceEqual(new[] { 0x8f8000, 0x8f8001 }, bytes.Reads, "cartridge word: low then high, once each");

        AssertEqual((ushort)0xbeef, SnesLittleEndian.Word(0xef, 0xbe), "named composition: low then high");
    }

    private static void VerifyWordReadBankBoundary()
    {
        // $8F:FFFF holds the low byte; $90:0000 and $8F:0000 hold different candidate high bytes.
        var bytes = new RecordingByteSource(new()
        {
            [0x8fffff] = 0x11, [0x900000] = 0x22, [0x8f0000] = 0x33,
            [0x7effff] = 0x44, [0x7f0000] = 0x55, [0x7e0000] = 0x66,
            [0x8ffffe] = 0x77, [0x900001] = 0x99, [0x8f0001] = 0xaa,
        });
        AssertEqual((ushort)0x2211, bytes.ReadCartridgeWord(0x8fffff), "linear cartridge word crosses into the next bank");
        AssertEqual((ushort)0x3311, bytes.ReadCartridgeWordFixedBank(0x8fffff), "fixed-bank cartridge word wraps within the bank");
        AssertEqual((ushort)0x5544, bytes.ReadWorkRamWord(0x7effff), "linear WRAM word crosses into bank $7F");
        AssertEqual((ushort)0x6644, bytes.ReadWorkRamWordFixedBank(0x7effff), "fixed-bank WRAM word wraps within bank $7E");
        AssertEqual(0x330077 | 0x11 << 8, bytes.ReadCartridgeLongFixedBank(0x8ffffe), "fixed-bank long wraps within the bank");
        bytes.Reads.Clear();
        AssertEqual(0xa15678, new RecordingByteSource(new() { [0x8f8000] = 0x78, [0x8f8001] = 0x56, [0x8f8002] = 0xa1 })
            .ReadCartridgeLongFixedBank(0x8f8000), "cartridge long: word, then bank byte");

        // Away from the boundary both progressions read the same bytes.
        bytes = new RecordingByteSource(new() { [0x8f8000] = 0x01, [0x8f8001] = 0x02 });
        AssertEqual(bytes.ReadCartridgeWord(0x8f8000), bytes.ReadCartridgeWordFixedBank(0x8f8000), "progressions agree inside a bank");
    }

    private static void VerifyWordReadFailurePropagation()
    {
        // The high byte is unmapped: the source's own exception must reach the caller, after the low read.
        var bytes = new RecordingByteSource(new() { [0x8f8000] = 0x01 });
        var cartridge = AssertThrows<UnmappedByteException>(() => bytes.ReadCartridgeWord(0x8f8000), "cartridge word: high-byte failure propagates");
        AssertEqual(0x8f8001, cartridge.Address, "cartridge word: failure names the high byte");
        bytes = new RecordingByteSource(new() { [0x7e0010] = 0x01 });
        var workRam = AssertThrows<UnmappedByteException>(() => bytes.ReadWorkRamWordFixedBank(0x7e0010), "WRAM word: high-byte failure propagates");
        AssertEqual(0x7e0011, workRam.Address, "WRAM word: failure names the high byte");
        AssertThrows<ArgumentNullException>(() => ((ISnesMutableMemory)null!).ReadWorkRamWord(0x7e0000), "WRAM word: null source");
        AssertThrows<ArgumentNullException>(() => ((IImportCartridgeSource)null!).ReadCartridgeWord(0x8f8000), "cartridge word: null source");
    }

    [PpuLayoutBoundary("exact hardware test: checks the PPU tilemap, character and VRAM word formats")]
    private static void VerifyBackgroundFormatModel()
    {
        // BGSC screen blocks run top-left, top-right, bottom-left, bottom-right, $400 words each.
        var wide = new BgTilemapGeometry(new Extent16<TilemapCells>(64, 32));
        var tall = new BgTilemapGeometry(new Extent16<TilemapCells>(32, 64));
        var four = new BgTilemapGeometry(new Extent16<TilemapCells>(64, 64));
        AssertEqual(0x5000 + 0x0400 + 3 * 32 + 1, wide.EntryWordAddress(0x5000, new Position16<TilemapCells>(33, 3)), "64x32: right screen");
        AssertEqual(0x5000 + 0x0400 + 2 * 32 + 5, tall.EntryWordAddress(0x5000, new Position16<TilemapCells>(5, 34)), "32x64: lower screen");
        AssertEqual(0x4000 + 3 * 0x0400 + 31 * 32 + 31, four.EntryWordAddress(0x4000, new Position16<TilemapCells>(63, 63)), "64x64: bottom-right");
        AssertEqual(0x0000 + 2, wide.EntryWordAddress(0x7ffe, new Position16<TilemapCells>(4, 0)), "entry addresses wrap on the 15-bit VRAM bus");
        AssertEqual(5, wide.WrapX(512 + 5), "64-wide map wraps at 512 pixels");
        AssertEqual(255, wide.WrapY(-1), "32-tall map wraps below zero to its last line");
        AssertThrows<ArgumentOutOfRangeException>(() => new BgTilemapGeometry(new Extent16<TilemapCells>(48, 32)), "tilemaps are one or two screens");

        // One 4-bpp character row with each plane drawing a different column: pixel n's color is 2^plane.
        var vram = new byte[SnesPpuLayout.VramWordCount * SnesVramAddressing.BytesPerWord];
        BgCharacterFormat four4 = BgCharacterFormat.FourBitsPerPixel;
        int row = four4.RowByteOffset(characterBaseWord: 0x1000, character: 2, row: 3);
        AssertEqual((0x1000 + 2 * 16) * 2 + 3 * 2, row, "4-bpp row offset: 16 words per character, one word per row");
        vram[row] = 0b1000_0000;
        vram[row + 1] = 0b0100_0000;
        vram[row + 16] = 0b0010_0000;
        vram[row + 17] = 0b0001_0001;
        AssertSequenceEqual(new[] { 1, 2, 4, 8, 0, 0, 0, 8 },
            Enumerable.Range(0, 8).Select(column => four4.ColorIndexAt(vram, row, column)).ToArray(), "4-bpp planes 0-3 weight 1, 2, 4, 8");
        AssertEqual(7, BgCharacterGrid.SourcePixel(0, flipped: true), "flip mirrors within the character");
        AssertEqual(3 * 16 + 9, four4.CgramColor(3, 9), "4-bpp palettes hold sixteen colors");

        BgCharacterFormat two = BgCharacterFormat.TwoBitsPerPixel;
        int twoRow = two.RowByteOffset(characterBaseWord: 0x4000, character: 1, row: 0);
        AssertEqual((0x4000 + 8) * 2, twoRow, "2-bpp row offset: 8 words per character");
        vram[twoRow] = 0b1100_0000;
        vram[twoRow + 1] = 0b0110_0000;
        AssertSequenceEqual(new[] { 1, 3, 2, 0 },
            Enumerable.Range(0, 4).Select(column => two.ColorIndexAt(vram, twoRow, column)).ToArray(), "2-bpp planes 0-1 weight 1, 2");
        AssertEqual(2 * 4 + 3, two.CgramColor(2, 3), "2-bpp palettes hold four colors");
        vram[0x7fff * 2] = 0x34;
        vram[0x7fff * 2 + 1] = 0x12;
        AssertEqual((ushort)0x1234, SnesVramAddressing.ReadWord(vram, 0xffff), "VRAM word addresses wrap and read low byte first");
    }

    private sealed class UnmappedByteException(int address) : Exception($"No byte at ${address:X6}.")
    {
        public int Address { get; } = address;
    }

    /// <summary>A byte store that records every read and fails at unmapped addresses.</summary>
    [CpuBusBoundary("exact bus test: recording byte source for the canonical word reads' order, boundary and failure contract")]
    private sealed class RecordingByteSource(Dictionary<int, byte> bytes) : ISnesMutableMemory, IImportCartridgeSource
    {
        public List<int> Reads { get; } = [];

        public byte ReadWorkRamByte(int cpuAddress) => Read(cpuAddress);

        public byte ReadSaveRamByte(int cpuAddress) => Read(cpuAddress);

        public byte ReadCartridgeByte(int cpuAddress) => Read(cpuAddress);

        private byte Read(int address)
        {
            Reads.Add(address);
            return bytes.TryGetValue(address, out byte value) ? value : throw new UnmappedByteException(address);
        }
    }
}
