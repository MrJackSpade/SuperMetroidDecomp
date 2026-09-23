using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyGameOptionsCursorPhases()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
        Console.WriteLine("Options cursor: the native zero-pointer phases move its OAM actor offscreen.");

        GameOptionsMenuState NewMenu()
        {
            var menu = new GameOptionsMenuState(bus);
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
    private static int LastSpriteX(LayeredRenderSnapshot frame)
    {
        int sprite = frame.Memory.ModeledSpriteCount - 1;
        AssertTrue(sprite >= 0, "options has a cursor OAM entry");
        ReadOnlySpan<byte> oam = frame.Memory.Oam;
        int highBits = oam[512 + sprite / 4] >> (2 * (sprite % 4));
        return oam[sprite * 4] | ((highBits & 1) << 8);
    }
}
