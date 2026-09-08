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

    private sealed class StreamBus(byte[] bytes) : ISnesAddressSpace
    {
        // Deliberately cross xx:FFFF -> (xx+1):8000 inside the first literal run.
        public const int Start = 0x94fffc;
        private SnesAddress next = SnesAddress.FromBusAddress(Start);
        public int Reads { get; private set; }
        public byte ReadByte(int address)
        {
            if (address != (int)next || Reads >= bytes.Length)
                throw new InvalidDataException("Reader crossed the stream boundary or changed LoROM traversal.");
            next = next.NextLoRomByte();
            return bytes[Reads++];
        }
        public void WriteByte(int address, byte value) => throw new NotSupportedException();
    }
}
