using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's view of the import pipeline's room-header pointers (<see cref="RoomHeaderSources"/>).</summary>
internal static class RoomDefinitionsAccess
{
    /// <summary>Resolves and enumerates the header pointers of compiled rooms.</summary>
    extension(RoomDefinitions)
    {
        /// <inheritdoc cref="RoomHeaderSources.Room"/>
        internal static RoomDefinition Get(ushort pointer) => RoomHeaderSources.Room(pointer);

        /// <inheritdoc cref="RoomHeaderSourcesConstants.Contains"/>
        internal static bool Contains(ushort pointer) => RoomHeaderSourcesConstants.Contains(pointer);

        /// <inheritdoc cref="RoomHeaderSourcesConstants.PointerOf"/>
        internal static ushort PointerOf(RoomDefinition definition) => RoomHeaderSourcesConstants.PointerOf(definition);

        /// <summary>
        /// The first authored room of <paramref name="area"/>, for fixtures whose behaviour depends only on the area
        /// Samus is in.
        /// </summary>
        internal static RoomDefinition FirstIn(AreaId area) => RetailRooms.All.First(room => room.Area == area);

        /// <summary>
        /// The compiled room whose cartridge header holds <paramref name="area"/> and the per-area room number
        /// <paramref name="number"/>, for oracle checks of cartridge tables indexed by that number.
        /// </summary>
        internal static RoomDefinition WithCartridgeNumber(CartridgeImportAddressSpace rom, AreaId area, int number) =>
            RetailRooms.All.Single(room => room.Area == area && room.CartridgeNumber(rom) == number);
    }

    /// <summary>The header pointer of a compiled room.</summary>
    extension(RoomDefinition self)
    {
        /// <summary>The bank-$8F header pointer of this room.</summary>
        internal ushort Pointer => RoomHeaderSourcesConstants.PointerOf(self);

        /// <summary>
        /// The per-area room number the cartridge stores in this room's header, which cartridge tables index by and
        /// gameplay never reads.
        /// </summary>
        internal int CartridgeNumber(CartridgeImportAddressSpace rom) =>
            Ensure.NotNull(rom).ReadCartridgeByte(0x8f0000 | RoomHeaderSourcesConstants.PointerOf(self));
    }

    /// <summary>The header pointer of a loaded room.</summary>
    extension(SelectedRoom self)
    {
        /// <summary>The bank-$8F header pointer of this room.</summary>
        internal ushort Pointer => RoomHeaderSourcesConstants.PointerOf(self.Definition);
    }
}
