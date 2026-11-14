using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

namespace SuperMetroid.Verification.Frontend.GameOver;

/// <summary>Checks game-over Baby Metroid palette phases against the cartridge.</summary>
internal static class GameOverBabyColorsVerification
{
    /// <summary>Compares four sixteen-color native phases with installed CGRAM slots and verifies ink and phase bounds and content identity.</summary>
    internal static void VerifyGameOverBabyColors(ISnesAddressSpace rom)
    {
        var palettes = new Dictionary<GameOverBabyColouring, ushort[]>();
        for (int phase = 0; phase < 4; phase++)
        {
            var colors = new ushort[16];
            for (int ink = 0; ink < 16; ink++)
                colors[ink] = ReadVerificationWord(rom, 0x82bd97 + phase * 32 + ink * 2);
            palettes.Add((GameOverBabyColouring)phase, colors);
        }
        var catalog = new GameOverBabyPaint(palettes.ToDictionary(entry => entry.Key, entry => ToColors(entry.Value)));
        byte[] json = GameOverPresentationExtractor.Extract(rom);
        using var stream = new MemoryStream(json);
        GameOverSourcePresentation presentation = GameOverSourcePresentation.Load(stream);
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(json)),
            presentation.ContentIdentity, "Baby presentation preserves content identity");
        // The Baby's colourings are shades of the installed game-over sheet (#1512): each recolours the Baby's colour
        // group, sprite colours 192..207, in the cartridge's phase colours.
        SpriteImage babySheet = MenuSheetFixtures.For(InstalledFixtures.Maps).Get(SpriteSheetId.GameOver)
            .Frame(GameOverPresentationNames.BabyFrameName(GameOverBabyFrame.Open)).Sheet;
        for (int phase = 0; phase < 4; phase++)
        {
            // The sheet's colours laid out by the palette index each was composed as (#1544).
            RgbColor[] shaded = SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.ByComposedIndex(babySheet,
                PictureShading.Shaded(GameOverBabyShades.Colours((GameOverBabyColouring)phase)).KeyColours(babySheet));
            RgbColor[] loaded = presentation.BabyPaint.Colours((GameOverBabyColouring)phase);
            for (int ink = 0; ink < 16; ink++)
            {
                ushort expected = ReadVerificationWord(rom, 0x82bd97 + phase * 32 + ink * 2);
                AssertEqual(expected, catalog.Read((GameOverBabyColouring)phase, ink), "original Baby color");
                AssertEqual(expected, loaded[ink], "Baby colours the import shades the sheet in");
                if (ink != 0 && SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.KeyComposedAs(babySheet, 64 + ink) is not null)
                    AssertEqual(expected, shaded[64 + ink], "installed Baby shade");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Read(GameOverBabyColouring.Idle, invalid), "Baby ink bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Read((GameOverBabyColouring)invalid, 0), "Baby phase bounds");
    }
}
