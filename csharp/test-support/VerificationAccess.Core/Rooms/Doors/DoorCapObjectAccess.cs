using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification's view of a door cap's animation (#2480), which recorded caps' countdowns are compared with.</summary>
internal static class DoorCapObjectAccess
{
    /// <summary>Views of a cap.</summary>
    extension(DoorCapObject cap)
    {
        /// <summary>The updates the cap's shown image has left before its next image draws, or its animation ends.</summary>
        internal int UpdatesBeforeNextStep =>
            PrivateState.Field<SequencedRoomObject<DoorCapSegment, DoorCapFrame, SoundEffectId>,
                AnimationPlayer<DoorCapSegment, DoorCapFrame, SoundEffectId>>(cap, "_player").UpdatesLeft;
    }
}
