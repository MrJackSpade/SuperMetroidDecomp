using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class CpuBusBoundaryVerification
{
    /// <summary>
    /// Checks the #629 CPU-bus boundary: catalog identities and aliases, the repository inventory,
    /// and the cell primitives' mapping, mirroring, wrapping, bounds and write order on a real
    /// <see cref="SuperMetroidAddressSpace"/>.
    /// </summary>
    [CpuBusBoundary("exact bus test: suite driver over the catalog inventory")]
    internal static void VerifyCpuBusBoundary()
    {
        IReadOnlyList<CpuBusCatalogEntry> entries = CpuBusCatalogInventory.Enumerate(CpuBusCatalogInventory.ProductionAssemblies);
        Suite(nameof(VerifyCpuBusCatalogIdentities), () => VerifyCpuBusCatalogIdentities(entries));
        Suite(nameof(VerifyCpuBusAliasRoundTrips), () => VerifyCpuBusAliasRoundTrips(entries));
        Suite(nameof(VerifyCpuBusInventoryCurrent), () => VerifyCpuBusInventoryCurrent(entries));
        Suite(nameof(VerifyWramWordBankWrap), VerifyWramWordBankWrap);
        Suite(nameof(VerifyCpuBusTableBounds), VerifyCpuBusTableBounds);
        Suite(nameof(VerifyCpuBusCellShapeRejection), VerifyCpuBusCellShapeRejection);
        Suite(nameof(VerifyWramLowWindowMirror), VerifyWramLowWindowMirror);
        Suite(nameof(VerifySramWindowMapping), VerifySramWindowMapping);
        Suite(nameof(VerifyCpuBusWritesLowByteFirst), VerifyCpuBusWritesLowByteFirst);
        Suite(nameof(VerifyCpuBusTypedRoundTrips), VerifyCpuBusTypedRoundTrips);
        Console.WriteLine($"CPU-bus boundary: {entries.Count} catalog cells, " +
            $"{entries.Count(entry => entry.Cell.AliasOf is not null)} declared aliases, primitives and views verified.");
    }

    /// <summary>Every (owner, name) is unique and bytes are shared only through a declared alias.</summary>
    [CpuBusBoundary("exact bus test: catalog identity and alias rules")]
    private static void VerifyCpuBusCatalogIdentities(IReadOnlyList<CpuBusCatalogEntry> entries)
    {
        AssertTrue(entries.Count > 0, "production catalogs declare CPU-bus cells");
        AssertTrue(entries.GroupBy(entry => (entry.Owner.FullName, entry.Cell.Name)).All(group => group.Count() == 1),
            "every (owner, name) cell identity is unique");
        IReadOnlyList<string> violations = CpuBusCatalogInventory.Violations(entries);
        AssertTrue(violations.Count == 0, "catalog rules hold:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    /// <summary>Each alias pair shares its overlapping bytes: a write through either cell reads back through the other.</summary>
    [CpuBusBoundary("exact bus test: alias coherence on the real address space")]
    private static void VerifyCpuBusAliasRoundTrips(IReadOnlyList<CpuBusCatalogEntry> entries)
    {
        int pairs = 0;
        foreach (CpuBusCatalogEntry entry in entries)
        {
            if (entry.Cell.AliasOf is not { } target)
                continue;
            AssertTrue(CpuBusCatalogInventory.Overlaps(entry.Cell, target),
                $"{entry.Reference} shares bytes with the cell it aliases");
            if (entry.Cell.Region == CpuBusRegion.Cartridge)
                continue;
            RoundTrip(entry.Cell, target, seed: 0x5a, $"{entry.Reference} -> alias target");
            RoundTrip(target, entry.Cell, seed: 0xa5, $"alias target -> {entry.Reference}");
            pairs++;
        }
        AssertTrue(pairs > 0, "at least one mutable alias pair is round-tripped");

        static void RoundTrip(CpuBusCell writer, CpuBusCell reader, byte seed, string context)
        {
            var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
            int writerStart = CpuBusCatalogInventory.PhysicalStart(writer);
            int readerStart = CpuBusCatalogInventory.PhysicalStart(reader);
            int first = Math.Max(writerStart, readerStart);
            int end = Math.Min(writerStart + writer.ByteCount, readerStart + reader.ByteCount);
            for (int physical = first; physical < end; physical++)
                WriteCellByte(bus, writer, physical - writerStart, (byte)(seed ^ physical * 37));
            for (int physical = first; physical < end; physical++)
                AssertEqual((byte)(seed ^ physical * 37), ReadCellByte(bus, reader, physical - readerStart),
                    $"{context} byte {physical - readerStart}");
        }
    }

    [CpuBusBoundary("exact bus test: byte access through a cell's own primitive")]
    private static void WriteCellByte(ISnesMemoryBus bus, CpuBusCell cell, int index, byte value)
    {
        if (cell.Region == CpuBusRegion.SaveRam)
            new SramBytes(bus, cell).Write(index, value);
        else
            new WramBytes(bus, cell).Write(index, value);
    }

    [CpuBusBoundary("exact bus test: byte access through a cell's own primitive")]
    private static byte ReadCellByte(ISnesMemoryBus bus, CpuBusCell cell, int index) =>
        cell.Region == CpuBusRegion.SaveRam ? new SramBytes(bus, cell).Read(index) : new WramBytes(bus, cell).Read(index);

    /// <summary>The committed inventory lists exactly the catalog cells (consumers may drift between regenerations).</summary>
    [CpuBusBoundary("exact bus test: inventory matches the catalogs")]
    private static void VerifyCpuBusInventoryCurrent(IReadOnlyList<CpuBusCatalogEntry> entries)
    {
        string root = Directory.GetCurrentDirectory();
        using JsonDocument expected = JsonDocument.Parse(CpuBusCatalogInventory.RenderJson(entries, root));
        using JsonDocument actual = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, CpuBusCatalogInventory.InventoryPath)));
        string[] Cells(JsonDocument document) => document.RootElement.GetProperty("cells").EnumerateArray()
            .Select(cell => string.Join('|', new[] { "owner", "name", "region", "address", "byteCount", "encoding", "aliasOf" }
                .Select(field => cell.GetProperty(field).ToString())))
            .ToArray();
        AssertTrue(Cells(expected).SequenceEqual(Cells(actual)),
            $"{CpuBusCatalogInventory.InventoryPath} is current; regenerate it with DebugRunner --generate-cpu-bus-inventory");
    }

    /// <summary>A fixed-bank word at $xx:FFFF takes its high byte from $xx:0000, never the next bank.</summary>
    [CpuBusBoundary("exact bus test: WramWord and WramWords bank wrap")]
    private static void VerifyWramWordBankWrap()
    {
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var word = new WramWord(bus, CpuBusCell.WramWord("WrapProbe", 0x7effff));
        word.Write(0x1234);
        AssertEqual((byte)0x34, bus.ReadWorkRamByte(0x7effff), "low byte stays at $7E:FFFF");
        AssertEqual((byte)0x12, bus.ReadWorkRamByte(0x7e0000), "high byte wraps to $7E:0000");
        AssertEqual((byte)0x00, bus.ReadWorkRamByte(0x7f0000), "the next bank is untouched");
        AssertEqual((ushort)0x1234, word.Read(), "the wrapped word reads back");

        var table = new WramWords(bus, CpuBusCell.WramWords("WrapTable", 0x7ffffe, 2));
        table[1].Write(0xbeef);
        AssertEqual((byte)0xef, bus.ReadWorkRamByte(0x7f0000), "table element past $7F:FFFF starts at $7F:0000");
        AssertEqual((byte)0xbe, bus.ReadWorkRamByte(0x7f0001), "table element high byte follows within the bank");
        AssertEqual((ushort)0xbeef, table[1].Read(), "wrapped table element reads back");
    }

    /// <summary>Tables and ranges reject indexes outside their cell.</summary>
    [CpuBusBoundary("exact bus test: WramWords, WramBytes and SramBytes bounds")]
    private static void VerifyCpuBusTableBounds()
    {
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var words = new WramWords(bus, CpuBusCell.WramWords("BoundWords", 0x7e1000, 3));
        AssertEqual(3, words.Count, "word table count");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = words[-1], "word table rejects index -1");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = words[3], "word table rejects index Count");
        var bytes = new WramBytes(bus, CpuBusCell.WramBytes("BoundBytes", 0x7e1000, 4));
        AssertThrows<ArgumentOutOfRangeException>(() => bytes.Read(-1), "byte range rejects index -1");
        AssertThrows<ArgumentOutOfRangeException>(() => bytes.Read(4), "byte range rejects index Length");
        AssertThrows<ArgumentOutOfRangeException>(() => bytes.Write(4, 1), "byte range rejects a write at Length");
        AssertThrows<ArgumentException>(() => bytes.Write(new byte[3]), "byte range rejects a short whole-range write");
        var sram = new SramBytes(bus, CpuBusCell.SramBytes("BoundSram", 0x700000, 2));
        AssertThrows<ArgumentOutOfRangeException>(() => sram.Read(2), "SRAM range rejects index Length");
        AssertThrows<ArgumentOutOfRangeException>(() => sram.WriteWord(1, 0), "SRAM word cannot straddle the range end");
    }

    /// <summary>Primitives reject a cell of the wrong store or width.</summary>
    [CpuBusBoundary("exact bus test: region and width mismatch rejection")]
    private static void VerifyCpuBusCellShapeRejection()
    {
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        CpuBusCell wramByte = CpuBusCell.WramByte("ShapeByte", 0x7e1000);
        CpuBusCell wramWord = CpuBusCell.WramWord("ShapeWord", 0x7e1000);
        CpuBusCell sram = CpuBusCell.SramBytes("ShapeSram", 0x700000, 2);
        AssertThrows<ArgumentException>(() => new WramWord(bus, wramByte), "a word primitive rejects a byte cell");
        AssertThrows<ArgumentException>(() => new WramByte(bus, wramWord), "a byte primitive rejects a word cell");
        AssertThrows<ArgumentException>(() => new WramWord(bus, sram), "a WRAM primitive rejects an SRAM cell");
        AssertThrows<ArgumentException>(() => new WramBytes(bus, sram), "a WRAM range rejects an SRAM cell");
        AssertThrows<ArgumentException>(() => new SramBytes(bus, wramWord), "an SRAM range rejects a WRAM cell");
        AssertThrows<ArgumentException>(() => new WramWords(bus, CpuBusCell.WramBytes("Odd", 0x7e1000, 3)),
            "a word table rejects an odd byte count");
        AssertThrows<ArgumentOutOfRangeException>(() => CpuBusCell.WramByte("Wide", 0x1000000), "a cell address must fit in 24 bits");
        AssertThrows<ArgumentOutOfRangeException>(() => CpuBusCell.WramBytes("Empty", 0x7e1000, 0), "a cell holds at least one byte");
    }

    /// <summary>System banks $00-$3F/$80-$BF below $2000 read and write bank-$7E WRAM; nothing else does.</summary>
    [CpuBusBoundary("exact bus test: low-window WRAM mirror")]
    private static void VerifyWramLowWindowMirror()
    {
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        new WramByte(bus, CpuBusCell.WramByte("Home", 0x7e0123)).Write(0x9c);
        foreach (int mirror in new[] { 0x000123, 0x3f0123, 0x800123, 0xbf0123 })
            AssertEqual((byte)0x9c, new WramByte(bus, CpuBusCell.WramByte("Mirror", mirror)).Read(),
                $"${SnesAddressMath.BankNumber(mirror):X2}:0123 reads bank-$7E WRAM");
        new WramWord(bus, CpuBusCell.WramWord("MirrorWord", 0x801ffe)).Write(0x5a6b);
        AssertEqual((ushort)0x5a6b, new WramWord(bus, CpuBusCell.WramWord("HomeWord", 0x7e1ffe)).Read(),
            "a mirror write lands in bank-$7E WRAM");
        AssertThrows<ArgumentOutOfRangeException>(() => new WramByte(bus, CpuBusCell.WramByte("PastWindow", 0x002000)).Read(),
            "offset $2000 of a system bank is not WRAM");
        AssertThrows<ArgumentOutOfRangeException>(() => new WramByte(bus, CpuBusCell.WramByte("NonSystem", 0x400123)).Read(),
            "bank $40 has no low-window mirror");
    }

    /// <summary>SRAM banks $70-$7D/$F0-$FF below $8000 all reach the one 8-KiB image.</summary>
    [CpuBusBoundary("exact bus test: SRAM window mapping")]
    private static void VerifySramWindowMapping()
    {
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        new SramBytes(bus, CpuBusCell.SramBytes("Home", 0x700010, 2)).WriteWord(0, 0xc3d4);
        foreach (int window in new[] { 0xf00010, 0x7d0010, 0xff0010, 0x702010, 0x706010 })
            AssertEqual((ushort)0xc3d4, new SramBytes(bus, CpuBusCell.SramBytes("Window", window, 2)).ReadWord(0),
                $"${SnesAddressMath.BankNumber(window):X2}:{SnesAddressMath.OffsetInBank(window):X4} reads the same SRAM bytes");
        AssertEqual((byte)0xd4, SaveRamImage.Capture(bus)[0x10], "the SRAM image codec sees the window write");
        AssertThrows<ArgumentOutOfRangeException>(() => new SramBytes(bus, CpuBusCell.SramBytes("Rom", 0x708000, 1)).Read(0),
            "offset $8000 of an SRAM bank is cartridge ROM");
        AssertThrows<InvalidOperationException>(() => new SramBytes(bus, CpuBusCell.SramBytes("Rom", 0x708000, 1)).Write(0, 1),
            "a write to cartridge ROM is rejected");
    }

    /// <summary>Every word write stores its low byte before its high byte.</summary>
    [CpuBusBoundary("exact bus test: write ordering")]
    private static void VerifyCpuBusWritesLowByteFirst()
    {
        var bus = new WriteOrderRecordingBus();
        new WramWord(bus, CpuBusCell.WramWord("Order", 0x7e1000)).Write(0xabcd);
        new WramWords(bus, CpuBusCell.WramWords("OrderTable", 0x7e2000, 2))[1].Write(0x1234);
        new SramBytes(bus, CpuBusCell.SramBytes("OrderSram", 0x700020, 2)).WriteWord(0, 0x5678);
        AssertTrue(bus.Writes.SequenceEqual(new (int, byte)[]
            {
                (0x7e1000, 0xcd), (0x7e1001, 0xab),
                (0x7e2002, 0x34), (0x7e2003, 0x12),
                (0x700020, 0x78), (0x700021, 0x56),
            }), "word writes are low byte then high byte, in call order");
    }

    /// <summary>Representative typed domain views round-trip their values through the bus.</summary>
    [CpuBusBoundary("exact bus test: representative typed view round trips")]
    private static void VerifyCpuBusTypedRoundTrips()
    {
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var mirror = new SaveMirrorMemory(bus);
        mirror.LoadingGameState = 0x0022;
        mirror.LoadedItemCount = 0x1234;
        mirror.Events.CopyFrom([1, 2, 3, 4, 5, 6, 7, 8]);
        AssertEqual((ushort)0x0022, mirror.LoadingGameState, "saved dispatcher word round-trips");
        AssertEqual((ushort)0x1234, new WramWord(bus, SaveMirrorWorkRam.LoadedItemCount).Read(), "loaded-item count is a $7E:D91A word");
        AssertEqual((byte)8, bus.ReadWorkRamByte(SaveMirrorWorkRam.Events.Address + 7), "event bitset lands at $7E:D820");

        var fireflea = new FirefleaFxMemory(bus);
        fireflea.Timer = 0x0102;
        AssertEqual((ushort)0x0102, fireflea.Timer, "Fireflea timer round-trips");
        AssertEqual((byte)0x02, bus.ReadWorkRamByte(0x7e1778), "Fireflea timer is the $7E:1778 word through its mirror");

        var scroll = new RoomScrollMemory(bus);
        bus.WriteByte(0x7ecd52, 0x77);
        AssertEqual((byte)0x77, scroll.ReadIndexed(RoomScrollGrid.StorageByteCount), "scroll index $32 reads the adjacent explored-map byte");

        var scratch = new EnemyScratchMemory(bus);
        AssertThrows<InvalidDataException>(() => scratch.RotTable(0x8ffc, 2), "a rot table must lie inside the rotting heap");
        scratch.CorpseTiles.WriteWord(0x10, 0x4321);
        AssertEqual((ushort)0x4321, scratch.EnemyBg2Tilemap[8], "corpse tiles and the enemy BG2 image share $7E:2000");
    }

    /// <summary>A real address space that records each write in order.</summary>
    [CpuBusBoundary("exact bus test: write-order recording double")]
    internal sealed class WriteOrderRecordingBus : ISnesMemoryBus
    {
        private readonly SuperMetroidAddressSpace inner = SuperMetroidAddressSpace.CreateWithoutCartridge();

        public List<(int Address, byte Value)> Writes { get; } = [];

        public void WriteByte(int address, byte value)
        {
            Writes.Add((address, value));
            inner.WriteByte(address, value);
        }

        public byte ReadWorkRamByte(int cpuAddress) => inner.ReadWorkRamByte(cpuAddress);

        public byte ReadSaveRamByte(int cpuAddress) => inner.ReadSaveRamByte(cpuAddress);
    }

    [VerificationCommand("--cpu-bus-boundary")]
    internal static int CpuBusBoundaryCommand()
    {
        Suite(nameof(VerifyCpuBusBoundary), () => VerifyCpuBusBoundary());
        return 0;
    }
}
