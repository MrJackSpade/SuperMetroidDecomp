using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

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
            var origin = new Position16<SpritePixels>((ushort)part.X, (ushort)part.Y);
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
        // File select shows its backdrop's picture, then its page's picture with its slots' fields over it (#1467).
        SpriteSheet fileSelectPictures = MenuSheetFixtures.For(maps).Get(SpriteSheetId.MenuPictures);
        LayeredScene fileSelectScene = fileSelect.CaptureRenderSnapshot();
        AssertTrue(fileSelectScene.Layers.Length == 3 && fileSelectScene.Layers[0] is PicturedPlaneLayer { Plane.Pictures: [{ } fileBackdrop] } &&
            fileBackdrop.Picture.Equals(fileSelectPictures.Frame(FileSelectParts.BackgroundFrame)) && fileSelectScene.Layers[2] is SpritesLayer,
            "file select shows its backdrop's picture under its page and sprites");
        AssertTrue(fileSelectScene.Layers[1] is PicturedPlaneLayer { Plane.Pictures: { } filePage } && filePage.Length > 1 &&
            filePage[0].Picture.Equals(fileSelectPictures.Frame(FileSelectParts.PageFrame(FileSelectPage.MainWithData))) &&
            filePage.Any(placed => placed.Picture.Equals(fileSelectPictures.Frame(FileSelectParts.PatchFrame(FileSelectPatch.Energy)))),
            "file select shows the main page with the saved slot's ENERGY field");
        AssertTrue(fileSelectScene.Paint.Colors.SequenceEqual(fileSelect.screen.Palette.Colors), "file select draws its own palette");
        AssertTrue(PaintedSpriteEntries.AreOwn(fileSelectScene.Paint.SpriteDraws, fileSelect.screen.Objects.Draws), "file select draws its own sprite entries");

        InstalledSpriteSheets optionsSheets = MenuSheetFixtures.For(maps);
        var options = new GameOptionsMenuState(mapPresentation: maps, spriteSheets: optionsSheets);
        for (int frame = 0; frame < 30; frame++)
            options.Step(0);
        // Options shows its backdrop's picture, then the primary page's picture with its four English language rows over it (#1467).
        SpriteSheet menuPictures = optionsSheets.Get(SpriteSheetId.MenuPictures);
        LayeredScene optionsScene = options.CaptureRenderSnapshot();
        AssertTrue(optionsScene.Layers.Length == 3 && optionsScene.Layers[0] is PicturedPlaneLayer { Plane.Pictures: [{ } backdrop] } &&
            ReferenceEquals(backdrop.Picture, menuPictures.Frame(GameOptionsNames.BackgroundFrame)) && optionsScene.Layers[2] is SpritesLayer,
            "options shows its backdrop's picture under its page and sprites");
        AssertTrue(optionsScene.Layers[1] is PicturedPlaneLayer { Plane.Pictures: { } page } && page.Length == 1 + GameOptionsDefinitions.LanguagePaletteRegionCount &&
            ReferenceEquals(page[0].Picture, menuPictures.Frame(GameOptionsNames.PageFrame(GameOptionsTilemap.Primary))) &&
            Enumerable.Range(0, GameOptionsDefinitions.LanguagePaletteRegionCount).All(region =>
                ReferenceEquals(page[1 + region].Picture, menuPictures.Frame(GameOptionsNames.LanguageFrame(region, japanese: false)))),
            "options shows the primary page with its language rows highlighting English");
        AssertTrue(optionsScene.Paint.Colors.SequenceEqual(options.screen.Palette.Colors), "options draws its own palette");
        AssertTrue(PaintedSpriteEntries.AreOwn(optionsScene.Paint.SpriteDraws, options.screen.Objects.Draws), "options draws its own sprite entries");

        InstalledSpriteSheets gameOverSheets = MenuSheetFixtures.For(maps);
        var gameOver = new GameOverMenuState(new AudioQueues(), maps, gameOverSheets);
        for (int frame = 0; frame < 30; frame++)
            gameOver.Step(0);
        // Game over shows one picture of its whole page at the page's corner, under the sprites (#1467).
        LayeredScene gameOverScene = gameOver.CaptureRenderSnapshot();
        AssertTrue(gameOverScene.Layers.Length == 2 && gameOverScene.Layers[0] is PicturedPlaneLayer { Plane.Pictures: [{ } page] } &&
            ReferenceEquals(page.Picture, gameOverSheets.Get(SpriteSheetId.MenuPictures).Frame(SuperMetroid.Core.Frontend.GameOver.Artwork.GameOverPresentationDefinitions.PageFrame)) &&
            page.Corner == default && gameOverScene.Layers[1] is SpritesLayer, "game over shows its page's picture under its sprites");
        AssertTrue(gameOverScene.Paint.Colors.SequenceEqual(gameOver.screen.Palette.Colors), "game over draws its own palette");
        AssertTrue(PaintedSpriteEntries.AreOwn(gameOverScene.Paint.SpriteDraws, gameOver.screen.Objects.Draws), "game over draws its own sprite entries");
        Console.WriteLine("  Menu screens: file select, options and game over publish their pages' pictures, palette and sprite entries.");
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
