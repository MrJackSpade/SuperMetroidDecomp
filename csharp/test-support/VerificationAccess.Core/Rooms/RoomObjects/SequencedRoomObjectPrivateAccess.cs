namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The private state of <see cref="SuperMetroid.Core.Rooms.RoomObjects.SequencedRoomObject{TSegment, TFrame, TEvent}"/> checks read (#2570, #2558):
/// where its animations stand, reached by name through <see cref="PrivateState"/>, so production keeps it private.
/// </summary>
internal static class SequencedRoomObjectPrivateAccess
{
    /// <summary>The private state checks read.</summary>
    extension<TSegment, TFrame, TEvent>(global::SuperMetroid.Core.Rooms.RoomObjects.SequencedRoomObject<TSegment, TFrame, TEvent> self)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>Where the object's animations stand: the private field <c>_progress</c>, reached by name.</summary>
        internal SequencedRoomObjectProgress<TSegment, TFrame, TEvent> Progress =>
            PrivateState.Field<global::SuperMetroid.Core.Rooms.RoomObjects.SequencedRoomObject<TSegment, TFrame, TEvent>, SequencedRoomObjectProgress<TSegment, TFrame, TEvent>>(self, "_progress");

        /// <summary>Whether the object rests: its animation has ended with nothing to follow, until what it waits for wakes it.</summary>
        internal bool Resting => self.Progress is SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Resting or SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.RestingUnshown;
    }
}
