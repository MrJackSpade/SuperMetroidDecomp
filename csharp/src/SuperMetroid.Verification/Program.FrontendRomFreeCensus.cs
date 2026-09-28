using System.Globalization;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// Opt-in bounded retail-room census. Restore each room from the same live
    /// gameplay state so native VRAM queues, PLM slots, and room events from an
    /// earlier diagnostic room cannot affect the next result.
    /// </summary>
    private static void VerifyFrontendRomFreeRoomCensus(SuperMetroidGame native,
        SuperMetroidGame installed, Action<SuperMetroidGame, bool> bindInstalled)
    {
        using var nativeSnapshot = new MemoryStream();
        using var installedSnapshot = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(nativeSnapshot, native);
        DebuggerObjectGraphSerializer.Serialize(installedSnapshot, installed);
        string? snapshotDirectory = Environment.GetEnvironmentVariable(
            "SM_ROM_FREE_CENSUS_SNAPSHOT_DIR");
        if (!string.IsNullOrWhiteSpace(snapshotDirectory))
        {
            // These private debugger graphs contain the source cartridge bytes.
            // Keep them only in an explicitly requested, ignored local directory.
            string resolvedDirectory = Path.GetFullPath(snapshotDirectory);
            string tempRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
            AssertTrue(resolvedDirectory.StartsWith(tempRoot + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase),
                "ROM-free census snapshots stay inside ignored csharp/test-temp");
            Directory.CreateDirectory(resolvedDirectory);
            File.WriteAllBytes(Path.Combine(resolvedDirectory, "native.graph"),
                nativeSnapshot.ToArray());
            File.WriteAllBytes(Path.Combine(resolvedDirectory, "installed.graph"),
                installedSnapshot.ToArray());
        }
        VerifyFrontendRomFreeRoomCensus(nativeSnapshot, installedSnapshot, bindInstalled);
    }

    /// <summary>Repeat a room census from the private pre-room debugger graphs.</summary>
    private static void VerifyFrontendRomFreeRoomCensusFromSnapshots(
        string sourceRom, string snapshotDirectory)
    {
        string directory = Path.GetFullPath(snapshotDirectory);
        string tempRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        AssertTrue(directory.StartsWith(tempRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase),
            "ROM-free census snapshots stay inside ignored csharp/test-temp");
        string installationRoot = Path.GetFullPath(Path.Combine(tempRoot,
            "rom-free-census-install-" + Guid.NewGuid().ToString("N")));
        AssertTrue(installationRoot.StartsWith(tempRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase),
            "temporary census installation stays inside csharp/test-temp");
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, installationRoot);
            using FileStream nativeSnapshot = File.OpenRead(Path.Combine(directory, "native.graph"));
            using FileStream installedSnapshot = File.OpenRead(Path.Combine(directory, "installed.graph"));
            VerifyFrontendRomFreeRoomCensus(nativeSnapshot, installedSnapshot,
                PrepareRomFreeBindings(installation));
        }
        finally
        {
            if (Directory.Exists(installationRoot))
                Directory.Delete(installationRoot, recursive: true);
        }
    }

    private static void VerifyFrontendRomFreeRoomCensus(Stream nativeSnapshot,
        Stream installedSnapshot, Action<SuperMetroidGame, bool> bindInstalled)
    {
        string? selectedRoomText = Environment.GetEnvironmentVariable("SM_ROM_FREE_CENSUS_ROOM");
        ushort? selectedRoom = null;
        if (!string.IsNullOrWhiteSpace(selectedRoomText))
        {
            AssertTrue(ushort.TryParse(selectedRoomText.TrimStart('$'),
                    NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort parsedRoom),
                "ROM-free census room selector is a hexadecimal room pointer");
            selectedRoom = parsedRoom;
        }
        int frameCount = 1;
        string? frameCountText = Environment.GetEnvironmentVariable("SM_ROM_FREE_CENSUS_FRAMES");
        if (!string.IsNullOrWhiteSpace(frameCountText))
        {
            AssertTrue(int.TryParse(frameCountText, NumberStyles.None,
                    CultureInfo.InvariantCulture, out frameCount) &&
                frameCount is >= 1 and <= 3600,
                "ROM-free census frame count must be 1..3600");
            AssertTrue(selectedRoom is not null || frameCount <= 90,
                "whole-cartridge ROM-free room census is limited to 90 neutral frames per room");
        }
        var failures = new List<string>();
        int checkedRooms = 0;
        foreach (RoomHeaderDefinition room in RoomHeaderDefinitions.All)
        {
            if (selectedRoom is not null && room.Pointer != selectedRoom)
                continue;
            checkedRooms++;
            nativeSnapshot.Position = 0;
            installedSnapshot.Position = 0;
            try
            {
                SuperMetroidGame nativeRoom =
                    DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(nativeSnapshot);
                SuperMetroidGame installedRoom =
                    DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(installedSnapshot);
                bindInstalled(installedRoom, false);
                VerifyFrontendRomFreeRoom(nativeRoom, installedRoom, room.Pointer,
                    $"room ${room.Pointer:X4}", frameCount);
            }
            catch (Exception error)
            {
                failures.Add($"Room ${room.Pointer:X4}: " +
                    (selectedRoom is null ? $"{error.GetType().Name}: {error.Message}" : error.ToString()));
            }
        }
        AssertTrue(checkedRooms > 0, "ROM-free census selected at least one retail room");
        AssertTrue(failures.Count == 0,
            $"ROM-free {frameCount}-frame room census failed in {failures.Count} of " +
            $"{checkedRooms} rooms:\n" + string.Join("\n", failures));
        Console.WriteLine($"ROM-free room census: {checkedRooms} isolated rooms, " +
            $"{frameCount} frame(s) each, match native pixels without cartridge reads.");
    }
}
