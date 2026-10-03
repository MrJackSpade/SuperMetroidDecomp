using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyFileSelectSlotDestinations()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Slot field oracle revision");
        byte[] json = FileSelectPresentationExtractor.Extract(rom);
        _ = FileSelectPresentation.Load(new MemoryStream(json));
        FileSelectPresentationDocument document = JsonSerializer.Deserialize<FileSelectPresentationDocument>(
            json, MapPresentationFormat.JsonOptions)!;
        // Each independently indexed field uses its own original LDX operands.
        VerifyField(false, FileSelectSlotField.Label, [0x819f16, 0x819f49, 0x819f7f]);
        VerifyField(false, FileSelectSlotField.Energy, [0x819f25, 0x819f5b, 0x819f91]);
        VerifyField(false, FileSelectSlotField.TimeValue, [0x819f31, 0x819f67, 0x819f9d]);
        VerifyField(false, FileSelectSlotField.TimeLabel, [0x819f40, 0x819f76, 0x819fac]);
        VerifyField(true, FileSelectSlotField.Label, [0x819639, 0x819669, 0x819699]);
        VerifyField(true, FileSelectSlotField.Energy, [0x81960f, 0x81963f, 0x81966f]);
        VerifyField(true, FileSelectSlotField.TimeValue, [0x81961e, 0x81964e, 0x81967e]);
        VerifyField(true, FileSelectSlotField.TimeLabel, [0x819630, 0x819660, 0x819690]);

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
}
