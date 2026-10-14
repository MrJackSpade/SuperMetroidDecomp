using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.IntegrationVerification.Installation;
using SuperMetroid.AssetExtraction.Compression;

/// <summary>Checks typed cartridge reads and compressed-stream framing across a LoROM bank edge.</summary>
internal static class RomDecompressionVerification
{
    /// <summary>Compares every compression command family and verifies exact source extent and allocation bounds.</summary>
    [CpuBusBoundary("exact bus test: RomDataReader fixed-bank, word, long and compressed-stream framing reads exactly the declared LoROM bytes")]
    public static int Run()
    {
        // Four maximum-length literal runs full of terminator-valued payload bytes.
        // The reader must not repeatedly decode each prefix to find the true terminator.
        var encoded = new List<byte>();
        for (int run = 0; run < 4; run++)
        {
            encoded.Add(0xe3);
            encoded.Add(0xff);
            encoded.AddRange(Enumerable.Repeat((byte)0xff, 1024));
        }
        encoded.Add(SmCompressionCodes.Terminator);
        byte[] bytes = encoded.ToArray();
        var bus = new StreamBus(bytes);
        RomDataReader.Decompress(new StreamBus([0, 1, 0xff]), StreamBus.Start, 3);
        var fixedBankBus = new StreamBus([0x12, 0x34, 0x56, 0x78]);
        if (!RomDataReader.ReadFixedBank(fixedBankBus, StreamBus.Start, 4)
                .SequenceEqual(new byte[] { 0x12, 0x34, 0x56, 0x78 }) ||
            fixedBankBus.Reads != 4)
            throw new InvalidDataException("Fixed-bank import did not use the typed cartridge source.");
        RejectRange(() => RomDataReader.ReadFixedBank(
            new StreamBus([0x12, 0x34, 0x56, 0x78, 0x9a]), StreamBus.Start, 5));
        var wordBus = new StreamBus([0x12, 0x34]);
        if (RomDataReader.ReadWordFixedBank(wordBus, StreamBus.Start) != 0x3412 ||
            wordBus.Reads != 2)
            throw new InvalidDataException("16-bit word import did not use the typed cartridge source.");
        RejectRange(() => RomDataReader.ReadWordFixedBank(new StreamBus([0x12, 0x34]),
            StreamBus.Start + 3));
        var longPointerBus = new StreamBus([0x12, 0x34, 0x56]);
        if (RomDataReader.ReadLongFixedBank(longPointerBus, StreamBus.Start) != 0x563412 ||
            longPointerBus.Reads != 3)
            throw new InvalidDataException("24-bit pointer import did not use the typed cartridge source.");
        RejectRange(() => RomDataReader.ReadLongFixedBank(
            new StreamBus([0x12, 0x34, 0x56]), StreamBus.Start + 2));
        long before = GC.GetAllocatedBytesForCurrentThread();
        byte[] output = RomDataReader.Decompress(bus, StreamBus.Start, bytes.Length);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (output.Length != 4096 || output.Any(value => value != 0xff) || bus.Reads != bytes.Length)
            throw new InvalidDataException("Literal terminators or LoROM bank crossing changed decoded bytes/read extent.");
        Console.WriteLine($"Dense literal ROM stream: {allocated:N0} allocated bytes.");
        if (allocated > 100_000)
            throw new InvalidDataException("ROM framing repeatedly decodes payload terminator candidates.");

        byte[][] streams = [
            [0xff], [0, 0xff, 0xff], [0x23, 0xff, 0xff],
            [0x43, 0xff, 0x12, 0xff], [0x63, 0xff, 0xff],
            [1, 0x12, 0x34, 0x83, 0, 0, 0xff],
            [1, 0x12, 0x34, 0xa3, 0, 0, 0xff],
            [1, 0x12, 0x34, 0xc3, 2, 0xff],
            [1, 0x12, 0x34, 0xfc, 3, 2, 0xff]
        ];
        foreach (byte[] stream in streams)
        {
            var exact = new StreamBus(stream);
            if (!RomDataReader.Decompress(exact, StreamBus.Start, stream.Length)
                    .SequenceEqual(SmCompression.Decompress(stream)) || exact.Reads != stream.Length)
                throw new InvalidDataException("Framed ROM decode differs from the checked asset decoder.");
            for (int cap = 1; cap < stream.Length; cap++)
                Reject(() => RomDataReader.Decompress(new StreamBus(stream), StreamBus.Start, cap));
        }
        Reject(() => RomDataReader.Decompress(new StreamBus([0x80, 0, 0, 0xff]), StreamBus.Start, 4));
        Reject(() => RomDataReader.Decompress(new StreamBus([0x21, 1, 0xff]), StreamBus.Start, 3, 1));
        Console.WriteLine("PASS framing: every command family, payload FF, truncation, invalid copy, output cap, exact read extent and LoROM crossing.");
        return 0;
    }

    /// <summary>Requires malformed or truncated compressed data to fail within the verifier.</summary>
    private static void Reject(Action action)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidDataException("Malformed/truncated compressed stream was accepted.");
    }

    /// <summary>Requires fixed-bank imports to reject reads crossing into the mutable low window.</summary>
    private static void RejectRange(Action action)
    {
        try { action(); }
        catch (ArgumentOutOfRangeException) { return; }
        throw new InvalidDataException("Fixed-bank cartridge import crossed into a mutable low window.");
    }

    /// <summary>Tracks sequential LoROM byte reads and rejects reads beyond the declared fixture stream.</summary>
    [CpuBusBoundary("exact bus test: witnesses LoROM traversal order and read extent of a cartridge stream")]
    private sealed class StreamBus(byte[] bytes) : ISnesAddressSpace, IImportCartridgeSource
    {
        // Deliberately cross xx:FFFF -> (xx+1):8000 inside the first literal run.
        /// <summary>Begins four bytes before a bank boundary to exercise the native wrap.</summary>
        public const int Start = 0x94fffc;
        /// <summary>Next cartridge bus address expected after the previous fixture read.</summary>
        private SnesAddress next = SnesAddress.FromBusAddress(Start);
        /// <summary>Number of source bytes consumed by the reader.</summary>
        public int Reads { get; private set; }
        /// <summary>Rejects the untyped byte API so import code must use cartridge reads.</summary>
        public static byte ReadByte(int address) => throw new InvalidOperationException("Decompression must use the typed cartridge source.");
        /// <summary>Advances through the supplied bytes only in native LoROM order.</summary>
        public byte ReadCartridgeByte(int address)
        {
            if (address != (int)next || Reads >= bytes.Length)
                throw new InvalidDataException("Reader crossed the stream boundary or changed LoROM traversal.");
            next = next.NextLoRomByte();
            return bytes[Reads++];
        }
        /// <summary>Rejects writes because the decompression source is immutable.</summary>
        public void WriteByte(int address, byte value) => throw new NotSupportedException();
    }
}
