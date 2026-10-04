using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMenuMissileSpritemapIds(ISnesAddressSpace rom)
    {
        AssertEqual(4, MenuMissileAnimationDefinitions.FrameCount, "menu missile frame count");
        AssertEqual((byte)0x29, rom.ReadByte(0x82ba80), "native frame wrap AND immediate");
        AssertEqual((ushort)(MenuMissileAnimationDefinitions.FrameCount - 1),
            ReadVerificationWord(rom, 0x82ba81), "native frame wrap mask");
        for (int frame = 0; frame < 4; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x82bab2 + frame * 2),
                MenuMissileAnimationDefinitions.SpritemapId(frame), $"menu missile frame {frame}");
        foreach (int frame in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MenuMissileAnimationDefinitions.SpritemapId(frame),
                $"menu missile invalid frame {frame}");
    }

    private static void VerifyMenuMissileDurations(ISnesAddressSpace rom)
    {
        for (int frame = 0; frame < 4; frame++)
            AssertEqual((int)ReadVerificationWord(rom, 0x82baaa + frame * 2),
                MenuMissileAnimationDefinitions.FrameDuration, $"menu missile duration {frame}");
    }
}
