using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroMotherBrainExplosionPrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int pointer = 0xcdab; pointer < 0xcdeb; pointer++)
        {
            AssertEqual(rom.ReadByte(0x8b0000 + pointer), IntroMotherBrainExplosionInstructionDefinitions.ReadByte((ushort)pointer), "original explosion program byte");
            if (pointer < 0xcdea)
                AssertEqual(Word(0x8b0000 + pointer), IntroMotherBrainExplosionInstructionDefinitions.ReadWord((ushort)pointer), "original explosion overlapping word");
        }
        for (int offset = 0; offset < 2; offset++)
            AssertEqual(rom.ReadByte(0x8bce53 + offset), IntroMotherBrainExplosionInstructionDefinitions.ReadByte((ushort)(0xce53 + offset)), "original shared delete byte");
        AssertEqual(Word(0x8bce53), IntroMotherBrainExplosionInstructionDefinitions.ReadWord(0xce53), "original delete operation");
        foreach (ushort pointer in new ushort[] { 0, 0xcdaa, 0xcdeb, 0xce52, 0xce55, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => IntroMotherBrainExplosionInstructionDefinitions.ReadByte(pointer), "explosion byte boundary");
            AssertThrows<InvalidDataException>(() => IntroMotherBrainExplosionInstructionDefinitions.ReadWord(pointer), "explosion word boundary");
        }
        foreach (ushort pointer in new ushort[] { 0xcdea, 0xce54 })
            AssertThrows<InvalidDataException>(() => IntroMotherBrainExplosionInstructionDefinitions.ReadWord(pointer), "explosion word crosses boundary");
    }

    private static void VerifyIntroMotherBrainExplosionFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["small-explosion-0", "small-explosion-1", "small-explosion-2", "small-explosion-3", "small-explosion-4", "small-explosion-5",
            "big-explosion-0", "big-explosion-1", "big-explosion-2", "big-explosion-3", "big-explosion-4", "big-explosion-5"];
        var frames = IntroMotherBrainExplosionSpriteDefinitions.Frames;
        AssertEqual(12, frames.Count, "original explosion frame count");
        for (int index = 0; index < frames.Count; index++)
        {
            int operand = (index < 6 ? 0x8bcdcd : 0x8bcdad) + 4 * (index % 6);
            ushort pointer = (ushort)(rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8);
            int count = rom.ReadByte(0x8c0000 + pointer) | rom.ReadByte(0x8c0001 + pointer) << 8;
            AssertEqual(pointer, frames[index].Pointer, "original explosion frame operand");
            AssertEqual(count, frames[index].StockPartCount, "original explosion part count");
            AssertEqual(names[index], frames[index].Name, "stable explosion asset name");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = frames[invalid]; }, "explosion catalog bounds");
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        foreach (bool big in new[] { false, true })
            AssertThrows<ArgumentOutOfRangeException>(() => IntroMotherBrainExplosionSpriteDefinitions.FramePointer(big, invalid), "explosion pointer bounds");
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "explosion enumeration preserves indexed order");
    }
}
