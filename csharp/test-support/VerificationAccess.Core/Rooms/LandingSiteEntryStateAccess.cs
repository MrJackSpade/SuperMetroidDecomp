using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="LandingSiteEntryState"/> members production does not use.</summary>
internal static class LandingSiteEntryStateAccess
{
    /// <summary>Loads landing-site entry fixtures from their door pointer and native actor state.</summary>
    extension(LandingSiteEntryState)
    {
        /// <summary>Selects the intro landing-cutscene door and its compiled command-E record.</summary>
        internal static LandingSiteEntryState LoadLandingCutscene(ISnesAddressSpace bus) =>
            LandingSiteEntryState.Load(bus, LandingSiteRomData.LandingCutsceneDoorPointer);

        /// <summary>
        /// Uses the compiled door/room definitions and selects its transfer from
        /// <c>LibBG_ScrollingSky_Tilemaps_LandingSite</c> at <c>$8F:B76A</c>.
        /// </summary>
        internal static LandingSiteEntryState Load(ISnesAddressSpace bus, ushort doorPointer)
        {
            Ensure.NotNull(bus);
            CartridgeDoorHeader door = DoorDefinitions.Get(doorPointer);

            // The destination is a bank-$8F room pointer. Rejecting any other room
            // is important: the library-background list below is specific to Landing Site and
            // silently applying it to an arbitrary door would manufacture a plausible image.
            if (door.DestinationRoom != RoomHeaderDefinitions.LandingSite)
            {
                throw new InvalidDataException(
                    $"Door $83:{doorPointer:X4} targets room {door.DestinationRoom.Name}, not Landing Site.");
            }

            RoomHeaderDefinition room = RoomHeaderDefinitions.LandingSite;
            RoomIdentity roomIdentity = new(room.AreaIndex, room.RoomIndex);
            if (roomIdentity != RoomIdentities.LandingSite)
            {
                throw new InvalidDataException(
                    $"Landing Site room definition has logical identity {roomIdentity}, expected " +
                    $"{RoomIdentities.LandingSite}.");
            }

            ushort statePointer = RoomStateSelectionDefinitions.Select(
                RoomHeaderDefinitions.LandingSite, default);
            CartridgeRoomState state = RoomStateDefinitions.Get(statePointer);
            LibraryBackgroundInstruction transfer =
                LibraryBackgroundProgramDefinitions.GetDoorTransfer(
                    unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
                    doorPointer);
            return new LandingSiteEntryState(
                RoomIdentity: roomIdentity,
                RoomMapPosition: room.MapPosition,
                UpScroller: room.UpScroller,
                DownScroller: room.DownScroller);
        }
    }
}
