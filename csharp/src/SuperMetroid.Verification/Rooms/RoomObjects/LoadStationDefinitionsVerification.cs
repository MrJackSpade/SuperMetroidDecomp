using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

/// <summary>Checks the named load stations against the cartridge's per-area load-station lists.</summary>
internal static class LoadStationDefinitionsVerification
{
    /// <summary>The first number of the cartridge's debug load points, which no save, map or gameplay load names.</summary>
    private const int FirstDebugNumber = 16;

    /// <summary>The cartridge number of the post-Ceres arrival, the one load point past the save stations and elevators gameplay loads.</summary>
    private const int ZebesArrivalNumber = 18;

    /// <summary>Compares every named station's placement with its native record and the named set with the native lists.</summary>
    internal static void VerifyCompiledLoadStationDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Load-station oracle revision");
        Suite(nameof(VerifyLoadStationDomain), () => VerifyLoadStationDomain(rom));
        Suite(nameof(VerifyLoadStationPlacements), () => VerifyLoadStationPlacements(rom));
        Console.WriteLine("Load stations: the 36 named stations match their native records; every other native record is unused, a debug load point or a Ceres repeat.");
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

    /// <summary>
    /// Checks which native records are named: each named station is a record of its area's list, and a record that names a
    /// room is named unless it is a debug load point or a repeat of the Ceres start; lookups outside the named set find none.
    /// </summary>
    private static void VerifyLoadStationDomain(SuperMetroidAddressSpace rom)
    {
        int total = 0, named = 0;
        for (int areaNumber = 0; areaNumber < AreaNumberCodec.Count; areaNumber++)
        {
            AreaId area = AreaNumberCodec.At(areaNumber);
            int count = OriginalLoadStationCount(rom, areaNumber);
            total += count;
            for (int station = 0; station <= byte.MaxValue; station++)
            {
                LoadStation? found = LoadStations.Find(area, station);
                bool expected = station < count && LoadStationEntryImporter.Load(rom, area, (byte)station).Room is not null &&
                    (area == AreaId.Ceres ? station == 0 : station < FirstDebugNumber || (area == AreaId.Crateria && station == ZebesArrivalNumber));
                AssertEqual(expected, found is not null, $"{area} station {station} is named exactly when gameplay can load it");
                if (found is null)
                {
                    AssertThrows<InvalidDataException>(() => LoadStations.Get(area, station, "test"), $"{area} station {station} names no station");
                    continue;
                }
                AssertEqual(area, found.Area, $"{found} lies in its area's list");
                AssertEqual(station, found.Number, $"{found} keeps its number");
                AssertTrue(ReferenceEquals(found, LoadStations.Get(area, station, "test")), $"{found} is found by its area and number");
                named++;
            }
        }
        AssertEqual(134, total, "All original retail station slots");
        AssertEqual(36, named, "Named load stations");
        AssertEqual(36, LoadStationDefinitions.All.Count, "Every named station is listed once");
    }

    /// <summary>Checks each named station's room, door, camera and Samus placement against its native record.</summary>
    private static void VerifyLoadStationPlacements(SuperMetroidAddressSpace rom)
    {
        foreach (LoadStation station in LoadStationDefinitions.All)
        {
            NativeLoadStation original = LoadStationEntryImporter.Load(rom, station.Area, (byte)station.Number);
            AssertTrue(ReferenceEquals(original.Room, station.Room), $"{station} room");
            AssertTrue(ReferenceEquals(original.Door, station.Door), $"{station} door");
            AssertEqual(original.CameraPosition, station.CameraPosition, $"{station} camera");
            AssertEqual(original.SamusOffset, station.SamusOffset, $"{station} Samus offset");
            AssertEqual(unchecked((ushort)(original.CameraPosition.X + 128 + original.SamusOffset.X)), station.SamusPosition.X,
                $"{station} horizontal placement preserves the 16-bit wrap of its signed offset");
            AssertEqual(unchecked((ushort)(original.CameraPosition.Y + original.SamusOffset.Y)), station.SamusPosition.Y,
                $"{station} vertical placement preserves the 16-bit wrap");
        }
    }

    /// <summary>Runs the compiled load-station comparison against the retail cartridge.</summary>
    [VerificationCommand("--load-station-definitions")]
    internal static int LoadStationDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledLoadStationDefinitions), () => VerifyCompiledLoadStationDefinitions());
        return 0;
    }
}
