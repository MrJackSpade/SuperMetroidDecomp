using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroDiscoveryFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["egg-intact", "egg-crack-1", "egg-crack-2", "egg-crack-3",
            "egg-crack-4", "egg-crack-5", "egg-crack-6", "egg-crack-7", "egg-hatched",
            "egg-remnant-1", "egg-remnant-2", "egg-remnant-3", "egg-remnant-4",
            "egg-remnant-5", "egg-remnant-6", "egg-remnant-7",
            "confused-baby-1", "confused-baby-2", "confused-baby-3", "hatched-baby"];
        var frames = IntroDiscoveryActorSpriteDefinitions.Frames;
        AssertEqual(20, frames.Count, "discovery catalog original frame count");
        int pointer = 0x8d6f;
        for (int index = 0; index < frames.Count; index++)
        {
            if (index == 16)
            {
                AssertEqual(0x8f7e, pointer, "egg record chain end");
                pointer = 0x8fcb;
            }
            if (index == 19)
            {
                AssertEqual(0x8fe0, pointer, "small baby record chain end");
                pointer = rom.ReadByte(0x8bcc41) | rom.ReadByte(0x8bcc42) << 8;
            }
            int count = rom.ReadByte(0x8c0000 + pointer) | rom.ReadByte(0x8c0001 + pointer) << 8;
            AssertEqual((ushort)pointer, frames[index].Pointer, "discovery original record pointer");
            AssertEqual(count, frames[index].StockPartCount, "discovery original part count");
            AssertEqual(names[index], frames[index].Name, "discovery published asset key");
            pointer += 2 + 5 * count;
        }
        AssertEqual(0x90fe, pointer, "large baby record chain end");
        foreach (int invalid in new[] { int.MinValue, -1, 20, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = frames[invalid]; }, "discovery catalog bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => IntroDiscoveryActorSpriteDefinitions.EggFramePointer(invalid), "egg pointer bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => IntroDiscoveryActorSpriteDefinitions.BabyFramePointer(invalid), "baby pointer bounds");
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "discovery enumeration agrees with indexing");
    }
}
