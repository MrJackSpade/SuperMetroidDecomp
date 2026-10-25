using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Frontend.Options;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks the three game-options heading sprites and placement anchors against cartridge instructions.</summary>
internal static class GameOptionsHeadingsVerification
{
    /// <summary>Loads extracted presentation and checks each heading's native identity, parts, and screen anchor.</summary>
    internal static void VerifyGameOptionsHeadings()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Options headings oracle revision");
        byte[] json = GameOptionsPresentationExtractor.Extract(rom);
        _ = GameOptionsPresentation.Load(new MemoryStream(json));
        var document = RequiredJson.Deserialize<GameOptionsPresentationDocument>(json, ArtworkJson.Options);
        Suite(nameof(VerifyGameOptionsHeadingIds), () => VerifyGameOptionsHeadingIds(rom, document));
        Suite(nameof(VerifyGameOptionsHeadingAnchors), () => VerifyGameOptionsHeadingAnchors(rom, document));
    }

    /// <summary>Maps the three native options pages to their editable heading identities.</summary>
    private static GameOptionsPage OriginalHeadingPage(int index) => index switch
    {
        0 => GameOptionsPage.Primary,
        1 => GameOptionsPage.Controller,
        2 => GameOptionsPage.Special,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Checks heading table entries, both instruction-list uses, and imported part counts.</summary>
    private static void VerifyGameOptionsHeadingIds(ISnesAddressSpace rom, GameOptionsPresentationDocument document)
    {
        // The original table-entry locations independently identify the native IDs;
        // instruction lists store pointers, so also resolve both displayed records.
        int[] entries = [0x82c5ff, 0x82c601, 0x82c603];
        for (int page = 0; page < 3; page++)
        {
            ushort id = GameOptionsSourceDefinitions.HeadingSpritemaps[(GameOptionsPage)page];
            AssertEqual(entries[page], 0x82c569 + 2 * id, "original heading table entry identity");
            ushort pointer = ReadVerificationWord(rom, entries[page]);
            int list = 0x82f47e + 16 * page;
            AssertEqual(pointer, ReadVerificationWord(rom, list + 2), "initial native heading record");
            AssertEqual(pointer, ReadVerificationWord(rom, list + 10), "repeated native heading record");
            string name = GameOptionsPresentationDefinitions.HeadingSpriteName(OriginalHeadingPage(page));
            AssertEqual(ReadVerificationWord(rom, 0x820000 | pointer), document.Sprites[name].Length,
                "imported original heading part count");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 65536, int.MaxValue })
            AssertThrows<KeyNotFoundException>(() => _ = GameOptionsSourceDefinitions.HeadingSpritemaps[(GameOptionsPage)invalid],
                "heading sprite selector bounds");
    }

    /// <summary>Checks per-page X and shared Y setup operands against imported drawing anchors.</summary>
    private static void VerifyGameOptionsHeadingAnchors(ISnesAddressSpace rom, GameOptionsPresentationDocument document)
    {
        AssertEqual((byte)0xa9, GameOptionsHeadingAnchorsRomByte(rom, 0x82f369), "native heading common Y LDA");
        ushort y = ReadVerificationWord(rom, 0x82f36a);
        AssertEqual(y, GameOptionsDefinitions.Headings.HeadingY, "native common heading Y");
        for (int page = 0; page < 3; page++)
        {
            int setup = 0x82f34b + 8 * page;
            AssertEqual((byte)0xa9, GameOptionsHeadingAnchorsRomByte(rom, setup), "native heading X LDA");
            ushort x = ReadVerificationWord(rom, setup + 1);
            AssertEqual(x, GameOptionsLayout.HeadingX((GameOptionsPage)page), "native heading X case");
            Position16<SpritePixels> actual = document.HeadingAnchors[GameOptionsPresentationDefinitions.MenuName(OriginalHeadingPage(page))];
            AssertEqual((int)x, actual.X, "imported heading X");
            AssertEqual((int)y, actual.Y, "imported heading Y");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => GameOptionsLayout.HeadingX((GameOptionsPage)invalid),
                "heading anchor selector bounds");
    }

    /// <summary>Reads a native heading setup opcode for the cartridge comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for game options heading anchors")]
    private static byte GameOptionsHeadingAnchorsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the game-options heading sprite and anchor checks.</summary>
    [VerificationCommand("--lookup-options-headings")]
    internal static int LookupOptionsHeadingsCommand()
    {
        Suite(nameof(VerifyGameOptionsHeadings), () => VerifyGameOptionsHeadings());
        Console.WriteLine("Options headings: all three original sprite selections and anchor pairs, imported records and enum bounds pass.");
        return 0;
    }
}
