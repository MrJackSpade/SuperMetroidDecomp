using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Checks the loaded room's typed header fields against the compiled header for every retail room: the isolated door
/// transition is selected exactly for the rooms whose header asks for it, and the scroller distances keep their values.
/// </summary>
internal static class CartridgeRoomHeaderVerification
{
    /// <summary>Loads every retail header and compares its typed fields with the compiled header bytes.</summary>
    internal static void VerifyRoomIsolatedDoorTransition()
    {
        int isolated = 0;
        foreach (RoomHeaderDefinition definition in RoomHeaderDefinitions.All)
        {
            SelectedRoom room = SelectedRoom.LoadUsingCompiledSelection(definition);
            AssertEqual(definition.UsesIsolatedDoorTransition, room.UsesIsolatedDoorTransition,
                $"room ${definition.Pointer:X4} isolated door transition");
            AssertEqual(definition.UpScroller, room.UpScroller, $"room ${definition.Pointer:X4} up scroller");
            AssertEqual(definition.DownScroller, room.DownScroller, $"room ${definition.Pointer:X4} down scroller");
            if (room.UsesIsolatedDoorTransition) isolated++;
        }
        AssertEqual(5, isolated, "retail rooms selecting the isolated door transition");
    }

    /// <summary>Runs the focused loaded room header verification command.</summary>
    [VerificationCommand("--cartridge-room-header")]
    internal static int CartridgeRoomHeaderCommand()
    {
        Suite(nameof(VerifyRoomIsolatedDoorTransition), () => VerifyRoomIsolatedDoorTransition());
        return 0;
    }
}
