using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Menus;

/// <summary>Checks that the menus publish their own screen and that sprite entries keep OAM's placement (#1358).</summary>
internal static class MenuScreenPublicationVerification
{
    /// <summary>
    /// Sprite entries place spritemap parts exactly as the cartridge's OAM loader does: nine-bit X, the vertical-wrap
    /// hiding that parks a part, a negative offset reaching above the screen, large parts and the 128-object limit.
    /// </summary>
    internal static void VerifyScreenSpritePlacement()
    {
        MenuScreenPublicationPart[] parts =
        [
            new MenuScreenPublicationPart(100, 100, 4, 8, false),
            new MenuScreenPublicationPart(100, 220, 0, 8, false),
            new MenuScreenPublicationPart(100, 4, 0, -8, false),
            new MenuScreenPublicationPart(250, 40, 10, 0, true),
            new MenuScreenPublicationPart(10, 40, -20, 0, false),
            new MenuScreenPublicationPart(128, 0, 0, -16, true),
        ];
        var sprites = ScreenSpritesFixtures.Blank();
        var oam = new OamBuffer();
        sprites.BeginFrame();
        oam.BeginFrame();
        for (int index = 0; index < ScreenSprites.Capacity + 2; index++)
        {
            var part = parts[index % parts.Length];
            var x = SnesSpritemapXWord.Create(part.OffsetX, part.Large);
            var y = SnesSpritemapYByte.Create(part.OffsetY);
            var attributes = SnesObjAttributeWord.Create(index % 0x200, index % 8, index % 4);
            var origin = new Position16<OamPixels>((ushort)part.X, (ushort)part.Y);
            sprites.AddImage(ScreenSpritesFixtures.PartFrame(x, y, attributes), origin);
            oam.AddOnScreenSpritePart(x, y, attributes, origin);
        }
        sprites.FinishFrame();
        oam.FinalizeFrame();
        AssertEqual(ScreenSprites.Capacity, sprites.Draws.Length, "sprite entries stop at the 128-object limit");
        AssertTrue(oam.ImageEquals(sprites), "sprite entries stand for exactly the OAM records the native loader writes");
        Console.WriteLine("  Screen sprites: spritemap frames keep the native OAM loader's placement as sprite entries.");
    }

    /// <summary>
    /// File select, the options menu and game over each publish a frame composed from their own screen: its tilemaps
    /// over the bound characters, its palette, and its sprite entries over the menu OBJ page.
    /// </summary>
    internal static void VerifyMenuScreenPublication()
    {
        var maps = InstalledFixtures.Maps;
        var saves = new SaveSlotStore(RetailPresentationFixture());
        saves.SaveSlot(0, new SuperMetroidSaveSnapshot { Health = 99, MaxHealth = 99 });
        saves.SelectSlot(0);
        var fileSelect = new FileSelectMenuState(saves, mapPresentation: maps, spriteSheets: MenuSheetFixtures.For(maps));
        for (int frame = 0; frame < 60 && fileSelect.Phase != FileSelectPhase.Main; frame++)
            fileSelect.Step(0);
        AssertEqual(FileSelectPhase.Main, fileSelect.Phase, "file select reaches its main menu");
        AssertPublished(fileSelect.screen, fileSelect.CaptureRenderSnapshot(), "file select");

        var options = new GameOptionsMenuState(mapPresentation: maps, spriteSheets: MenuSheetFixtures.For(maps));
        for (int frame = 0; frame < 30; frame++)
            options.Step(0);
        AssertPublished(options.screen, options.CaptureRenderSnapshot(), "options");

        var gameOver = new GameOverMenuState(new CartridgeAudioState(), maps, MenuSheetFixtures.For(maps));
        for (int frame = 0; frame < 30; frame++)
            gameOver.Step(0);
        AssertPublished(gameOver.screen, gameOver.CaptureRenderSnapshot(), "game over");
        Console.WriteLine("  Menu screens: file select, options and game over publish their own tilemaps, palette and sprite entries.");
    }

    /// <summary>Asserts that <paramref name="snapshot"/> is composed from <paramref name="screen"/>'s published state.</summary>
    private static void AssertPublished(MenuScreen screen, LayeredScene snapshot, string menu)
    {
        FramePaint memory = snapshot.Paint;
        // The image the menu composes from its screen when it captures a frame (#1403).
        byte[] vram = MenuScreenMemory.Compose(screen);
        ReadOnlySpan<ushort> tilemaps = screen.Tilemaps.All;
        int first = MenuVramLayout.Bg1TilemapWord * sizeof(ushort);
        for (int word = 0; word < tilemaps.Length; word++)
            AssertEqual(tilemaps[word], SnesLittleEndian.Word(vram[first + word * 2], vram[first + word * 2 + 1]),
                $"{menu} draws its own tilemap word {word}");
        int last = first + tilemaps.Length * sizeof(ushort);
        AssertTrue(vram[..first].SequenceEqual(screen.Characters[..first]) && vram[last..].SequenceEqual(screen.Characters[last..]),
            $"{menu} draws its bound characters");
        AssertTrue(memory.Colors.SequenceEqual(screen.Palette.Colors), $"{menu} draws its own palette");
        AssertTrue(screen.Objects.Draws.Length > 0, $"{menu} places sprite entries");
        AssertTrue(memory.SpriteDraws.SequenceEqual(screen.Objects.Draws), $"{menu} draws its own sprite entries");
        // A screen's single objects draw from the menu OBJ page; its whole frames from its sheet.
        foreach (SpriteDraw draw in memory.SpriteDraws)
            AssertTrue(draw is ImageSpriteDraw || ReferenceEquals(MapSpriteArtwork.Of(screen.Sprites).Page, draw.Artwork), $"{menu} sprite entries draw from the menu OBJ page");
    }

    /// <summary>Runs the screen sprite placement and menu screen publication checks.</summary>
    [VerificationCommand("--menu-screen")]
    internal static int MenuScreenCommand()
    {
        Suite(nameof(VerifyScreenSpritePlacement), () => VerifyScreenSpritePlacement());
        Suite(nameof(VerifyMenuScreenPublication), () => VerifyMenuScreenPublication());
        return 0;
    }
}
