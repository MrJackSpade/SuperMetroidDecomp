using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static class RomDecompressionVerification
{
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
        encoded.Add(SmCompressionFormat.Terminator);
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

    private static void Reject(Action action)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidDataException("Malformed/truncated compressed stream was accepted.");
    }

    private static void RejectRange(Action action)
    {
        try { action(); }
        catch (ArgumentOutOfRangeException) { return; }
        throw new InvalidDataException("Fixed-bank cartridge import crossed into a mutable low window.");
    }

    private sealed class StreamBus(byte[] bytes) : ISnesAddressSpace, IImportCartridgeSource
    {
        // Deliberately cross xx:FFFF -> (xx+1):8000 inside the first literal run.
        public const int Start = 0x94fffc;
        private SnesAddress next = SnesAddress.FromBusAddress(Start);
        public int Reads { get; private set; }
        public byte ReadByte(int address) => throw new InvalidOperationException("Decompression must use the typed cartridge source.");
        public byte ReadCartridgeByte(int address)
        {
            if (address != (int)next || Reads >= bytes.Length)
                throw new InvalidDataException("Reader crossed the stream boundary or changed LoROM traversal.");
            next = next.NextLoRomByte();
            return bytes[Reads++];
        }
        public void WriteByte(int address, byte value) => throw new NotSupportedException();
    }
}
