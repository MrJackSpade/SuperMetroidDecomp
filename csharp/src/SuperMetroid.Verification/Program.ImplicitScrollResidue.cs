using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using static AssertionsVerification;

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
        var bus = new TestAddressSpace();
        CartridgeRoomHeader previous = CartridgeRoomHeader.LoadUsingCompiledSelection(0xa7de);
        CartridgeRoomHeader room = CartridgeRoomHeader.LoadUsingCompiledSelection(0xa7b3);
        AssertTrue(unchecked((short)room.State.ScrollPointer) >= 0, "$A7B3 uses an implicit scroll table");

        RoomScrollGrid before = RoomScrollDefinitions.CreateGrid(bus, previous.State.ScrollPointer, previous.Size);
        RoomScrollGrid grid = RoomScrollDefinitions.CreateGrid(bus, room.State.ScrollPointer, room.Size);

        AssertEqual(6, grid.LogicalCellCount, "$A7B3 is three screens by two");
        AssertEqual((byte)RoomScrollState.Blue, (byte)grid.ReadNativeState(3), "the bottom row is blue");
        for (int index = grid.LogicalCellCount; index < RoomScrollGrid.StorageByteCount; index++)
            AssertEqual(before.ReadStorage(index), grid.ReadStorage(index), $"scroll byte {index} keeps the previous room's value");
        AssertEqual((byte)RoomScrollState.Blue, (byte)grid.ReadNativeState(3 + room.Size.X),
            "the cell below the bottom row is $A7DE's blue, not red");
        Console.WriteLine("  Implicit scroll residue: $A7B3 keeps $A7DE's bytes past its six cells.");
    }

    [VerificationCommand("--implicit-scroll-residue")]
    internal static int ImplicitScrollResidueCommand()
    {
        Suite(nameof(VerifyImplicitScrollResidue), () => VerifyImplicitScrollResidue());
        return 0;
    }
}
