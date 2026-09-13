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
        }
        foreach (byte bank in new byte[] { 0x40, 0x7e, 0x7f, 0xc0, 0xff })
            AssertEqual((ushort)0xabab, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, bank, 0, 0x21db), "same offset outside mirrored banks remains mapped");
        AssertEqual((ushort)0xabab, SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus, 0x9b, 0, 0x2140), "real APU registers are not silently treated as open bus");
    }

    private sealed class OperandReadWitness : ISnesAddressSpace
    {
        public byte ReadByte(int address) => 0xab;
        public void WriteByte(int address, byte value) => throw new NotSupportedException();
    }
}
