using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks file-select cursor rows and helmet anchors against native geometry and extracted presentation.</summary>
internal static class FileSelectGeometryLookupsVerification
{
    /// <summary>Runs cartridge comparisons for the main, data, and helmet positions using the pinned ROM revision.</summary>
    internal static void VerifyFileSelectGeometryLookups()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),
            "file select geometry oracle revision");
        byte[] bytes = FileSelectPresentationExtractor.Extract(rom);
        FileSelectPresentation presentation = FileSelectPresentation.Load(new MemoryStream(bytes));
        FileSelectPresentationDocument document = RequiredJson.Deserialize<FileSelectPresentationDocument>(
            bytes, ArtworkJson.Options);
        Suite(nameof(VerifyFileSelectMainCursorY), () => VerifyFileSelectMainCursorY(rom, presentation));
        Suite(nameof(VerifyFileSelectDataCursorY), () => VerifyFileSelectDataCursorY(rom, presentation));
        Suite(nameof(VerifyFileSelectHelmetY), () => VerifyFileSelectHelmetY(rom, document));
        Console.WriteLine("File select geometry: six main rows, both four-row data views, three helmet anchors, extracted output and bounds pass.");
    }

    /// <summary>Compares six main-menu cursor rows with native records and extracted positions.</summary>
    private static void VerifyFileSelectMainCursorY(ISnesAddressSpace rom, FileSelectPresentation presentation)
    {
        AssertEqual(6, FileSelectLayout.MainSelectionCount, "main selection count");
        for (int row = 0; row < 6; row++)
        {
            ushort expected = ReadVerificationWord(rom, 0x81a312 + row * 4);
            AssertEqual(expected, FileSelectLayout.MainSelectionY(row), $"main cursor Y {row}");
            AssertEqual((int)expected, presentation.CursorPosition(FileSelectCursorPage.Main, row).Y,
                $"extracted main cursor Y {row}");
        }
        Suite(nameof(VerifyFileSelectGeometryBounds), () => VerifyFileSelectGeometryBounds(FileSelectLayout.MainSelectionY, 6));
    }

    /// <summary>Compares four data-menu cursor rows from both native views with extracted positions.</summary>
    private static void VerifyFileSelectDataCursorY(ISnesAddressSpace rom, FileSelectPresentation presentation)
    {
        AssertEqual(4, FileSelectLayout.DataSelectionCount, "data selection count");
        foreach (int address in new[] { 0x819772, 0x819c03 })
        for (int row = 0; row < 4; row++)
        {
            ushort expected = ReadVerificationWord(rom, address + row * 2);
            AssertEqual(expected, FileSelectLayout.DataSelectionY(row), $"data cursor {address:X6}/{row}");
            AssertEqual((int)expected, presentation.CursorPosition(FileSelectCursorPage.DataManagement, row).Y,
                $"extracted data cursor {address:X6}/{row}");
        }
        Suite(nameof(VerifyFileSelectGeometryBounds), () => VerifyFileSelectGeometryBounds(FileSelectLayout.DataSelectionY, 4));
    }

    /// <summary>Compares three helmet anchors with native immediate operands and extracted artwork positions.</summary>
    private static void VerifyFileSelectHelmetY(ISnesAddressSpace rom, FileSelectPresentationDocument document)
    {
        AssertEqual(3, FileSelectLayout.SaveSlotCount, "helmet slot count");
        AssertEqual(3, document.HelmetAnchors.Length, "extracted helmet anchor count");
        for (int slot = 0; slot < 3; slot++)
        {
            int instruction = 0x81a027 + slot * 6;
            AssertEqual((byte)0xa9, FileSelectHelmetYRomByte(rom, instruction), "helmet LDA immediate opcode");
            AssertEqual((byte)0x8d, FileSelectHelmetYRomByte(rom, instruction + 3), "helmet STA absolute opcode");
            ushort expected = ReadVerificationWord(rom, instruction + 1);
            AssertEqual(expected, FileSelectLayout.HelmetY(slot), $"helmet Y {slot}");
            AssertEqual((int)expected, document.HelmetAnchors[slot].Y, $"extracted helmet Y {slot}");
        }
        Suite(nameof(VerifyFileSelectGeometryBounds), () => VerifyFileSelectGeometryBounds(FileSelectLayout.HelmetY, 3));
    }

    /// <summary>Reads a native helmet placement opcode as the cartridge comparison oracle.</summary>
    private static byte FileSelectHelmetYRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that each indexed geometry lookup rejects values outside its native row count.</summary>
    private static void VerifyFileSelectGeometryBounds(Func<int, ushort> lookup, int count)
    {
        foreach (int index in new[] { int.MinValue, -1, count, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => lookup(index), $"unsupported geometry index {index}");
    }

    /// <summary>Runs the file-select cursor and helmet geometry checks.</summary>
    [VerificationCommand("--lookup-file-select-geometry")]
    internal static int LookupFileSelectGeometryCommand()
    {
        Suite(nameof(VerifyFileSelectGeometryLookups), () => VerifyFileSelectGeometryLookups());
        return 0;
    }
}
