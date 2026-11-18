using SuperMetroid.TestSupport.RoomHeaderImport;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
        var storage = RoomScrollGrid.PowerOnCells();
        SelectedRoom previous = SelectedRoom.LoadUsingCompiledSelection(RoomDefinitions.BusinessCenter, RoomStateSelectionContexts.NoProgress);
        SelectedRoom room = SelectedRoom.LoadUsingCompiledSelection(RoomDefinitions.CathedralEntrance, RoomStateSelectionContexts.NoProgress);
        AssertTrue(unchecked((short)room.State.ScrollPointer) >= 0, "$A7B3 uses an implicit scroll table");

        ScrollCellsPastRoomScreens<RoomScrollState> pastScreens = InstalledFixtures.RoomAssets.ScrollCellsPastRoomScreens;
        RoomScrollGrid before = RoomScrollGrid.FromDefinition(storage, previous.State.Scroll, previous.Size, pastScreens);
        RoomScrollState[] previousTail = before.KeptStates;
        RoomScrollGrid grid = RoomScrollGrid.FromDefinition(storage, room.State.Scroll, room.Size, pastScreens);

        AssertEqual(6, grid.ScreenCount, "$A7B3 is three screens by two");
        // #1336, #1500: past the 50 cells the scroll reads land in the stored explored maps, unexplored at power-on, and
        // past every stored map they are rejected.
        AssertEqual(RoomScrollState.Closed, grid.ReadCameraState(grid.Size.CellAtRowMajorIndex(RoomScrollGrid.KeptCellCount)),
            "index $32 reads Crateria's unexplored stored map as red");
        AssertThrows<ArgumentOutOfRangeException>(() => grid.ReadCameraState(grid.Size.CellAtRowMajorIndex(ushort.MaxValue)),
            "the last index a 16-bit register selects lies past every stored map");
        AssertEqual(RoomScrollState.OpenAlignedToTop, grid.ReadScreen(new(0, 1)), "the bottom row is blue");
        RoomScrollState[] kept = grid.KeptStates;
        for (int index = grid.ScreenCount; index < RoomScrollGrid.KeptCellCount; index++)
            AssertEqual(previousTail[index], kept[index], $"scroll cell {index} keeps the previous room's value");
        AssertEqual(RoomScrollState.OpenAlignedToTop, grid.ReadCameraState(new(0, room.Size.Y)),
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
