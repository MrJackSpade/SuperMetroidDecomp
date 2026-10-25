using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

/// <summary>Checks the compiled seven-area load-station table against all retail records.</summary>
internal static class LoadStationDefinitionsVerification
{
    /// <summary>Compares native list lengths, valid area/station pairs, and every station placement field.</summary>
    internal static void VerifyCompiledLoadStationDefinitions()
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

    /// <summary>Uses consecutive native pointer-table entries to derive one area's fourteen-byte record count.</summary>
    private static int OriginalLoadStationCount(SuperMetroidAddressSpace rom, int area)
    {
        // Original pointer table includes the debug-list start after Ceres; that
        // word is a boundary oracle, not an accepted eighth managed area.
        int start = ReadVerificationWord(rom, 0x80c4b5 + area * 2);
        int end = ReadVerificationWord(rom, 0x80c4b5 + (area + 1) * 2);
        AssertTrue(end > start && (end - start) % 14 == 0, "Original native load-list record alignment");
        return (end - start) / 14;
    }

    /// <summary>Checks all byte-sized area/station pairs against the seven supported native lists.</summary>
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

    /// <summary>Checks every destination room header pointer against its native station record.</summary>
    private static void VerifyLoadStationRoomPointer(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.Room is { } room ? RoomHeaderDefinitions.PointerOf(room) : 0, "RoomPointer");
    /// <summary>Checks every destination door pointer against its native station record.</summary>
    private static void VerifyLoadStationDoorPointer(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.Door is { } door ? DoorDefinitions.PointerOf(door) : 0, "DoorPointer");
    /// <summary>Checks camera X placement for all 134 retail station slots.</summary>
    private static void VerifyLoadStationCameraX(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.CameraPosition.X, "CameraX");
    /// <summary>Checks camera Y placement for all 134 retail station slots.</summary>
    private static void VerifyLoadStationCameraY(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.CameraPosition.Y, "CameraY");
    /// <summary>Checks vertical Samus offset and the resulting wrapped room position.</summary>
    private static void VerifyLoadStationSamusYOffset(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.SamusOffset.Y, "SamusYOffset");
    /// <summary>Checks horizontal Samus offset and its signed sixteen-bit wrap during placement.</summary>
    private static void VerifyLoadStationSamusXOffset(SuperMetroidAddressSpace rom) => VerifyLoadStationField(rom, entry => entry.SamusOffset.X, "SamusXOffset");

    /// <summary>Compares one selected field across every native area list, including placeholder stations and Ceres.</summary>
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
                AssertEqual(unchecked((ushort)(original.CameraPosition.X + 128 + original.SamusOffset.X)), actual.SamusPosition.X,
                    "Native horizontal placement preserves16-bit wrap of signed offset encoding");
            if (name == "SamusYOffset")
                AssertEqual(unchecked((ushort)(original.CameraPosition.Y + original.SamusOffset.Y)), actual.SamusPosition.Y,
                    "Native vertical placement preserves16-bit wrap");
            total++;
        }
        AssertEqual(134, total, "Complete original field domain including placeholders and Ceres exception");
    }

    /// <summary>Runs the compiled load-station comparison against the retail cartridge.</summary>
    [VerificationCommand("--load-station-definitions")]
    internal static int LoadStationDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledLoadStationDefinitions), () => VerifyCompiledLoadStationDefinitions());
        return 0;
    }
}
