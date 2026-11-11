using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Frontend;

namespace SuperMetroid.Verification.Frontend.Menus;

/// <summary>Checks the file-menu missile animation's compiled spritemap selectors and frame duration against retail data.</summary>
internal static class MenuMissileLookupsVerification
{
    /// <summary>Compares the four spritemap IDs and native wrap mask, including rejection of invalid frame indices.</summary>
    internal static void VerifyMenuMissileSpritemapIds(ISnesAddressSpace rom)
    {
        AssertEqual(4, MenuMissileAnimationDefinitions.FrameCount, "menu missile frame count");
        AssertEqual((byte)0x29, rom.ReadCartridgeByte(0x82ba80), "native frame wrap AND immediate");
        AssertEqual((ushort)(MenuMissileAnimationDefinitions.FrameCount - 1),
            ReadVerificationWord(rom, 0x82ba81), "native frame wrap mask");
        for (int frame = 0; frame < 4; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x82bab2 + frame * 2),
                MenuMissileSpritemaps.SpritemapId(frame), $"menu missile frame {frame}");
        foreach (int frame in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MenuMissileSpritemaps.SpritemapId(frame),
                $"menu missile invalid frame {frame}");
    }

    /// <summary>Compares the duration the import installs for the menu missile's frames with each of the four bank-$82 words.</summary>
    internal static void VerifyMenuMissileDurations(ISnesAddressSpace rom)
    {
        int installed = MenuAnimationTimings.MissileFrameDuration(rom);
        for (int frame = 0; frame < 4; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x82baaa + frame * 2), installed, $"menu missile duration {frame}");
    }
}
