using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Frontend.Options;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>Checks installed game-options pages against five native source selections and page bytes.</summary>
internal static class GameOptionsPageCasesVerification
{
    /// <summary>Compares page addresses, descriptions, and decompressed tilemaps with the pinned ROM.</summary>
    internal static void VerifyGameOptionsPageCases()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Options page oracle revision");
        GameOptionsPresentation presentation = GameOptionsSourcePresentation.Load(
            new MemoryStream(GameOptionsPresentationExtractor.Extract(rom)));
        OriginalOrder[] originalOrder =
        [
            new OriginalOrder(GameOptionsTilemap.Primary, "primary"),
            new OriginalOrder(GameOptionsTilemap.ControllerEnglish, "English controller"),
            new OriginalOrder(GameOptionsTilemap.ControllerJapanese, "Japanese controller"),
            new OriginalOrder(GameOptionsTilemap.SpecialEnglish, "English special-settings"),
            new OriginalOrder(GameOptionsTilemap.SpecialJapanese, "Japanese special-settings"),
        ];
        for (int index = 0; index < originalOrder.Length; index++)
        {
            int instruction = 0x82ec66 + 17 * index;
            AssertEqual((byte)0xa9, CpuBusRegionRead.Read(rom, instruction), "native page bank LDA immediate");
            AssertEqual((byte)0xa9, CpuBusRegionRead.Read(rom, instruction + 5), "native page address LDA immediate");
            int source = (int)new SnesAddress(LittleEndianBytes.HighByte(ReadVerificationWord(rom, instruction + 1)), ReadVerificationWord(rom, instruction + 6));
            var expected = originalOrder[index];
            GameOptionsPageResource actual = GameOptionsSourceDefinitions.Pages[expected.Page];
            AssertEqual(source, actual.Address, "original options resource address");
            AssertEqual(expected.Description, actual.Description, "preserved resource diagnostic name");
            byte[] bytes = RomDataReader.Decompress(CartridgeImportSource.Require(rom), source, maximumOutputBytes: 0x800);
            AssertEqual(0x800, bytes.Length, "original options page size");
            AssertTrue(bytes.SequenceEqual(presentation.CreatePageBytes(expected.Page)),
                "named installed page matches independently selected native resource");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, 65536, int.MaxValue })
            AssertThrows<KeyNotFoundException>(() => _ = GameOptionsSourceDefinitions.Pages[(GameOptionsTilemap)invalid],
                "unsupported options tilemap selector");
    }

    /// <summary>Runs the focused game-options page-source check.</summary>
    [VerificationCommand("--lookup-options-pages")]
    internal static int LookupOptionsPagesCommand()
    {
        Suite(nameof(VerifyGameOptionsPageCases), () => VerifyGameOptionsPageCases());
        Console.WriteLine("Options pages: all five native source pairs, named imported page bytes, descriptions and rejected selectors pass.");
        return 0;
    }
}
