using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks all five installed game-options pages against their original retail resources and confirms unsupported page selectors are rejected.</summary>
    private static void VerifyGameOptionsPageCases()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Options page oracle revision");
        GameOptionsPresentation presentation = GameOptionsPresentation.Load(
            new MemoryStream(GameOptionsPresentationExtractor.Extract(rom)));
        (GameOptionsTilemap Page, string Asset, string Description)[] originalOrder =
        [
            (GameOptionsTilemap.Primary, GameOptionsPresentationDefinitions.PrimaryPage, "primary"),
            (GameOptionsTilemap.ControllerEnglish, GameOptionsPresentationDefinitions.ControllerEnglishPage, "English controller"),
            (GameOptionsTilemap.ControllerJapanese, GameOptionsPresentationDefinitions.ControllerJapanesePage, "Japanese controller"),
            (GameOptionsTilemap.SpecialEnglish, GameOptionsPresentationDefinitions.SpecialEnglishPage, "English special-settings"),
            (GameOptionsTilemap.SpecialJapanese, GameOptionsPresentationDefinitions.SpecialJapanesePage, "Japanese special-settings"),
        ];
        for (int index = 0; index < originalOrder.Length; index++)
        {
            int instruction = 0x82ec66 + 17 * index;
            AssertEqual((byte)0xa9, rom.ReadByte(instruction), "native page bank LDA immediate");
            AssertEqual((byte)0xa9, rom.ReadByte(instruction + 5), "native page address LDA immediate");
            int source = (ReadVerificationWord(rom, instruction + 1) & 0xff00) << 8 |
                ReadVerificationWord(rom, instruction + 6);
            var expected = originalOrder[index];
            GameOptionsPageResource actual = GameOptionsRomData.Pages.Get(expected.Page);
            AssertEqual(source, actual.Address, "original options resource address");
            AssertEqual(expected.Description, actual.Description, "preserved resource diagnostic name");
            byte[] bytes = RomDataReader.Decompress(CartridgeImportSource.Require(rom), source, maximumOutputBytes: 0x800);
            AssertEqual(0x800, bytes.Length, "original options page size");
            AssertTrue(bytes.AsSpan().SequenceEqual(presentation.CreatePage(expected.Asset)),
                "named installed page matches independently selected native resource");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameOptionsRomData.Pages.Get((GameOptionsTilemap)invalid),
                "unsupported options tilemap selector");
    }
}
