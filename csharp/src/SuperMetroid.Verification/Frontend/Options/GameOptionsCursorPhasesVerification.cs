using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Options;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks the options cursor's native phase selection, and that transitions draw no cursor (#1775).</summary>
internal static class GameOptionsCursorPhasesVerification
{
    /// <summary>Compares all thirteen phase entries and exercises dissolve and intro fade transitions.</summary>
    internal static void VerifyGameOptionsCursorPhases()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Options cursor oracle revision");
        GameOptionsCursorPhasesPhase[] phases =
        [
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.FadeIn, GameOptionsPage.Primary, 2),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.Browsing, GameOptionsPage.Primary, 3),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.DissolveOut, GameOptionsPage.Primary, 5),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.DissolveIn, GameOptionsPage.Controller, 6),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.Browsing, GameOptionsPage.Controller, 7),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.Browsing, GameOptionsPage.Special, 8),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.ScrollControllerDown, GameOptionsPage.Controller, 9),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.ScrollControllerUp, GameOptionsPage.Controller, 10),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.FadeOutToFileSelect, GameOptionsPage.Primary, 11),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.FadeOutToIntro, GameOptionsPage.Primary, 12),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.StartGame, GameOptionsPage.Primary, 4),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.BlackHold, GameOptionsPage.Primary, 0),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.BlackHold, GameOptionsPage.Primary, 1),
        ];
        foreach (var phase in phases)
        {
            ushort pointer = GameOptionsCursorPolicy.Select(phase.Phase, phase.Page) switch
            {
                null => 0,
                GameOptionsPage.Primary => 0xf307,
                GameOptionsPage.Controller => 0xf31b,
                GameOptionsPage.Special => 0xf33f,
                _ => throw new InvalidDataException("Unexpected cursor page."),
            };
            AssertEqual(ReadVerificationWord(bus, 0x82f2ed + phase.Native * 2), pointer,
                $"native options cursor selection {phase.Native}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 13, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameOptionsCursorPolicy.Select((GameOptionsPhase)invalid, GameOptionsPage.Primary),
                "unsupported options cursor phase");
        var installed = RetailPresentationFixture();
        var controllerPage = NewMenu();
        for (int row = 0; row < GameOptionsPageRows.Primary.PlaceOf(GameOptionsPrimaryRow.ControllerSettings); row++)
        {
            controllerPage.Step(ControllerButton.Down);
            controllerPage.Step(0);
        }
        AssertCursorVisible(controllerPage, "primary selection before dissolve");
        controllerPage.Step(ControllerButton.A);
        AssertEqual(GameOptionsPhase.DissolveOut, controllerPage.Phase, "enter dissolve-out");
        AssertCursorHidden(controllerPage, "dissolve-out");
        for (int frame = 0; frame < 20 && controllerPage.Phase != GameOptionsPhase.DissolveIn; frame++)
            controllerPage.Step(0);
        AssertEqual(GameOptionsPhase.DissolveIn, controllerPage.Phase, "enter dissolve-in");
        AssertCursorHidden(controllerPage, "dissolve-in");

        var intro = NewMenu();
        AssertCursorVisible(intro, "primary selection before intro");
        intro.Step(ControllerButton.A);
        AssertEqual(GameOptionsPhase.FadeOutToIntro, intro.Phase, "enter intro fade-out");
        AssertCursorHidden(intro, "intro fade-out");
        Console.WriteLine("Options cursor: all 13 native entries, managed phase bounds, and no cursor through the dissolves and intro fade pass.");

        GameOptionsMenuState NewMenu()
        {
            var menu = new GameOptionsMenuState(AudioQueues.Standalone(), screenContent: installed, spriteSheets: MenuSheetFixtures.For(installed), controllerBindings: SuperMetroid.Core.Input.ControllerBindings.Default, iconCancelEnabled: false, moonwalkEnabled: false, language: TextLanguage.English);
            for (int frame = 0; frame < 20 && menu.Phase != GameOptionsPhase.Browsing; frame++)
                menu.Step(0);
            AssertTrue(menu.Phase == GameOptionsPhase.Browsing && menu.Page == GameOptionsPage.Primary, "options reaches primary page");
            return menu;
        }

        void AssertCursorVisible(GameOptionsMenuState menu, string context) =>
            AssertTrue(DrawsCursor(menu, installed), context);

        void AssertCursorHidden(GameOptionsMenuState menu, string context) =>
            AssertTrue(!DrawsCursor(menu, installed), context);

    }

    /// <summary>
    /// Whether <paramref name="menu"/> draws its missile cursor this frame: one of the options sheet's cursor frames,
    /// installed from <paramref name="presentation"/>, among the menu's sprite draws. A transition draws none (#1775), where
    /// the cartridge parks the cursor off screen.
    /// </summary>
    internal static bool DrawsCursor(GameOptionsMenuState menu, InstalledScreenContent presentation)
    {
        menu.ComposeScene();
        SpriteSheet sheet = MenuSheetFixtures.For(presentation).Get(SpriteSheetId.GameOptions);
        SpriteSheetFrame[] cursorFrames = [.. MenuMissileFrames.Names.Select(sheet.Frame)];
        return menu.screen.Sprites.Draws.OfType<ImageSpriteDraw>().Any(draw => cursorFrames.Contains(draw.Image));
    }

    /// <summary>Runs the native cursor-phase and rendered OAM check.</summary>
    [VerificationCommand("--game-options-cursor-phases")]
    internal static int GameOptionsCursorPhasesCommand()
    {
        Suite(nameof(VerifyGameOptionsCursorPhases), () => VerifyGameOptionsCursorPhases());
        return 0;
    }
}
