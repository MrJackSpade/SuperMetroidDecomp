using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCpuOperandOpenBus()
    {
        var bus = new OperandReadWitness();
        foreach (byte bank in new byte[] { 0, 0x3f, 0x80, 0x9b, 0xbf })
        {
            AssertEqual((ushort)0, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0, 0x21db), "absolute zero operand drives zero on reserved bus");
            AssertEqual((ushort)0x2121, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0x2100, 0xdb), "nonzero operand drives actual MDR, not a constant fallback");
            AssertEqual((ushort)0xabab, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0, 0x2183), "low data byte drives following undriven high-byte read");
            foreach (ushort offset in new ushort[] { 0x21ff, 0x2200, 0x220b, 0x223b, 0x226b, 0x229b, 0x22cb, 0x22fb, 0x3ffe })
                AssertEqual((ushort)0, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0, offset), "unpopulated expansion reads preserve operand MDR across the complete reflected direction set");
            AssertEqual((ushort)0x2222, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0x2200, 0x6b), "expansion open bus preserves nonzero operand MDR");
            AssertEqual((ushort)0xab00, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0, 0x3fff), "expansion ends before real CPU register window");
        }
        foreach (byte bank in new byte[] { 0x40, 0x7e, 0x7f, 0xc0, 0xff })
        {
            AssertEqual((ushort)0xabab, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0, 0x21db), "same offset outside mirrored banks remains mapped");
            AssertEqual((ushort)0xabab, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0, 0x226b), "expansion offsets outside system banks remain mapped");
        }
        AssertEqual((ushort)0xabab, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, 0x9b, 0, 0x2140), "real APU registers are not silently treated as open bus");
    }

    private sealed class OperandReadWitness : ISnesAddressSpace
    {
        public byte ReadByte(int address) => 0xab;
        public void WriteByte(int address, byte value) => throw new NotSupportedException();
    }
}
