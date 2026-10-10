using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>Checks native cursor-page selection for each managed options phase and cursor visibility during page transitions.</summary>
    private static void VerifyGameOptionsCursorPhases()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Options cursor oracle revision");
        (GameOptionsPhase Phase, int Native)[] phases =
        [
            (GameOptionsPhase.FadeIn, 2), (GameOptionsPhase.Main, 3),
            (GameOptionsPhase.DissolveOut, 5), (GameOptionsPhase.DissolveIn, 6),
            (GameOptionsPhase.ControllerSettings, 7), (GameOptionsPhase.SpecialSettings, 8),
            (GameOptionsPhase.ScrollControllerDown, 9), (GameOptionsPhase.ScrollControllerUp, 10),
            (GameOptionsPhase.FadeOutToFileSelect, 11), (GameOptionsPhase.FadeOutToIntro, 12),
            (GameOptionsPhase.StartGame, 4),
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
        foreach (int lifecycle in new[] { 0, 1 })
            AssertEqual((ushort)0, ReadVerificationWord(bus, 0x82f2ed + lifecycle * 2),
                "unrepresented native lifecycle phase has no cursor page");
        foreach (int invalid in new[] { int.MinValue, -1, 11, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameOptionsCursorPolicy.Select((GameOptionsPhase)invalid),
                "unsupported options cursor phase");
        var installed = RetailPresentationFixture();
        var controllerPage = NewMenu();
        for (int row = 0; row < GameOptionsRomData.Rows.PrimaryControllerSettings; row++)
        {
            controllerPage.Step((ushort)SnesButton.Down);
            controllerPage.Step(0);
        }
        AssertCursorVisible(controllerPage, "primary selection before dissolve");
        controllerPage.Step((ushort)SnesButton.A);
        AssertEqual(GameOptionsPhase.DissolveOut, controllerPage.Phase, "enter dissolve-out");
        AssertCursorHidden(controllerPage, "dissolve-out");
        for (int frame = 0; frame < 20 && controllerPage.Phase != GameOptionsPhase.DissolveIn; frame++)
            controllerPage.Step(0);
        AssertEqual(GameOptionsPhase.DissolveIn, controllerPage.Phase, "enter dissolve-in");
        AssertCursorHidden(controllerPage, "dissolve-in");

        var intro = NewMenu();
        AssertCursorVisible(intro, "primary selection before intro");
        intro.Step((ushort)SnesButton.A);
        AssertEqual(GameOptionsPhase.FadeOutToIntro, intro.Phase, "enter intro fade-out");
        AssertCursorHidden(intro, "intro fade-out");
        Console.WriteLine("Options cursor: all 13 native entries, managed phase bounds and real dissolve/intro OAM transitions pass.");

        GameOptionsMenuState NewMenu()
        {
            var menu = new GameOptionsMenuState(bus, mapPresentation: installed);
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

    // The cursor spritemap is appended after the heading; its final OAM entry
    // suffices to check that the native X=$0180 anchor is outside the viewport.
    /// <summary>Reads the nine-bit horizontal coordinate of the final sprite entry in a captured OAM table.</summary>
    /// <param name="frame">Captured menu frame whose modeled sprite count identifies the final OAM entry.</param>
    /// <returns>The final sprite's horizontal position, including its OAM high-table bit.</returns>
    private static int LastSpriteX(LayeredRenderSnapshot frame)
    {
        int sprite = frame.Memory.ModeledSpriteCount - 1;
        AssertTrue(sprite >= 0, "options has a cursor OAM entry");
        ReadOnlySpan<byte> oam = frame.Memory.Oam;
        int highBits = oam[512 + sprite / 4] >> (2 * (sprite % 4));
        return oam[sprite * 4] | ((highBits & 1) << 8);
    }
}
