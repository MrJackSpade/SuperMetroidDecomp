using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's view of the import pipeline's room-state pointers (<see cref="RoomStateSources"/>).</summary>
internal static class RoomStateDefinitionsAccess
{
    /// <summary>Resolves compiled states by pointer.</summary>
    extension(RoomStateDefinitions)
    {
        /// <inheritdoc cref="RoomStateSourcesConstants.State"/>
        internal static CartridgeRoomState Get(ushort pointer) => RoomStateSourcesConstants.State(pointer);
    }

    /// <summary>The pointers of a room's states.</summary>
    extension(RoomStateSelectionDefinitions)
    {
        /// <summary>The pointers of <see cref="RoomStateSelectionDefinitions.GetStates"/>, default first.</summary>
        internal static IReadOnlyList<ushort> GetStatePointers(RoomHeaderDefinition room) =>
            RoomStateSelectionDefinitions.GetStates(room).Select(RoomStateSourcesConstants.PointerOf).ToArray();
    }

    /// <summary>The bank-$8F pointer of a state.</summary>
    extension(CartridgeRoomState self)
    {
        /// <summary>The bank-$8F pointer of this state.</summary>
        internal ushort Pointer => RoomStateSourcesConstants.PointerOf(self);
    }
}
