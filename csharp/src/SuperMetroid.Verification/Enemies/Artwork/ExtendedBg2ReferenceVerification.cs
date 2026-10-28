using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Constructs a cartridge-sourced extended BG2 VRAM reference independently of runtime catalogs.</summary>
internal static class ExtendedBg2ReferenceVerification
{
    /// <summary>
    /// Emulates native $A0:96CA copying each $FFFE stream through the $7E:2000
    /// tilemap to VRAM word $4800. Returns an empty VRAM image when the instruction
    /// frame is unchanged and rejects unterminated streams.
    /// </summary>
    internal static byte[] ReadReferenceExtendedBg2Vram(
        ISnesAddressSpace rom, byte bank, ushort pointer, bool newInstructionFrame)
    {
        byte ReadByte(int address) => rom.ReadCartridgeByte(((int)new SnesAddress(bank, checked((ushort)(SnesAddressMath.OffsetInBank(address))))));
        ushort ReadWord(int address) => LittleEndianBytes.Word(ReadByte(address), ReadByte(address + 1));
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
