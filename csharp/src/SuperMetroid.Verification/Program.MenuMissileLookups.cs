using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;

/// <summary>Represents menu missile lookups verification.</summary>
internal static class MenuMissileLookupsVerification
{
    /// <summary>Verifies menu missile spritemap ids.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for menu missile spritemap ids")]
    internal static void VerifyMenuMissileSpritemapIds(ISnesAddressSpace rom)
    {
        AssertEqual(4, MenuMissileAnimationDefinitions.FrameCount, "menu missile frame count");
        AssertEqual((byte)0x29, rom.ReadCartridgeByte(0x82ba80), "native frame wrap AND immediate");
        AssertEqual((ushort)(MenuMissileAnimationDefinitions.FrameCount - 1),
            ReadVerificationWord(rom, 0x82ba81), "native frame wrap mask");
        for (int frame = 0; frame < 4; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x82bab2 + frame * 2),
                MenuMissileAnimationDefinitions.SpritemapId(frame), $"menu missile frame {frame}");
        foreach (int frame in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MenuMissileAnimationDefinitions.SpritemapId(frame),
                $"menu missile invalid frame {frame}");
    }

    /// <summary>Verifies menu missile durations.</summary>
    internal static void VerifyMenuMissileDurations(ISnesAddressSpace rom)
    {
        for (int frame = 0; frame < 4; frame++)
            AssertEqual((int)ReadVerificationWord(rom, 0x82baaa + frame * 2),
                MenuMissileAnimationDefinitions.FrameDuration, $"menu missile duration {frame}");
    }
}
