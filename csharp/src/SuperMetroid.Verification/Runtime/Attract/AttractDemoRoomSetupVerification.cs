using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>Checks the title demonstrations' room setups against the original's routines.</summary>
internal static class AttractDemoRoomSetupVerification
{
    /// <summary>
    /// The Charge Beam demo's setup (#1741) stores one scroll state to one of the room's cells: <c>SEP #$20; LDA #state;
    /// STA $7E:cell; REP #$20; RTS</c>. The screen the port names, by its column and row in Big Pink, is that cell at Big
    /// Pink's width, inside the room, and the state it gives the screen is the stored one.
    /// </summary>
    internal static void VerifyChargeBeamClosedScreen()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int routine = 0x820000 | AttractDemoNativeWords.RoomSetups.Word(AttractDemoRoomSetup.ChargeBeamScroll);
        byte[] shape = [0xe2, 0x20, 0xa9, 0x00, 0x8f, 0x00, 0x00, 0x7e, 0xc2, 0x20, 0x60];
        foreach (int offset in new[] { 0, 1, 2, 4, 7, 8, 9, 10 })
            AssertEqual(shape[offset], rom.ReadCartridgeByte(routine + offset), $"the Charge Beam setup's instruction byte {offset}");
        RoomScrollState stored = RoomScrollStates.FromStorageByte(rom.ReadCartridgeByte(routine + 3));
        int cell = rom.ReadCartridgeByte(routine + 5) | rom.ReadCartridgeByte(routine + 6) << 8;
        cell -= NativeSnapshotMemory.ScrollStorage;

        Extent16<RoomScreens> bigPink = RoomDefinitions.BigPink.Size;
        Position16<RoomScreens> screen = AttractDemoDefinitions.ChargeBeamClosedScreen;
        AssertTrue(bigPink.Contains(screen), $"the Charge Beam demo's screen {screen} lies inside Big Pink");
        AssertEqual(cell, bigPink.RowMajorIndex(screen), "the Charge Beam demo's screen is the cell the original stores to");
        AssertEqual(RoomScrollState.Closed, stored, "the Charge Beam demo closes its screen");
        Console.WriteLine($"  Charge Beam demo: screen {screen} of Big Pink is the original's cell {cell}, closed.");
    }
}
