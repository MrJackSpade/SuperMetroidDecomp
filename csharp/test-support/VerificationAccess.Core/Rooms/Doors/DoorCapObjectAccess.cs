using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification's view of a door cap's animation (#2480), which recorded caps' countdowns are compared with.</summary>
internal static class DoorCapObjectAccess
{
    /// <summary>Views of a cap.</summary>
    extension(DoorCapObject cap)
    {
        /// <summary>
        /// The updates the cap's shown image has left before its next image draws, or its animation ends; a cap deciding
        /// what follows decides on its next update (#2558).
        /// </summary>
        /// <exception cref="InvalidOperationException">The cap rests, counting nothing.</exception>
        internal int UpdatesBeforeNextStep =>
            PrivateState.Field<SequencedRoomObject<DoorCapSegment, DoorCapShown, SoundEffectId>,
                SequencedRoomObjectProgress<DoorCapSegment, DoorCapShown, SoundEffectId>>(cap, "_progress") switch
            {
                SequencedRoomObjectProgress<DoorCapSegment, DoorCapShown, SoundEffectId>.Playing playing => playing.Player.UpdatesLeft,
                SequencedRoomObjectProgress<DoorCapSegment, DoorCapShown, SoundEffectId>.Deciding => 1,
                var resting => throw new InvalidOperationException($"A resting door cap ({resting}) counts no updates."),
            };
    }
}
