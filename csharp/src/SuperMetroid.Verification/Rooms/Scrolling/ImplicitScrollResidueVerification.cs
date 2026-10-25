using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;
namespace SuperMetroid.Verification.Rooms.Scrolling;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Reproduces retained implicit-scroll bytes when a smaller room
/// follows a larger one and the camera reads below the new bottom row.</summary>
internal static class ImplicitScrollResidueVerification
{
    /// <summary>
    /// <c>$82:E88D</c> builds an implicit scroll table by writing only the room's width x height
    /// cells, so the rest of the 50-byte buffer keeps the previous room's bytes, and
    /// <c>$80:A893</c> reads one of them below a bottom-row camera. In the 13% movie Samus
    /// falls from $A7DE into 3x2 $A7B3; index 6 there still holds $A7DE's blue, so the camera
    /// keeps following her instead of clamping.
    /// </summary>
    internal static void VerifyImplicitScrollResidue()
    {
        var storage = new RoomScrollStorage();
        SelectedRoom previous = SelectedRoom.LoadUsingCompiledSelection(RoomHeaderDefinitions.BusinessCenter);
        SelectedRoom room = SelectedRoom.LoadUsingCompiledSelection(RoomHeaderDefinitions.CathedralEntrance);
        AssertTrue(unchecked((short)room.State.ScrollPointer) >= 0, "$A7B3 uses an implicit scroll table");

        RoomScrollGrid before = RoomScrollGrid.FromDefinition(storage, previous.State.Scroll, previous.Size);
        byte[] previousTail = before.StorageBytes.ToArray();
        RoomScrollGrid grid = RoomScrollGrid.FromDefinition(storage, room.State.Scroll, room.Size);

        AssertEqual(6, grid.LogicalCellCount, "$A7B3 is three screens by two");
        // #1336: past the 50 cells the scroll reads land in the saved explored-map planes, which read as zero.
        AssertEqual(RoomScrollDefinitions.ExploredMapOverreadByte, grid.ReadCameraStorage(RoomScrollGrid.StorageByteCount),
            "index $32 reads the explored-map byte as the cartridge's zero");
        AssertEqual(RoomScrollDefinitions.ExploredMapOverreadByte, grid.ReadCameraStorage(ushort.MaxValue),
            "the last index a 16-bit register selects reads zero too");
        AssertEqual((byte)RoomScrollState.Blue, (byte)grid.ReadCameraState(3), "the bottom row is blue");
        for (int index = grid.LogicalCellCount; index < RoomScrollGrid.StorageByteCount; index++)
            AssertEqual(previousTail[index], grid.ReadStorage(index), $"scroll byte {index} keeps the previous room's value");
        AssertEqual((byte)RoomScrollState.Blue, (byte)grid.ReadCameraState(3 + room.Size.X),
            "the cell below the bottom row is $A7DE's blue, not red");
        Console.WriteLine("  Implicit scroll residue: $A7B3 keeps $A7DE's bytes past its six cells.");
    }

    /// <summary>Runs the retained scroll-buffer reproduction from the CLI.</summary>
    [VerificationCommand("--implicit-scroll-residue")]
    internal static int ImplicitScrollResidueCommand()
    {
        Suite(nameof(VerifyImplicitScrollResidue), () => VerifyImplicitScrollResidue());
        return 0;
    }
}
