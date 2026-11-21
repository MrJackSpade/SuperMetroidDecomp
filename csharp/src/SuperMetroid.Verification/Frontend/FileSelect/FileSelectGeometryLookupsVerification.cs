using SuperMetroid.AssetExtraction.Frontend.FileSelect;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

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
        FileSelectExtraction extraction = FileSelectPresentationExtractor.Extract(rom);
        FileSelectPresentation presentation = FileSelectSourcePresentationConstants.Load(extraction).Presentation;
        FileSelectLayoutDocument document = RequiredJson.Deserialize<FileSelectLayoutDocument>(
            extraction.Layout, ArtworkJson.Options);
        Suite(nameof(VerifyFileSelectMainCursorY), () => VerifyFileSelectMainCursorY(rom, presentation));
        Suite(nameof(VerifyFileSelectDataCursorY), () => VerifyFileSelectDataCursorY(rom, presentation));
        Suite(nameof(VerifyFileSelectHelmetY), () => VerifyFileSelectHelmetY(rom, document));
        Console.WriteLine("File select geometry: six main rows, both four-row data views, three helmet anchors, extracted output and bounds pass.");
    }

    /// <summary>Compares six main-menu cursor rows with native records and extracted positions.</summary>
    private static void VerifyFileSelectMainCursorY(ISnesAddressSpace rom, FileSelectPresentation presentation)
    {
        AssertEqual(6, FileSelectCursorPositionsConstants.MainSelectionCount, "main selection count");
        for (int row = 0; row < 6; row++)
        {
            ushort expected = ReadVerificationWord(rom, 0x81a312 + row * 4);
            AssertEqual(expected, FileSelectCursorPositions.MainSelectionY(FileSelectItemRows.MainWithData[row]), $"main cursor Y {row}");
            AssertEqual(expected, presentation.CursorPosition(FileSelectCursorPage.Main, FileSelectItemRows.MainWithData[row]).Y,
                $"extracted main cursor Y {row}");
        }
        Suite(nameof(VerifyFileSelectGeometryBounds), () => VerifyFileSelectGeometryBounds(FileSelectCursorPositions.MainSelectionY, [(FileSelectItem)99]));
    }

    /// <summary>Compares four data-menu cursor rows from both native views with extracted positions.</summary>
    private static void VerifyFileSelectDataCursorY(ISnesAddressSpace rom, FileSelectPresentation presentation)
    {
        AssertEqual(4, FileSelectCursorPositionsConstants.DataSelectionCount, "data selection count");
        AssertEqual(FileSelectCursorPositionsConstants.DataSelectionCount, FileSelectItemRows.Data.Count, "the data pages show one row per cursor anchor");
        foreach (int address in new[] { 0x819772, 0x819c03 })
        for (int row = 0; row < 4; row++)
        {
            ushort expected = ReadVerificationWord(rom, address + row * 2);
            AssertEqual(expected, FileSelectCursorPositions.DataSelectionY(FileSelectItemRows.Data[row]), $"data cursor {address:X6}/{row}");
            AssertEqual(expected, presentation.CursorPosition(FileSelectCursorPage.DataManagement, FileSelectItemRows.Data[row]).Y,
                $"extracted data cursor {address:X6}/{row}");
        }
        Suite(nameof(VerifyFileSelectGeometryBounds), () => VerifyFileSelectGeometryBounds(FileSelectCursorPositions.DataSelectionY, [FileSelectItem.Copy, FileSelectItem.Clear, (FileSelectItem)99]));
    }

    /// <summary>Compares three helmet anchors with native immediate operands and extracted artwork positions.</summary>
    private static void VerifyFileSelectHelmetY(ISnesAddressSpace rom, FileSelectLayoutDocument document)
    {
        AssertEqual(3, document.HelmetAnchors.Count, "extracted helmet anchor count");
        for (int slot = 0; slot < 3; slot++)
        {
            int instruction = 0x81a027 + slot * 6;
            AssertEqual((byte)0xa9, FileSelectHelmetYRomByte(rom, instruction), "helmet LDA immediate opcode");
            AssertEqual((byte)0x8d, FileSelectHelmetYRomByte(rom, instruction + 3), "helmet STA absolute opcode");
            ushort expected = ReadVerificationWord(rom, instruction + 1);
            AssertEqual(expected, FileSelectCursorPositions.HelmetY(FileSelectCursorPositions.Slots[slot]), $"helmet Y {slot}");
            AssertEqual(expected, document.HelmetAnchors[FileSelectCursorPositions.Slots[slot].ToString()].Y, $"extracted helmet Y {slot}");
        }
        Suite(nameof(VerifyFileSelectGeometryBounds), () => VerifyFileSelectGeometryBounds(FileSelectCursorPositions.HelmetY, [(SuperMetroid.Core.Saves.SaveSlot)99]));
    }

    /// <summary>Reads a native helmet placement opcode as the cartridge comparison oracle.</summary>
    private static byte FileSelectHelmetYRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that each indexed geometry lookup rejects values outside its native row count.</summary>
    private static void VerifyFileSelectGeometryBounds<TRow>(Func<TRow, int> lookup, TRow[] notOnThePage)
    {
        foreach (TRow row in notOnThePage)
            AssertThrows<ArgumentOutOfRangeException>(() => lookup(row), $"unsupported geometry row {row}");
    }

    /// <summary>Runs the file-select cursor and helmet geometry checks.</summary>
    [VerificationCommand("--lookup-file-select-geometry")]
    internal static int LookupFileSelectGeometryCommand()
    {
        Suite(nameof(VerifyFileSelectGeometryLookups), () => VerifyFileSelectGeometryLookups());
        return 0;
    }
}
