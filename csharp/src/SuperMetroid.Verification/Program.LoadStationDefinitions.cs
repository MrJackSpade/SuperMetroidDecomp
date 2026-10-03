using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledLoadStationDefinitions()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Load-station oracle revision");
        VerifyLoadStationDomain(rom);
        VerifyLoadStationListPointers(rom);
        VerifyLoadStationRoomPointer(rom);
        VerifyLoadStationDoorPointer(rom);
        VerifyLoadStationDoorBts(rom);
        VerifyLoadStationCameraX(rom);
        VerifyLoadStationCameraY(rom);
        VerifyLoadStationSamusYOffset(rom);
        VerifyLoadStationSamusXOffset(rom);
        Console.WriteLine("Load stations: all134 original records, seven area pointers/lengths, seven placement fields and complete byte area/station bounds pass.");
    }

    private static int OriginalLoadStationCount(SuperMetroidAddressSpace rom, int area)
    {
        // Original pointer table includes the debug-list start after Ceres; that
        // word is a boundary oracle, not an accepted eighth managed area.
        int start = ReadVerificationWord(rom, 0x80c4b5 + area * 2);
        int end = ReadVerificationWord(rom, 0x80c4b5 + (area + 1) * 2);
        AssertTrue(end > start && (end - start) % 14 == 0, "Original native load-list record alignment");
        return (end - start) / 14;
    }

    private static void VerifyLoadStationDomain(SuperMetroidAddressSpace rom)
    {
        int total = 0;
        for (int areaValue = 0; areaValue <= byte.MaxValue; areaValue++)
        {
            AreaId area = (AreaId)areaValue;
            int count = areaValue < 7 ? OriginalLoadStationCount(rom, areaValue) : 0;
            if (areaValue < 7)
            {
                AssertEqual(count, LoadStationDefinitions.Count(area), "Exact original area-list length");
                total += count;
            }
            else
                AssertThrows<ArgumentOutOfRangeException>(() => LoadStationDefinitions.Count(area), "Unknown area count rejects");
            for (int stationValue = 0; stationValue <= byte.MaxValue; stationValue++)
            {
                byte station = (byte)stationValue;
                if (stationValue < count)
                {
                    LoadStationEntry entry = LoadStationDefinitions.Get(area, station);
                    AssertEqual(area, entry.RequestedAreaIndex, "Selected load-station area identity");
                    AssertEqual(station, entry.StationIndex, "Selected load-station slot identity");
                }
                else
                    AssertThrows<ArgumentOutOfRangeException>(() => LoadStationDefinitions.Get(area, station),
                        "Every unsupported area/station pair rejects before placeholder selection");
            }
        }
        AssertEqual(134, total, "All original retail station slots");
    }

    private static void VerifyLoadStationListPointers(SuperMetroidAddressSpace rom)
    {
        for (int area = 0; area < 7; area++)
        {
            ushort expected = ReadVerificationWord(rom, 0x80c4b5 + area * 2);
            for (int station = 0; station < OriginalLoadStationCount(rom, area); station++)
                AssertEqual(expected, LoadStationDefinitions.Get((AreaId)area, (byte)station).ListPointer,
                    "Calculated prefix-sum pointer matches the original area-table word");
        }
    }

    private static void VerifyLoadStationRoomPointer(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.RoomPointer, "RoomPointer");
    private static void VerifyLoadStationDoorPointer(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.DoorPointer, "DoorPointer");
    private static void VerifyLoadStationDoorBts(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.DoorBts, "DoorBts");
    private static void VerifyLoadStationCameraX(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.CameraX, "CameraX");
    private static void VerifyLoadStationCameraY(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.CameraY, "CameraY");
    private static void VerifyLoadStationSamusYOffset(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.SamusYOffset, "SamusYOffset");
    private static void VerifyLoadStationSamusXOffset(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.SamusXOffset, "SamusXOffset");

    private static void VerifyLoadStationField(SuperMetroidAddressSpace rom,
        Func<LoadStationEntry, int> field, string name)
    {
        int total = 0;
        for (int area = 0; area < 7; area++)
        for (int station = 0; station < OriginalLoadStationCount(rom, area); station++)
        {
            LoadStationEntry original = LoadStationEntryImporter.Load(rom, (AreaId)area, (byte)station);
            LoadStationEntry actual = LoadStationDefinitions.Get((AreaId)area, (byte)station);
            AssertEqual(field(original), field(actual), $"Area {area} station {station} original {name}");
            if (name == "SamusXOffset")
                AssertEqual(unchecked((ushort)(original.CameraX + 128 + original.SamusXOffset)), actual.SamusX,
                    "Native horizontal placement preserves16-bit wrap of signed offset encoding");
            if (name == "SamusYOffset")
                AssertEqual(unchecked((ushort)(original.CameraY + original.SamusYOffset)), actual.SamusY,
                    "Native vertical placement preserves16-bit wrap");
            total++;
        }
        AssertEqual(134, total, "Complete original field domain including placeholders and Ceres exception");
    }
}
