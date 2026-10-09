using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Imports the three Game Options heading sprites from the pinned cartridge and verifies their IDs and anchors.</summary>
    private static void VerifyGameOptionsHeadings()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Options headings oracle revision");
        byte[] json = GameOptionsPresentationExtractor.Extract(rom);
        _ = GameOptionsPresentation.Load(new MemoryStream(json));
        var document = JsonSerializer.Deserialize<GameOptionsPresentationDocument>(json, MapPresentationFormat.JsonOptions)!;
        Suite(nameof(VerifyGameOptionsHeadingIds), () => VerifyGameOptionsHeadingIds(rom, document));
        Suite(nameof(VerifyGameOptionsHeadingAnchors), () => VerifyGameOptionsHeadingAnchors(rom, document));
    }

    /// <summary>Maps an ordinal Game Options page to the corresponding original heading asset name.</summary>
    /// <param name="index">Page ordinal: primary menu, controller menu, or special menu.</param>
    /// <returns>The stable asset name used to locate the imported heading.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The ordinal is not one of the three Game Options pages.</exception>
    private static string OriginalHeadingName(int index) => index switch
    {
        0 => GameOptionsPresentationDefinitions.PrimaryMenu,
        1 => GameOptionsPresentationDefinitions.ControllerMenu,
        2 => GameOptionsPresentationDefinitions.SpecialMenu,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Checks cartridge heading IDs, native instruction-list pointers, imported sprite parts, and selector bounds.</summary>
    /// <param name="rom">Pinned cartridge address space used as the native reference.</param>
    /// <param name="document">Extracted presentation document containing the imported heading sprites.</param>
    private static void VerifyGameOptionsHeadingIds(ISnesAddressSpace rom, GameOptionsPresentationDocument document)
    {
        // The original table-entry locations independently identify the native IDs;
        // instruction lists store pointers, so also resolve both displayed records.
        int[] entries = [0x82c5ff, 0x82c601, 0x82c603];
        for (int page = 0; page < 3; page++)
        {
            ushort id = GameOptionsRomData.Spritemaps.Heading((GameOptionsPage)page);
            AssertEqual(entries[page], 0x82c569 + 2 * id, "original heading table entry identity");
            ushort pointer = ReadVerificationWord(rom, entries[page]);
            int list = 0x82f47e + 16 * page;
            AssertEqual(pointer, ReadVerificationWord(rom, list + 2), "initial native heading record");
            AssertEqual(pointer, ReadVerificationWord(rom, list + 10), "repeated native heading record");
            string name = GameOptionsPresentationDefinitions.HeadingFrameName(OriginalHeadingName(page));
            AssertEqual((int)ReadVerificationWord(rom, 0x820000 | pointer), document.Sprites[name].Length,
                "imported original heading part count");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameOptionsRomData.Spritemaps.Heading((GameOptionsPage)invalid),
                "heading sprite selector bounds");
    }

    /// <summary>Checks native heading coordinate operands against their decoded definitions and imported anchor points.</summary>
    /// <param name="rom">Pinned cartridge address space containing the heading setup instructions.</param>
    /// <param name="document">Extracted presentation document whose heading anchors are compared with the cartridge.</param>
    private static void VerifyGameOptionsHeadingAnchors(ISnesAddressSpace rom, GameOptionsPresentationDocument document)
    {
        AssertEqual((byte)0xa9, rom.ReadByte(0x82f369), "native heading common Y LDA");
        ushort y = ReadVerificationWord(rom, 0x82f36a);
        AssertEqual(y, GameOptionsRomData.Spritemaps.HeadingY, "native common heading Y");
        for (int page = 0; page < 3; page++)
        {
            int setup = 0x82f34b + 8 * page;
            AssertEqual((byte)0xa9, rom.ReadByte(setup), "native heading X LDA");
            ushort x = ReadVerificationWord(rom, setup + 1);
            AssertEqual(x, GameOptionsRomData.Spritemaps.HeadingX((GameOptionsPage)page), "native heading X case");
            MapLabelPoint actual = document.HeadingAnchors[OriginalHeadingName(page)];
            AssertEqual((int)x, actual.X, "imported heading X");
            AssertEqual((int)y, actual.Y, "imported heading Y");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameOptionsRomData.Spritemaps.HeadingX((GameOptionsPage)invalid),
                "heading anchor selector bounds");
    }
}
