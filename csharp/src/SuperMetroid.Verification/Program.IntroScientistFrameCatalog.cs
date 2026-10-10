using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the ten intro-scientist frame records against their cartridge chain, including the final shared-caret alias and catalog bounds.</summary>
    /// <param name="rom">Cartridge address space containing the native scientist frame record chain.</param>
    private static void VerifyIntroScientistFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["examined-loop-1", "examined-loop-2", "examined-loop-3",
            "delivered-baby-1", "delivered-baby-2", "delivered-baby-3",
            "examined-baby-1", "examined-baby-2", "examined-baby-3", "examined-baby-hold"];
        var frames = IntroScientistSpriteDefinitions.Frames;
        AssertEqual(10, frames.Count, "scientist catalog contains ten source records");
        int pointer = 0x8ccf;
        for (int index = 0; index < 10; index++)
        {
            int count = rom.ReadByte(0x8c0000 + pointer) | rom.ReadByte(0x8c0001 + pointer) << 8;
            AssertEqual((ushort)pointer, frames[index].Pointer, "scientist frame follows original record chain");
            AssertEqual(count, frames[index].StockPartCount, "original scientist part count");
            AssertEqual(names[index], frames[index].Name, "published scientist asset key");
            pointer += 2 + 5 * count;
        }
        AssertEqual(0x8d6f, pointer, "original record chain ends at catalog boundary");
        AssertEqual(IntroCaretSpriteDefinitions.Still, frames[9].Pointer, "last scientist record aliases the shared caret");
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = frames[invalid]; }, "scientist catalog boundary");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroScientistSpriteDefinitions.FramePointer(invalid), "scientist pointer boundary");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "scientist catalog enumeration agrees with indexing");
    }
}
