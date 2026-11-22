namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Reads where a sequenced room object's animations stand.</summary>
internal static class SequencedRoomObjectAccess
{
    /// <summary>Reads the object's progress.</summary>
    extension<TSegment, TFrame, TEvent>(SequencedRoomObject<TSegment, TFrame, TEvent> self)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>Whether the object rests: its animation has ended with nothing to follow, until what it waits for wakes it.</summary>
        internal bool Resting => self.Progress is SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.Resting or
            SequencedRoomObjectProgress<TSegment, TFrame, TEvent>.RestingUnshown;
    }
}
