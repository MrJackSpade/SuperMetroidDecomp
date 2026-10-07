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
        Suite(nameof(VerifyFrontendRomFreeRoomCensus), () => VerifyFrontendRomFreeRoomCensus(nativeSnapshot, installedSnapshot, bindInstalled));
    }

    /// <summary>Repeat a room census from the private pre-room debugger graphs.</summary>
    private static void VerifyFrontendRomFreeRoomCensus(Stream nativeSnapshot,
        Stream installedSnapshot, Action<SuperMetroidGame, bool> bindInstalled)
    {
        // Every retail room, one neutral frame each.
        const int frameCount = 1;
        var failures = new List<string>();
        int checkedRooms = 0;
        foreach (RoomHeaderDefinition room in RoomHeaderDefinitions.All)
        {
            checkedRooms++;
            nativeSnapshot.Position = 0;
            installedSnapshot.Position = 0;
            try
            {
                SuperMetroidGame nativeRoom =
                    DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(nativeSnapshot);
                SuperMetroidGame installedRoom =
                    DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(installedSnapshot);
                // Restored snapshots carry no asset bindings; both games load rooms from the installation.
                bindInstalled(nativeRoom, false);
                bindInstalled(installedRoom, false);
                VerifyFrontendRomFreeRoom(nativeRoom, installedRoom, room.Pointer,
                    $"room ${room.Pointer:X4}", frameCount);
            }
            catch (Exception error)
            {
                failures.Add($"Room ${room.Pointer:X4}: " +
                    $"{error.GetType().Name}: {error.Message}");
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
