using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the loaded room header's typed fields against the compiled header bytes for every retail room: the
/// isolated door transition is selected exactly when common-room-element bit zero is set, whatever the other bits
/// hold, and the scroller distances keep their byte values.
/// </summary>
internal static class CartridgeRoomHeaderVerification
{
    /// <summary>Loads every retail header and compares its typed fields with the compiled header bytes.</summary>
    internal static void VerifyRoomIsolatedDoorTransition()
    {
        int isolated = 0;
        bool sawOtherBitsOnly = false;
        foreach (RoomHeaderDefinition definition in RoomHeaderDefinitions.All)
        {
            CartridgeRoomHeader room = CartridgeRoomHeader.LoadUsingCompiledSelection(definition);
            bool expected = (definition.CreBitset & 1) != 0;
            AssertEqual(expected, room.UsesIsolatedDoorTransition,
                $"room ${definition.Pointer:X4} settings ${definition.CreBitset:X2} isolated door transition");
            AssertEqual(definition.UpScroller, room.UpScroller, $"room ${definition.Pointer:X4} up scroller");
            AssertEqual(definition.DownScroller, room.DownScroller, $"room ${definition.Pointer:X4} down scroller");
            if (expected) isolated++;
            if (!expected && definition.CreBitset != 0) sawOtherBitsOnly = true;
        }
        AssertEqual(5, isolated, "retail rooms selecting the isolated door transition");
        AssertTrue(sawOtherBitsOnly, "a room whose only common-room-element bits are other bits keeps the ordinary transition");
    }

    /// <summary>Runs the focused loaded room header verification command.</summary>
    [VerificationCommand("--cartridge-room-header")]
    internal static int CartridgeRoomHeaderCommand()
    {
        Suite(nameof(VerifyRoomIsolatedDoorTransition), () => VerifyRoomIsolatedDoorTransition());
        return 0;
    }
}
