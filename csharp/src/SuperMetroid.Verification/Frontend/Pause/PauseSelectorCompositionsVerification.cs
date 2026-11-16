using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause equipment selector OAM against native sprites.</summary>
internal static class PauseSelectorCompositionsVerification
{
    /// <summary>Checks the installed compositions against the cartridge's and the phase bound.</summary>
    internal static void VerifyPauseSelectorCompositions(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var stock = PauseSelectorSourcePresentation.Load(new MemoryStream(bytes));
        for (int group = 0; group < 3; group++) VerifyPauseSelectorNativeCompositions(rom, stock, group);
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Presentation.Draw(ScreenSpritesFixtures.Blank(), MenuSheetFixtures.PauseSprites(stock), PauseEquipmentControls.ReserveMode, -1), "negative composition phase");
    }

    /// <summary>
    /// Compares each group's 14 phases with native OAM; the object limit that cuts a frame short in a nearly full sprite
    /// buffer is not replicated (#1513).
    /// </summary>
    internal static void VerifyPauseSelectorNativeCompositions(ISnesAddressSpace rom, PauseSelectorSourcePresentation selector, int group)
    {
        int program = 0x820000 | ReadVerificationWord(rom, 0x82c0ec);
        int bases = 0x820000 | ReadVerificationWord(rom, 0x82c1e8);
        var columns = PauseMenuSourceDefinitions.SelectorColumns;
        // Suits and boots share the third group's frames.
        for (int table = 0; table < columns.Length; table++)
        for (int item = 0; item < columns[table].Length; item++)
        {
            if (Math.Min(table, 2) != group)
                continue;
            int positions = 0x820000 | ReadVerificationWord(rom, 0x82c18e + table * 2);
            ushort x = (ushort)(ReadVerificationWord(rom, positions + item * 4) - 1);
            ushort y = (ushort)(ReadVerificationWord(rom, positions + item * 4 + 2) - 1);
            for (int phase = 0; phase < 14; phase++)
            {
                int sprite = ReadVerificationWord(rom, bases + table * 2) + rom.ReadCartridgeByte(program + phase * 3 + 2);
                int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + sprite * 2);
                var expected = new OamBuffer(); var actual = ScreenSpritesFixtures.Blank(); expected.BeginFrame(); actual.BeginFrame();
                DrawImportedSpritemap(rom, expected, pointer, new(x, y), 0x600);
                SpriteSheet sheet = MenuSheetFixtures.PauseSprites(selector);
                selector.Presentation.Draw(actual, sheet, columns[table][item], phase);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.ImageEquals(actual),
                    "native selector composition, attributes and order");
            }
        }
    }
}
