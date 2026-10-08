using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyFileSelectSlotDestinations()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Slot field oracle revision");
        Suite(nameof(VerifyFileSelectSlotLabelSources), () => VerifyFileSelectSlotLabelSources(rom));
        byte[] json = FileSelectPresentationExtractor.Extract(rom);
        _ = FileSelectPresentation.Load(new MemoryStream(json));
        FileSelectPresentationDocument document = JsonSerializer.Deserialize<FileSelectPresentationDocument>(
            json, MapPresentationFormat.JsonOptions)!;
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
                ? FileSelectLayout.DataSlotDestination : FileSelectLayout.MainSlotDestination;
            for (int slot = 0; slot < 3; slot++)
            {
                int instruction = instructions[slot];
                AssertEqual((byte)0xa2, rom.ReadByte(instruction), "native slot LDX immediate");
                int expected = ReadVerificationWord(rom, instruction + 1);
                AssertEqual(expected, destination(slot, field), $"slot destination {dataPage}/{field}/{slot}");
                FileSelectSlotFieldDocument exported = (dataPage ? document.DataSlots : document.MainSlots)[slot];
                if (field is FileSelectSlotField.Energy or FileSelectSlotField.TimeValue)
                {
                    MapLabelPoint point = field == FileSelectSlotField.Energy ? exported.EnergyAnchor : exported.TimeValueAnchor;
                    AssertEqual(expected, (point.Y * 32 + point.X) * 2, "extracted slot field anchor");
                }
                else
                {
                    AssertEqual((byte)0xa0, rom.ReadByte(instruction - 3), "native slot label LDY immediate");
                    int source = 0x810000 | ReadVerificationWord(rom, instruction - 2);
                    ushort firstTile = ReadVerificationWord(rom, source);
                    string[] pages = dataPage
                        ? [FileSelectPresentationDefinitions.CopySourcePage, FileSelectPresentationDefinitions.ClearSelectionPage]
                        : [FileSelectPresentationDefinitions.MainWithDataPage, FileSelectPresentationDefinitions.MainEmptyPage];
                    foreach (string page in pages)
                    {
                        MapPresentationCell cell = document.Pages[page][expected / 2];
                        AssertEqual(firstTile & 0x3ff, cell.TileRow * 32 + cell.TileColumn, "extracted slot text starts at original destination");
                    }
                }
            }
            foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => destination(invalid, field), "slot index bounds");
            foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => destination(0, (FileSelectSlotField)invalid), "slot field bounds");
        }
    }

    private static void VerifyFileSelectSlotLabelSources(SuperMetroid.Core.Hardware.ISnesAddressSpace rom)
    {
        int[][] loadInstructions = [[0x819f13, 0x819f46, 0x819f7c], [0x819636, 0x819666, 0x819696]];
        foreach (int[] view in loadInstructions)
        for (int slot = 0; slot < 3; slot++)
        {
            AssertEqual((byte)0xa0, rom.ReadByte(view[slot]), "native slot label LDY");
            ushort source = ReadVerificationWord(rom, view[slot] + 1);
            AssertEqual(source, FileSelectTilemaps.SlotLabel(slot), "original slot label source");
            AssertEqual((ushort)0xffff, ReadVerificationWord(rom, 0x810000 | (source + 30)),
                "slot label record includes terminal word");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 65536, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FileSelectTilemaps.SlotLabel(invalid), "slot label source bounds");
    }
}
