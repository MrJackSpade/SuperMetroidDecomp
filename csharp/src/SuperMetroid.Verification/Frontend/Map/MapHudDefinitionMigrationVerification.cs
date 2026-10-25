using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>
/// Confirms #1416's file-select, map and HUD migration: each deleted catalog's definitions still produce, for one member
/// of each kind, the values the catalog held; the sources that moved to extraction keep their cartridge locations; and
/// the identity tables translate the arbitrary map-sprite and message-routine identities to the cartridge's ordinals and
/// routine words. None reads the ROM.
/// </summary>
internal static class MapHudDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions, extraction sources and identity tables with the values the deleted catalogs held.</summary>
    internal static void VerifyMapHudDefinitionMigration()
    {
        // FileSelectMapRomData and FileSelectMapIconRomData: counts, windows, timing and marker palettes stay in Core.
        AssertEqual(6, FileSelectMapDefinitions.AreaCount, "selectable areas");
        AssertEqual(24, FileSelectMapDefinitions.RoomFrameVerticalScroll, "room frame offset");
        AssertEqual(111, FileSelectMapDefinitions.EntryWindowTop, "entry window top");
        AssertEqual(4, FileSelectMapDefinitions.EntryWindowSpeed, "entry window speed");
        AssertEqual(32, FileSelectMapDefinitions.LoadBlackFrames, "load black frames");
        AssertEqual(15, FileSelectMapDefinitions.EntryPaletteDenominator, "entry fade steps");
        AssertEqual(0x0e00, FileSelectMapDefinitions.StationMarkerPalette, "station marker palette");
        AssertEqual(0x0c00, FileSelectMapDefinitions.DefeatedBossPalette, "defeated boss palette");
        AssertEqual(4, MapScrollDefinitions.PulseTick, "scroll pulse tick");
        AssertEqual(8, MapScrollDefinitions.StepTicks, "scroll step ticks");
        AssertEqual(8, MapScrollDefinitions.StepPixels, "scroll step pixels");

        // Their sources, the map animation, palette, atlas and area-map sources moved to extraction.
        AssertEqual(0x82c569, MapSourceDefinitions.Menu.SpritemapPointers, "menu spritemap pointers");
        AssertEqual(0x8ee400, MapSourceDefinitions.Menu.Colors, "menu colours");
        AssertEqual(0x81af32, MapSourceDefinitions.FileSelectMap.ScrollArrows, "scroll arrow records");
        AssertEqual(0xefff, MapSourceDefinitions.FileSelectMap.RoomLabelMask, "room label mask");
        AssertEqual(0x82c7cb, MapSourceDefinitions.Landmarks.BossLists, "boss lists");
        AssertEqual(0x82c10c, MapSourceDefinitions.Animation.PaletteTiming, "highlight timing");
        AssertEqual(0xb6f000, MapSourceDefinitions.Artwork.PauseColors, "pause colours");
        AssertEqual(0xb68000, MapSourceDefinitions.Artwork.MapCharacters, "map characters");
        AssertEqual(0x9ab200, MapSourceDefinitions.Artwork.HudCharacters, "HUD characters");
        AssertEqual(0x82964a, MapSourceDefinitions.AreaMaps.TilemapPointers, "area tilemap pointers");
        AssertEqual(0x1000, MapSourceDefinitions.AreaMaps.TilemapBytes, "area tilemap bytes");
        AssertEqual(0xc000, MapTileWordMasks.FlipMask, "map tile flip bits");

        // FileSelectTilemapDefinitions moved to extraction whole.
        AssertEqual(0xb40a, FileSelectTilemapSourceDefinitions.SamusData, "SAMUS DATA stream");
        AssertEqual(0xb476, FileSelectTilemapFormat.SlotLabel(2), "third slot label stream");

        // MapSpriteId's values are arbitrary; the import's table holds the spritemap ordinals.
        AssertEqual(0x4e, MapSpriteOrdinals.Spritemaps.Word(MapSpriteId.StationMap), "map station spritemap ordinal");
        AssertEqual(MapSpriteId.WorldTitle, MapSpriteOrdinals.Spritemaps.Decode(0x38, "map spritemap ordinal"), "world title decodes");

        // GameplayMessageRomData: layout, timing, colours and glyphs stay in Core; its sources moved to extraction.
        AssertEqual(32, GameplayMessageBoxDefinitions.Layout.TilemapWidth, "message tilemap width");
        AssertEqual(124, GameplayMessageBoxDefinitions.Layout.WindowCenterY, "message window centre");
        AssertEqual(8, GameplayMessageBoxDefinitions.Timing.PreOpenLagFrames, "pre-open waits");
        AssertEqual(360, GameplayMessageBoxDefinitions.Timing.ItemMinimumDisplayFrames, "item message duration");
        AssertEqual(RgbColor.FromWord(0x0bb1), GameplayMessageBoxDefinitions.Palette.TemporaryLightColor, "light message colour");
        AssertEqual(0x284e, GameplayMessageBoxDefinitions.Buttons.UnknownGlyph.Raw, "blank button glyph");
        AssertEqual(0x3ce1, GameplayMessageButtonGlyphs.ResolveGlyphWord(ControllerButton.B), "B glyph");
        AssertEqual(0x120, GameplayMessageButtonGlyphs.SpecialGlyphByteOffset(GameplayMessageId.SpeedBooster), "speed booster glyph offset");
        AssertEqual(0x85869b, HudSourceDefinitions.Messages.DefinitionTable, "message definition table");
        AssertEqual(6, HudSourceDefinitions.Messages.DefinitionBytes, "message definition bytes");
        AssertEqual(0x80988b, HudSourceDefinitions.TopRow, "HUD top row");
        AssertEqual(0x80998b, HudSourceDefinitions.AutoReserve, "AUTO indicator cells");
        AssertEqual(0x8441, GameplayMessageRoutineWords.Routines.Word(GameplayMessageRoutine.SetupLarge), "large setup routine word");
        AssertEqual(GameplayMessageRoutine.DrawSmallTilemap, GameplayMessageRoutineWords.Routines.Decode(0x8289, "message draw routine"), "small draw routine decodes");
        Console.WriteLine("Map and HUD definition migration: file-select, map and message definitions, extraction sources and identity tables keep their values.");
    }

    /// <summary>Runs the focused map and HUD definition-migration check.</summary>
    [VerificationCommand("--map-hud-definition-migration")]
    internal static int MapHudDefinitionMigrationCommand()
    {
        VerifyMapHudDefinitionMigration();
        return 0;
    }
}
