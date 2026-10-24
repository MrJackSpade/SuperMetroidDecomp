using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Map.Artwork;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks file-select slot field destinations and label sources against bank $81 instructions.</summary>
internal static class FileSelectSlotDestinationsVerification
{
    /// <summary>Compares 24 native destinations with typed layouts and extracted text or numeric anchors.</summary>
    internal static void VerifyFileSelectSlotDestinations()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Slot field oracle revision");
        Suite(nameof(VerifyFileSelectSlotLabelSources), () => VerifyFileSelectSlotLabelSources(rom));
        byte[] json = FileSelectPresentationExtractor.Extract(rom);
        _ = FileSelectPresentation.Load(new MemoryStream(json));
        FileSelectPresentationDocument document = RequiredJson.Deserialize<FileSelectPresentationDocument>(
            json, ArtworkJson.Options);
        // Each independently indexed field uses its own original LDX operands.
        Suite(nameof(VerifyField), () => VerifyField(false, FileSelectSlotField.Label, [0x819f16, 0x819f49, 0x819f7f]));
        Suite(nameof(VerifyField), () => VerifyField(false, FileSelectSlotField.Energy, [0x819f25, 0x819f5b, 0x819f91]));
        Suite(nameof(VerifyField), () => VerifyField(false, FileSelectSlotField.TimeValue, [0x819f31, 0x819f67, 0x819f9d]));
        Suite(nameof(VerifyField), () => VerifyField(false, FileSelectSlotField.TimeLabel, [0x819f40, 0x819f76, 0x819fac]));
        Suite(nameof(VerifyField), () => VerifyField(true, FileSelectSlotField.Label, [0x819639, 0x819669, 0x819699]));
        Suite(nameof(VerifyField), () => VerifyField(true, FileSelectSlotField.Energy, [0x81960f, 0x81963f, 0x81966f]));
        Suite(nameof(VerifyField), () => VerifyField(true, FileSelectSlotField.TimeValue, [0x81961e, 0x81964e, 0x81967e]));
        Suite(nameof(VerifyField), () => VerifyField(true, FileSelectSlotField.TimeLabel, [0x819630, 0x819660, 0x819690]));

        void VerifyField(bool dataPage, FileSelectSlotField field, int[] instructions)
        {
            Func<int, FileSelectSlotField, int> destination = dataPage
                ? FileSelectNativeLayout.DataSlotDestination : FileSelectNativeLayout.MainSlotDestination;
            for (int slot = 0; slot < 3; slot++)
            {
                int instruction = instructions[slot];
                AssertEqual((byte)0xa2, FileSelectSlotDestinationsRomByte(rom, instruction), "native slot LDX immediate");
                int expected = ReadVerificationWord(rom, instruction + 1);
                AssertEqual(expected, destination(slot, field), $"slot destination {dataPage}/{field}/{slot}");
                FileSelectSlotFieldDocument exported = (dataPage ? document.DataSlots : document.MainSlots)[slot];
                if (field is FileSelectSlotField.Energy or FileSelectSlotField.TimeValue)
                {
                    Position16<TilemapCells> point = field == FileSelectSlotField.Energy ? exported.EnergyAnchor : exported.TimeValueAnchor;
                    AssertEqual(expected, (point.Y * 32 + point.X) * 2, "extracted slot field anchor");
                }
                else
                {
                    AssertEqual((byte)0xa0, FileSelectSlotDestinationsRomByte(rom, instruction - 3), "native slot label LDY immediate");
                    int source = 0x810000 | ReadVerificationWord(rom, instruction - 2);
                    ushort firstTile = ReadVerificationWord(rom, source);
                    string[] pages = dataPage
                        ? [FileSelectPresentationDefinitions.PageName(FileSelectPage.CopySource), FileSelectPresentationDefinitions.PageName(FileSelectPage.ClearSelection)]
                        : [FileSelectPresentationDefinitions.PageName(FileSelectPage.MainWithData), FileSelectPresentationDefinitions.PageName(FileSelectPage.MainEmpty)];
                    foreach (string page in pages)
                    {
                        MapPresentationCell cell = document.Pages[page][expected / 2];
                        AssertEqual(firstTile & 0x3ff, cell.TileRow * 32 + cell.TileColumn, "extracted slot text starts at original destination");
                    }
                }
            }
            foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => destination(invalid, field), "slot index bounds");
            foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => destination(0, (FileSelectSlotField)invalid), "slot field bounds");
        }
    }

    /// <summary>Reads native slot destination instructions for the layout comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select slot destinations")]
    private static byte FileSelectSlotDestinationsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks both slot-label load sites use the compiled source and terminal word.</summary>
    private static void VerifyFileSelectSlotLabelSources(SuperMetroid.Core.Hardware.ISnesAddressSpace rom)
    {
        int[][] loadInstructions = [[0x819f13, 0x819f46, 0x819f7c], [0x819636, 0x819666, 0x819696]];
        foreach (int[] view in loadInstructions)
        for (int slot = 0; slot < 3; slot++)
        {
            AssertEqual((byte)0xa0, FileSelectSlotLabelSourcesRomByte(rom, view[slot]), "native slot label LDY");
            ushort source = ReadVerificationWord(rom, view[slot] + 1);
            AssertEqual(source, FileSelectTilemapDefinitions.SlotLabel(slot), "original slot label source");
            AssertEqual((ushort)0xffff, ReadVerificationWord(rom, 0x810000 | (source + 30)),
                "slot label record includes terminal word");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectTilemapDefinitions.SlotLabel(invalid), "slot label source bounds");
    }

    /// <summary>Reads native slot-label source instructions for the layout comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select slot label sources")]
    private static byte FileSelectSlotLabelSourcesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the slot field destination and source verification command.</summary>
    [VerificationCommand("--lookup-file-select-slot-fields")]
    internal static int LookupFileSelectSlotFieldsCommand()
    {
        Suite(nameof(VerifyFileSelectSlotDestinations), () => VerifyFileSelectSlotDestinations());
        Console.WriteLine("File-select slot fields: all 24 original destinations, both slot-label source views, extracted anchors/text and bounds pass.");
        return 0;
    }
}
