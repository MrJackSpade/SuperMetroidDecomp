using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Cartridge;

/// <summary>Checks the test-support WRAM cell primitives' addressing, wrapping, bounds and write order.</summary>
/// <remarks>
/// The cells run on the sparse test bus, which stores each byte at the exact address written. The work-RAM image whose
/// low-window mirrors a former check read back is gone (#1688): nothing in the game or the import maps work RAM, so the
/// mirror mapping is no longer asserted; the cells' own fixed-bank wrap still is.
/// </remarks>
internal static class WorkRamCellVerification
{
    /// <summary>
    /// Checks the cell primitives' wrapping, bounds and write order on the sparse test bus.
    /// </summary>
    internal static void VerifyWorkRamCells()
    {
        Suite(nameof(VerifyWramWordBankWrap), VerifyWramWordBankWrap);
        Suite(nameof(VerifyCpuBusTableBounds), VerifyCpuBusTableBounds);
        Suite(nameof(VerifyCpuBusCellShapeRejection), VerifyCpuBusCellShapeRejection);
        Suite(nameof(VerifyCpuBusWritesLowByteFirst), VerifyCpuBusWritesLowByteFirst);
        Console.WriteLine("WRAM cell primitives verified.");
    }

    /// <summary>A fixed-bank word at $xx:FFFF takes its high byte from $xx:0000, never the next bank.</summary>
    private static void VerifyWramWordBankWrap()
    {
        var bus = new TestAddressSpace();
        var word = new WramWord(bus, CpuBusCellConstants.WramWord("WrapProbe", 0x7effff));
        word.Write(0x1234);
        AssertEqual((byte)0x34, bus.ReadWorkRamByte(0x7effff), "low byte stays at $7E:FFFF");
        AssertEqual((byte)0x12, bus.ReadWorkRamByte(0x7e0000), "high byte wraps to $7E:0000");
        AssertEqual((byte)0x00, bus.ReadWorkRamByte(0x7f0000), "the next bank is untouched");
        AssertEqual((ushort)0x1234, word.Read(), "the wrapped word reads back");

        var table = new WramWords(bus, CpuBusCellConstants.WramWords("WrapTable", 0x7ffffe, 2));
        table[1].Write(0xbeef);
        AssertEqual((byte)0xef, bus.ReadWorkRamByte(0x7f0000), "table element past $7F:FFFF starts at $7F:0000");
        AssertEqual((byte)0xbe, bus.ReadWorkRamByte(0x7f0001), "table element high byte follows within the bank");
        AssertEqual((ushort)0xbeef, table[1].Read(), "wrapped table element reads back");
    }

    /// <summary>Tables and ranges reject indexes outside their cell.</summary>
    private static void VerifyCpuBusTableBounds()
    {
        var bus = new TestAddressSpace();
        var words = new WramWords(bus, CpuBusCellConstants.WramWords("BoundWords", 0x7e1000, 3));
        AssertEqual(3, words.Count, "word table count");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = words[-1], "word table rejects index -1");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = words[3], "word table rejects index Count");
        var bytes = new WramBytes(bus, CpuBusCellConstants.WramBytes("BoundBytes", 0x7e1000, 4));
        AssertThrows<ArgumentOutOfRangeException>(() => bytes.Read(-1), "byte range rejects index -1");
        AssertThrows<ArgumentOutOfRangeException>(() => bytes.Read(4), "byte range rejects index Length");
        AssertThrows<ArgumentOutOfRangeException>(() => bytes.Write(4, 1), "byte range rejects a write at Length");
        AssertThrows<ArgumentException>(() => bytes.Write(new byte[3]), "byte range rejects a short whole-range write");
    }

    /// <summary>Primitives reject a cell of the wrong store or width.</summary>
    private static void VerifyCpuBusCellShapeRejection()
    {
        var bus = new TestAddressSpace();
        CpuBusCell wramByte = CpuBusCellConstants.WramByte("ShapeByte", 0x7e1000);
        CpuBusCell wramWord = CpuBusCellConstants.WramWord("ShapeWord", 0x7e1000);
        AssertThrows<ArgumentException>(() => new WramWord(bus, wramByte), "a word primitive rejects a byte cell");
        AssertThrows<ArgumentException>(() => new WramByte(bus, wramWord), "a byte primitive rejects a word cell");
        AssertThrows<ArgumentException>(() => new WramWords(bus, CpuBusCellConstants.WramBytes("Odd", 0x7e1000, 3)),
            "a word table rejects an odd byte count");
        AssertThrows<ArgumentOutOfRangeException>(() => CpuBusCellConstants.WramByte("Wide", 0x1000000), "a cell address must fit in 24 bits");
        AssertThrows<ArgumentOutOfRangeException>(() => CpuBusCellConstants.WramBytes("Empty", 0x7e1000, 0), "a cell holds at least one byte");
    }

    /// <summary>Every word write stores its low byte before its high byte.</summary>
    private static void VerifyCpuBusWritesLowByteFirst()
    {
        var bus = new WriteOrderRecordingBus();
        new WramWord(bus, CpuBusCellConstants.WramWord("Order", 0x7e1000)).Write(0xabcd);
        new WramWords(bus, CpuBusCellConstants.WramWords("OrderTable", 0x7e2000, 2))[1].Write(0x1234);
        AssertTrue(bus.Writes.SequenceEqual(new RecordedBusWrite[]
            {
                new RecordedBusWrite(0x7e1000, 0xcd), new RecordedBusWrite(0x7e1001, 0xab),
                new RecordedBusWrite(0x7e2002, 0x34), new RecordedBusWrite(0x7e2003, 0x12),
            }), "word writes are low byte then high byte, in call order");
    }

    /// <summary>A sparse test bus that records each write in order.</summary>
    internal sealed class WriteOrderRecordingBus : ISnesMemoryBus
    {
        /// <summary>Backing sparse bus that keeps each recorded write readable.</summary>
        private readonly TestAddressSpace inner = new();

        /// <summary>CPU addresses and bytes in the order they reached the bus.</summary>
        public List<RecordedBusWrite> Writes { get; } = [];

        /// <summary>Records the requested write before forwarding it to the backing address space.</summary>
        public void WriteByte(int address, byte value)
        {
            Writes.Add(new RecordedBusWrite(address, value));
            inner.WriteByte(address, value);
        }

        /// <summary>Reads back a recorded write at its exact address.</summary>
        public byte ReadWorkRamByte(int cpuAddress) => inner.ReadWorkRamByte(cpuAddress);
    }

    /// <summary>Runs the WRAM cell primitive suite from the verification command.</summary>
    [VerificationCommand("--work-ram-cells")]
    internal static int WorkRamCellsCommand()
    {
        Suite(nameof(VerifyWorkRamCells), () => VerifyWorkRamCells());
        return 0;
    }
}
