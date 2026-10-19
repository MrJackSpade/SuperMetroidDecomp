using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's view of the import pipeline's room-header pointers (<see cref="RoomHeaderSources"/>).</summary>
internal static class RoomHeaderDefinitionsAccess
{
    /// <summary>Resolves and enumerates the header pointers of compiled rooms.</summary>
    extension(RoomHeaderDefinitions)
    {
        /// <inheritdoc cref="RoomHeaderSources.Room"/>
        internal static RoomHeaderDefinition Get(ushort pointer) => RoomHeaderSources.Room(pointer);

        /// <inheritdoc cref="RoomHeaderSourcesConstants.Contains"/>
        internal static bool Contains(ushort pointer) => RoomHeaderSourcesConstants.Contains(pointer);

        /// <inheritdoc cref="RoomHeaderSourcesConstants.PointerOf"/>
        internal static ushort PointerOf(RoomHeaderDefinition definition) => RoomHeaderSourcesConstants.PointerOf(definition);
    }

    /// <summary>The header pointer of a compiled room.</summary>
    extension(RoomHeaderDefinition self)
    {
        /// <summary>The bank-$8F header pointer of this room.</summary>
        internal ushort Pointer => RoomHeaderSourcesConstants.PointerOf(self);
    }

    /// <summary>The header pointer of a loaded room.</summary>
    extension(CartridgeRoomHeader self)
    {
        /// <summary>The bank-$8F header pointer of this room.</summary>
        internal ushort Pointer => RoomHeaderSourcesConstants.PointerOf(self.Definition);
    }
}
