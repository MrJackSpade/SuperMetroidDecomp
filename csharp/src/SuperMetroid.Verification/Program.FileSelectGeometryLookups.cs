using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFileSelectGeometryLookups()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),
            "file select geometry oracle revision");
        byte[] bytes = FileSelectPresentationExtractor.Extract(rom);
        FileSelectPresentation presentation = FileSelectPresentation.Load(new MemoryStream(bytes));
        FileSelectPresentationDocument document = JsonSerializer.Deserialize<FileSelectPresentationDocument>(
            bytes, MapPresentationFormat.JsonOptions)!;
        VerifyFileSelectMainCursorY(rom, presentation);
        VerifyFileSelectDataCursorY(rom, presentation);
        VerifyFileSelectHelmetY(rom, document);
        Console.WriteLine("File select geometry: six main rows, both four-row data views, three helmet anchors, extracted output and bounds pass.");
    }

    private static void VerifyFileSelectMainCursorY(ISnesAddressSpace rom, FileSelectPresentation presentation)
    {
        AssertEqual(6, FileSelectLayout.MainSelectionCount, "main selection count");
        for (int row = 0; row < 6; row++)
        {
            ushort expected = ReadVerificationWord(rom, 0x81a312 + row * 4);
            AssertEqual(expected, FileSelectLayout.MainSelectionY(row), $"main cursor Y {row}");
            AssertEqual((int)expected, presentation.CursorPosition(true, false, row).Y,
                $"extracted main cursor Y {row}");
        }
        VerifyFileSelectGeometryBounds(FileSelectLayout.MainSelectionY, 6);
    }

    private static void VerifyFileSelectDataCursorY(ISnesAddressSpace rom, FileSelectPresentation presentation)
    {
        AssertEqual(4, FileSelectLayout.DataSelectionCount, "data selection count");
        foreach (int address in new[] { 0x819772, 0x819c03 })
        for (int row = 0; row < 4; row++)
        {
            ushort expected = ReadVerificationWord(rom, address + row * 2);
            AssertEqual(expected, FileSelectLayout.DataSelectionY(row), $"data cursor {address:X6}/{row}");
            AssertEqual((int)expected, presentation.CursorPosition(false, false, row).Y,
                $"extracted data cursor {address:X6}/{row}");
        }
        VerifyFileSelectGeometryBounds(FileSelectLayout.DataSelectionY, 4);
    }

    private static void VerifyFileSelectHelmetY(ISnesAddressSpace rom, FileSelectPresentationDocument document)
    {
        AssertEqual(3, FileSelectLayout.SaveSlotCount, "helmet slot count");
        AssertEqual(3, document.HelmetAnchors.Length, "extracted helmet anchor count");
        for (int slot = 0; slot < 3; slot++)
        {
            int instruction = 0x81a027 + slot * 6;
            AssertEqual((byte)0xa9, rom.ReadByte(instruction), "helmet LDA immediate opcode");
            AssertEqual((byte)0x8d, rom.ReadByte(instruction + 3), "helmet STA absolute opcode");
            ushort expected = ReadVerificationWord(rom, instruction + 1);
            AssertEqual(expected, FileSelectLayout.HelmetY(slot), $"helmet Y {slot}");
            AssertEqual((int)expected, document.HelmetAnchors[slot].Y, $"extracted helmet Y {slot}");
        }
        VerifyFileSelectGeometryBounds(FileSelectLayout.HelmetY, 3);
    }

    private static void VerifyFileSelectGeometryBounds(Func<int, ushort> lookup, int count)
    {
        foreach (int index in new[] { int.MinValue, -1, count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => lookup(index), $"unsupported geometry index {index}");
    }
}
