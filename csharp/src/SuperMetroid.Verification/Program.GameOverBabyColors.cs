using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGameOverBabyColors(ISnesAddressSpace rom)
    {
        var palettes = new Dictionary<string, ushort[]>();
        for (int phase = 0; phase < 4; phase++)
        {
            var colors = new ushort[16];
            for (int ink = 0; ink < 16; ink++)
                colors[ink] = ReadVerificationWord(rom, 0x82bd97 + phase * 32 + ink * 2);
            palettes.Add(GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)phase), colors);
        }
        var catalog = new GameOverBabyColorCatalog(palettes);
        byte[] json = GameOverPresentationExtractor.Extract(rom);
        using var stream = new MemoryStream(json);
        GameOverPresentation presentation = GameOverPresentation.Load(stream);
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(json)),
            presentation.ContentIdentity, "Baby presentation preserves content identity");
        var cgram = new SnesCgram();
        for (int phase = 0; phase < 4; phase++)
        {
            presentation.ApplyBabyPalette(cgram, (GameOverBabyPalette)phase);
            for (int ink = 0; ink < 16; ink++)
            {
                ushort expected = ReadVerificationWord(rom, 0x82bd97 + phase * 32 + ink * 2);
                AssertEqual(expected, catalog.Read((GameOverBabyPalette)phase, ink), "original Baby color");
                AssertEqual(expected, cgram.Colors[192 + ink], "Baby CGRAM application");
            }
        }
        // Every independently editable RGB5 component must survive import exactly,
        // including downstream targets whose input colors have changed.
        foreach (ushort[] edited in palettes.Values)
        for (int ink = 0; ink < 16; ink++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = edited[ink];
            for (int intensity = 0; intensity < 32; intensity++)
            {
                int shift = channel * 5;
                edited[ink] = (ushort)((original & ~(31 << shift)) | intensity << shift);
                var changed = new GameOverBabyColorCatalog(palettes);
                for (int phase = 0; phase < 4; phase++)
                for (int target = 0; target < 16; target++)
                    AssertEqual(palettes[GameOverPresentationDefinitions.BabyPaletteName((GameOverBabyPalette)phase)][target],
                        changed.Read((GameOverBabyPalette)phase, target), "independent Baby color edit");
            }
            edited[ink] = original;
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => catalog.Read(GameOverBabyPalette.Idle, invalid), "Baby ink bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Read((GameOverBabyPalette)invalid, 0), "Baby phase bounds");
    }
}
