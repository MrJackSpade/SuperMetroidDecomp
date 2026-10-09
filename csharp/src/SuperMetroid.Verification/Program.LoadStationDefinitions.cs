using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Validates the compiled load-station catalog against the supported retail ROM's complete area and station domain.</summary>
    private static void VerifyCompiledLoadStationDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Load-station oracle revision");
        Suite(nameof(VerifyLoadStationDomain), () => VerifyLoadStationDomain(rom));
        Suite(nameof(VerifyLoadStationRoomPointer), () => VerifyLoadStationRoomPointer(rom));
        Suite(nameof(VerifyLoadStationDoorPointer), () => VerifyLoadStationDoorPointer(rom));
        Suite(nameof(VerifyLoadStationCameraX), () => VerifyLoadStationCameraX(rom));
        Suite(nameof(VerifyLoadStationCameraY), () => VerifyLoadStationCameraY(rom));
        Suite(nameof(VerifyLoadStationSamusYOffset), () => VerifyLoadStationSamusYOffset(rom));
        Suite(nameof(VerifyLoadStationSamusXOffset), () => VerifyLoadStationSamusXOffset(rom));
        Console.WriteLine("Load stations: all134 original records, seven area lengths, six placement fields and complete byte area/station bounds pass.");
    }

    /// <summary>Reads the original area's pointer-table bounds and derives its record count, treating the post-Ceres debug start as a boundary.</summary>
    /// <param name="rom">Retail address space containing the load-station pointer table.</param>
    /// <param name="area">Zero-based native area index whose list length is being measured.</param>
    /// <returns>The number of fourteen-byte records between the area's start and next boundary.</returns>
    private static int OriginalLoadStationCount(SuperMetroidAddressSpace rom, int area)
    {
        // Original pointer table includes the debug-list start after Ceres; that
        // word is a boundary oracle, not an accepted eighth managed area.
        int start = ReadVerificationWord(rom, 0x80c4b5 + area * 2);
        int end = ReadVerificationWord(rom, 0x80c4b5 + (area + 1) * 2);
        AssertTrue(end > start && (end - start) % 14 == 0, "Original native load-list record alignment");
        return (end - start) / 14;
    }

    /// <summary>Checks every byte-valued area and station selector, accepting exactly the original seven areas and their listed stations.</summary>
    /// <param name="rom">Retail source used to obtain each original area's list length.</param>
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
                    _ = LoadStationDefinitions.Get(area, station);
                else
                    AssertThrows<ArgumentOutOfRangeException>(() => LoadStationDefinitions.Get(area, station),
                        "Every unsupported area/station pair rejects before placeholder selection");
            }
        }
        AssertEqual(134, total, "All original retail station slots");
    }

    /// <summary>Compares compiled room-pointer values with every original station record.</summary>
    /// <param name="rom">Retail address space containing the original station records.</param>
    private static void VerifyLoadStationRoomPointer(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.RoomPointer, "RoomPointer");
    /// <summary>Compares compiled door-pointer values with every original station record.</summary>
    /// <param name="rom">Retail address space containing the original station records.</param>
    private static void VerifyLoadStationDoorPointer(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.DoorPointer, "DoorPointer");
    /// <summary>Compares compiled camera-X values with every original station record.</summary>
    /// <param name="rom">Retail address space containing the original station records.</param>
    private static void VerifyLoadStationCameraX(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.CameraX, "CameraX");
    /// <summary>Compares compiled camera-Y values with every original station record.</summary>
    /// <param name="rom">Retail address space containing the original station records.</param>
    private static void VerifyLoadStationCameraY(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.CameraY, "CameraY");
    /// <summary>Compares compiled Samus vertical placement offsets with every original station record.</summary>
    /// <param name="rom">Retail address space containing the original station records.</param>
    private static void VerifyLoadStationSamusYOffset(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.SamusYOffset, "SamusYOffset");
    /// <summary>Compares compiled Samus horizontal placement offsets with every original station record.</summary>
    /// <param name="rom">Retail address space containing the original station records.</param>
    private static void VerifyLoadStationSamusXOffset(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.SamusXOffset, "SamusXOffset");

    /// <summary>Verifies one selected field across all original records and checks wrapped Samus placement when that field is an offset.</summary>
    /// <param name="rom">Retail address space used to import the original station entries.</param>
    /// <param name="field">Projection selecting the value to compare from each entry.</param>
    /// <param name="name">Field label included in assertion messages.</param>
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
