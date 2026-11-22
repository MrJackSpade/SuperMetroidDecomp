using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Frontend;

namespace SuperMetroid.Verification.Frontend.Menus;

/// <summary>Checks the file-menu missile animation's compiled spritemap selectors and frame duration against retail data.</summary>
internal static class MenuMissileLookupsVerification
{
    /// <summary>Compares the roll's four spritemap IDs, in the order it shows them, and native wrap mask, including rejection of undefined pictures.</summary>
    internal static void VerifyMenuMissileSpritemapIds(ISnesAddressSpace rom)
    {
        AssertEqual(4, MenuMissileAnimationDefinitions.Roll.Count, "menu missile frame count");
        AssertEqual((byte)0x29, rom.ReadCartridgeByte(0x82ba80), "native frame wrap AND immediate");
        AssertEqual((ushort)(MenuMissileAnimationDefinitions.Roll.Count - 1),
            ReadVerificationWord(rom, 0x82ba81), "native frame wrap mask");
        for (int place = 0; place < 4; place++)
            AssertEqual(ReadVerificationWord(rom, 0x82bab2 + place * 2),
                MenuMissileSpritemaps.SpritemapId(MenuMissileAnimationDefinitions.Roll[place]), $"menu missile frame {place}");
    }

    /// <summary>Compares the duration the import installs for the menu missile's frames with each of the four bank-$82 words.</summary>
    internal static void VerifyMenuMissileDurations(ISnesAddressSpace rom)
    {
        int installed = MenuAnimationTimings.MissileFrameDuration(rom);
        for (int frame = 0; frame < 4; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x82baaa + frame * 2), installed, $"menu missile duration {frame}");
    }
}
