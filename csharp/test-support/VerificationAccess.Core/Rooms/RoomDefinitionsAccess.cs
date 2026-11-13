using SuperMetroid.AssetExtraction.Rooms;
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
    }

    /// <summary>The header pointer of a compiled room.</summary>
    extension(RoomDefinition self)
    {
        /// <summary>The bank-$8F header pointer of this room.</summary>
        internal ushort Pointer => RoomHeaderSourcesConstants.PointerOf(self);
    }

    /// <summary>The header pointer of a loaded room.</summary>
    extension(SelectedRoom self)
    {
        /// <summary>The bank-$8F header pointer of this room.</summary>
        internal ushort Pointer => RoomHeaderSourcesConstants.PointerOf(self.Definition);
    }
}
