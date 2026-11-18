using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification's view of a downward gate's animation (#2480): the image it shows and where its play stands.</summary>
internal static class DownwardGateObjectAccess
{
    /// <summary>Views of a gate.</summary>
    extension(DownwardGateObject gate)
    {
        /// <summary>The gate's animation playing.</summary>
        internal AnimationPlayer<DownwardGateSegment, DownwardGateImage, NoAnimationCue> Animation =>
            PrivateState.Field<SequencedRoomObject<DownwardGateSegment, DownwardGateImage, NoAnimationCue>,
                AnimationPlayer<DownwardGateSegment, DownwardGateImage, NoAnimationCue>>(gate, "_player");
    }
}
