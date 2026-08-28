using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
static void VerifyLibraryBackgroundLoader()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];

    // Fixture list mirrors Ceres $8F:E4A5: decompress a four-byte tilemap to $7E:4000,
    // copy it to both BG2 screen pages, then terminate. The compressed stream begins at
    // $90:8000 and consists of one four-byte literal followed by $FF.
    byte[] compressed = [0x03, 0x11, 0x22, 0x33, 0x44, 0xff];
    int compressedOffset = SuperMetroidAddressSpace.ToRomOffset(0x908000);
    compressed.CopyTo(rom, compressedOffset);

    ushort cursor = 0xe000;
    WriteLibraryWord(rom, cursor, 0x0004); cursor += 2;
    WriteLibraryLong(rom, cursor, 0x908000); cursor += 3;
    WriteLibraryWord(rom, cursor, 0x4000); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0002); cursor += 2;
    WriteLibraryLong(rom, cursor, 0x7e4000); cursor += 3;
    WriteLibraryWord(rom, cursor, 0x4800); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0004); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0002); cursor += 2;
    WriteLibraryLong(rom, cursor, 0x7e4000); cursor += 3;
    WriteLibraryWord(rom, cursor, 0x4c00); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0004); cursor += 2;
    WriteLibraryWord(rom, cursor, 0x0000);

    var bus = new SuperMetroidAddressSpace(rom);
    var vram = new SnesVram();
    int commands = LibraryBackgroundLoader.Execute(bus, vram, 0xe000, activeDoorPointer: 0);
    AssertEqual(4, commands, "library-background command count includes terminator");
    AssertEqual(0x2211, vram.ReadWord(0x4800), "library background first BG2 page word");
    AssertEqual(0x4433, vram.ReadWord(0x4801), "library background first BG2 page tail");
    AssertEqual(0x2211, vram.ReadWord(0x4c00), "library background second BG2 page word");
    AssertEqual(0x4433, vram.ReadWord(0x4c01), "library background second BG2 page tail");

    // Command A is the actual starting-Ceres list shape and must overwrite stale VRAM on
    // both pages with the native blank tile rather than relying on a fresh host array.
    WriteLibraryWord(rom, 0xe040, 0x000a);
    WriteLibraryWord(rom, 0xe042, 0x0000);
    bus = new SuperMetroidAddressSpace(rom);
    vram = new SnesVram();
    vram.ExecuteWordTransfer([0xffff], 0x4800, 1);
    LibraryBackgroundLoader.Execute(bus, vram, 0xe040, activeDoorPointer: 0);
    AssertEqual(0x0338, vram.ReadWord(0x4800), "library background clear first word");
    AssertEqual(0x0338, vram.ReadWord(0x4fff), "library background clear final word");

    Console.WriteLine(
        "  Library BG: decompression, both Ceres BG2 pages, and native clear agree.");
}

private static void WriteLibraryWord(byte[] rom, ushort pointer, ushort value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), (byte)(value >> 8));
}

private static void WriteLibraryLong(byte[] rom, ushort pointer, int value)
{
    WriteLibraryByte(rom, pointer, (byte)value);
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 1)), (byte)(value >> 8));
    WriteLibraryByte(rom, unchecked((ushort)(pointer + 2)), (byte)(value >> 16));
}

private static void WriteLibraryByte(byte[] rom, ushort pointer, byte value)
{
    int offset = SuperMetroidAddressSpace.ToRomOffset(0x8f0000 | pointer);
    rom[offset] = value;
}
}
