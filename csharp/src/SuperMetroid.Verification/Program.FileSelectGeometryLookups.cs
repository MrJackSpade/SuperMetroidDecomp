using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Represents file select geometry lookups verification.</summary>
internal static class FileSelectGeometryLookupsVerification
{
    /// <summary>verify file select geometry lookups.</summary>
    internal static void VerifyFileSelectGeometryLookups()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),
            "file select geometry oracle revision");
        byte[] bytes = FileSelectPresentationExtractor.Extract(rom);
        FileSelectPresentation presentation = FileSelectPresentation.Load(new MemoryStream(bytes));
        FileSelectPresentationDocument document = JsonSerializer.Deserialize<FileSelectPresentationDocument>(
            bytes, MapPresentationFormat.JsonOptions)!;
        Suite(nameof(VerifyFileSelectMainCursorY), () => VerifyFileSelectMainCursorY(rom, presentation));
        Suite(nameof(VerifyFileSelectDataCursorY), () => VerifyFileSelectDataCursorY(rom, presentation));
        Suite(nameof(VerifyFileSelectHelmetY), () => VerifyFileSelectHelmetY(rom, document));
        Console.WriteLine("File select geometry: six main rows, both four-row data views, three helmet anchors, extracted output and bounds pass.");
    }

    /// <summary>verify file select main cursor y.</summary>
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
        Suite(nameof(VerifyFileSelectGeometryBounds), () => VerifyFileSelectGeometryBounds(FileSelectLayout.MainSelectionY, 6));
    }

    /// <summary>verify file select data cursor y.</summary>
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
        Suite(nameof(VerifyFileSelectGeometryBounds), () => VerifyFileSelectGeometryBounds(FileSelectLayout.DataSelectionY, 4));
    }

    /// <summary>verify file select helmet y.</summary>
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

    /// <summary>file select helmet yrom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select helmet Y")]
    private static byte FileSelectHelmetYRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>verify file select geometry bounds.</summary>
    private static void VerifyFileSelectGeometryBounds(Func<int, ushort> lookup, int count)
    {
        foreach (int index in new[] { int.MinValue, -1, count, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => lookup(index), $"unsupported geometry index {index}");
    }

    /// <summary>lookup file select geometry command.</summary>
    [VerificationCommand("--lookup-file-select-geometry")]
    internal static int LookupFileSelectGeometryCommand()
    {
        Suite(nameof(VerifyFileSelectGeometryLookups), () => VerifyFileSelectGeometryLookups());
        return 0;
    }
}
