using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's view of the import pipeline's room-state pointers (<see cref="RoomStateSources"/>).</summary>
internal static class RoomStateDefinitionsAccess
{
    /// <summary>Resolves compiled states by pointer.</summary>
    extension(RoomStateDefinitions)
    {
        /// <inheritdoc cref="RoomStateSourcesConstants.State"/>
        internal static RoomStateSettings Get(ushort pointer) => RoomStateSourcesConstants.State(pointer);
    }

    /// <summary>The pointers of a room's states.</summary>
    extension(RoomStateSelection)
    {
        /// <summary>The pointers of <see cref="RoomStateSelection.GetStates"/>, default first.</summary>
        internal static IReadOnlyList<ushort> GetStatePointers(RoomHeaderDefinition room) =>
            RoomStateSelection.GetStates(room).Select(RoomStateSourcesConstants.PointerOf).ToArray();
    }

    /// <summary>The bank-$8F pointer of a state.</summary>
    extension(RoomStateSettings self)
    {
        /// <summary>The bank-$8F pointer of this state.</summary>
        internal ushort Pointer => RoomStateSourcesConstants.PointerOf(self);
    }
}
