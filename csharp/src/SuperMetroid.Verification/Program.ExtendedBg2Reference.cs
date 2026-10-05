using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Native $A0:96CA copies each $FFFE stream into the $7E:2000 tilemap,
    // later transferred to VRAM word $4800. This test reference reads raw
    // cartridge streams independently of installed catalogs and runtime writers.
    private static byte[] ReadReferenceExtendedBg2Vram(
        ISnesAddressSpace rom, byte bank, ushort pointer, bool newInstructionFrame)
    {
        byte ReadByte(int address) => rom.ReadByte((bank << 16) | (address & 0xffff));
        ushort ReadWord(int address) => (ushort)(ReadByte(address) | ReadByte(address + 1) << 8);
        var vram = new byte[0x10000];
        if (!newInstructionFrame)
            return vram;
        int components = ReadByte(pointer);
        for (int component = 0; component < components; component++)
        {
            ushort stream = ReadWord(pointer + 6 + component * 8);
            if (ReadWord(stream) != 0xfffe)
                continue;
            int cursor = stream + 2;
            bool terminated = false;
            for (int command = 0; command < 256; command++)
            {
                ushort destination = ReadWord(cursor);
                if (destination == 0xffff)
                {
                    terminated = true;
                    break;
                }
                ushort words = ReadWord(cursor + 2);
                int byteOffset = 0x9000 + destination - 0x2000;
                for (int index = 0; index < words * 2; index++)
                    vram[byteOffset + index] = ReadByte(cursor + 4 + index);
                cursor += 4 + words * 2;
            }
            AssertTrue(terminated, "native extended BG2 stream terminates");
        }
        return vram;
    }
}