using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroMotherBrainFrameCatalog(ISnesAddressSpace rom)
    {
        var frames = IntroMotherBrainSpriteDefinitions.Frames;
        AssertEqual(3, frames.Count, "Mother Brain three original frames");
        for (int frame = 0; frame < 3; frame++)
        {
            int operand = 0x8bcb07 + 4 * frame;
            ushort pointer = (ushort)(rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8);
            AssertEqual(pointer, frames[frame].Pointer, "original Mother Brain frame operand");
            int count = rom.ReadByte(0x8c0000 + pointer) | rom.ReadByte(0x8c0001 + pointer) << 8;
            AssertEqual(count, IntroMotherBrainSpriteDefinitions.StockPartCount, "original nine-part frame");
            AssertEqual($"mother-brain-frame-{frame}", frames[frame].Name, "published frame key");
        }
        AssertEqual(frames[1].Pointer, IntroMotherBrainSpriteDefinitions.FrameOne, "second frame alias");
        AssertEqual(frames[2].Pointer, IntroMotherBrainSpriteDefinitions.FrameTwo, "third frame alias");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = frames[invalid]; }, "frame catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroMotherBrainSpriteDefinitions.FramePointer(invalid), "frame pointer bounds");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "frame enumeration matches indexing");
    }
}
