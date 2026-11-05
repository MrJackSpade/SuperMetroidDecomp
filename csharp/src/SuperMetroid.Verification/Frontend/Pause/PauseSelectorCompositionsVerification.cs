using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause equipment selector OAM against native sprites.</summary>
internal static class PauseSelectorCompositionsVerification
{
    /// <summary>Checks stock compact storage, native compositions and the phase bound.</summary>
    internal static void VerifyPauseSelectorCompositions(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var stock = PauseSelectorSourcePresentation.Load(new MemoryStream(bytes));
        for (int group = 0; group < 3; group++) VerifyPauseSelectorNativeCompositions(rom, stock, group);
        AssertEqual(0, stock.StoredCompositionOverrideCount, "stock selector has no phase composition table");
        AssertTrue(!stock.StoresCompositionParts, "stock selector stores no sprite part records");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Draw(ScreenSpritesFixtures.Blank(), MenuSheetFixtures.PauseSprites(stock), 0, 0, -1, PictureShading.Standard), "negative composition phase");
    }

    /// <summary>
    /// Compares each group's 14 phases with native OAM; the object limit that cuts a frame short in a nearly full sprite
    /// buffer is not replicated (#1513).
    /// </summary>
    internal static void VerifyPauseSelectorNativeCompositions(ISnesAddressSpace rom, PauseSelectorPresentation selector, int group)
    {
        int program = 0x820000 | ReadVerificationWord(rom, 0x82c0ec);
        int bases = 0x820000 | ReadVerificationWord(rom, 0x82c1e8);
        foreach (var anchor in PauseSelectorAnchors.Anchors().Where(a => Math.Min((int)a.Category, 2) == group))
        {
            int positions = 0x820000 | ReadVerificationWord(rom, 0x82c18e + (int)anchor.Category * 2);
            ushort x = (ushort)(ReadVerificationWord(rom, positions + anchor.Item * 4) - 1);
            ushort y = (ushort)(ReadVerificationWord(rom, positions + anchor.Item * 4 + 2) - 1);
            for (int phase = 0; phase < 14; phase++)
            {
                int sprite = ReadVerificationWord(rom, bases + (int)anchor.Category * 2) + rom.ReadCartridgeByte(program + phase * 3 + 2);
                int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + sprite * 2);
                var expected = new OamBuffer(); var actual = ScreenSpritesFixtures.Blank(); expected.BeginFrame(); actual.BeginFrame();
                DrawImportedSpritemap(rom, expected, pointer, new(x, y), 0x600);
                SpriteSheet sheet = MenuSheetFixtures.PauseSprites(selector);
                selector.Draw(actual, sheet, anchor.Category, anchor.Item, phase, PictureShading.Standard);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.ImageEquals(actual),
                    "native selector composition, attributes and order");
            }
        }
    }
}
