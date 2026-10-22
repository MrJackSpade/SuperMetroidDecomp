using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Options;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks the options cursor's native phase selection and visible OAM transitions.</summary>
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
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.FadeIn, 2), new GameOptionsCursorPhasesPhase(GameOptionsPhase.Main, 3),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.DissolveOut, 5), new GameOptionsCursorPhasesPhase(GameOptionsPhase.DissolveIn, 6),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.ControllerSettings, 7), new GameOptionsCursorPhasesPhase(GameOptionsPhase.SpecialSettings, 8),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.ScrollControllerDown, 9), new GameOptionsCursorPhasesPhase(GameOptionsPhase.ScrollControllerUp, 10),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.FadeOutToFileSelect, 11), new GameOptionsCursorPhasesPhase(GameOptionsPhase.FadeOutToIntro, 12),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.StartGame, 4),
            new GameOptionsCursorPhasesPhase(GameOptionsPhase.FinishFadingOut, 0), new GameOptionsCursorPhasesPhase(GameOptionsPhase.LoadingMenu, 1),
        ];
        foreach (var phase in phases)
        {
            ushort pointer = GameOptionsCursorPolicy.Select(phase.Phase) switch
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
            AssertThrows<ArgumentOutOfRangeException>(() => GameOptionsCursorPolicy.Select((GameOptionsPhase)invalid),
                "unsupported options cursor phase");
        var installed = RetailPresentationFixture();
        var controllerPage = NewMenu();
        for (int row = 0; row < (int)GameOptionsPrimaryRow.ControllerSettings; row++)
        {
            controllerPage.Step(SnesButton.Down);
            controllerPage.Step(0);
        }
        AssertCursorVisible(controllerPage, "primary selection before dissolve");
        controllerPage.Step(SnesButton.A);
        AssertEqual(GameOptionsPhase.DissolveOut, controllerPage.Phase, "enter dissolve-out");
        AssertCursorHidden(controllerPage, "dissolve-out");
        for (int frame = 0; frame < 20 && controllerPage.Phase != GameOptionsPhase.DissolveIn; frame++)
            controllerPage.Step(0);
        AssertEqual(GameOptionsPhase.DissolveIn, controllerPage.Phase, "enter dissolve-in");
        AssertCursorHidden(controllerPage, "dissolve-in");

        var intro = NewMenu();
        AssertCursorVisible(intro, "primary selection before intro");
        intro.Step(SnesButton.A);
        AssertEqual(GameOptionsPhase.FadeOutToIntro, intro.Phase, "enter intro fade-out");
        AssertCursorHidden(intro, "intro fade-out");
        Console.WriteLine("Options cursor: all 13 native entries, managed phase bounds and real dissolve/intro OAM transitions pass.");

        GameOptionsMenuState NewMenu()
        {
            var menu = new GameOptionsMenuState(mapPresentation: installed, spriteSheets: MenuSheetFixtures.For(installed));
            for (int frame = 0; frame < 20 && menu.Phase != GameOptionsPhase.Main; frame++)
                menu.Step(0);
            AssertEqual(GameOptionsPhase.Main, menu.Phase, "options reaches primary page");
            return menu;
        }

        static void AssertCursorVisible(GameOptionsMenuState menu, string context) =>
            AssertTrue(LastSpriteX(menu.CaptureRenderSnapshot()) < 256, context);

        static void AssertCursorHidden(GameOptionsMenuState menu, string context) =>
            AssertTrue(LastSpriteX(menu.CaptureRenderSnapshot()) >= 256, context);

    }

    /// <summary>
    /// The final sprite part's nine-bit OAM X, because the cursor follows the heading; native X=$0180 hides it.
    /// </summary>
    internal static int LastSpriteX(LayeredRenderSnapshot frame)
    {
        PlacedSpritePart[] parts = frame.Memory.SpriteParts();
        AssertTrue(parts.Length > 0, "options has a cursor sprite");
        return parts[^1].Part.Position.X & 511;
    }

    /// <summary>Runs the native cursor-phase and rendered OAM check.</summary>
    [VerificationCommand("--game-options-cursor-phases")]
    internal static int GameOptionsCursorPhasesCommand()
    {
        Suite(nameof(VerifyGameOptionsCursorPhases), () => VerifyGameOptionsCursorPhases());
        return 0;
    }
}
