using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification's view of a downward gate's animation (#2480, #2558): the image it rests on or the animation it plays.</summary>
internal static class DownwardGateObjectAccess
{
    /// <summary>Views of a gate.</summary>
    extension(DownwardGateObject gate)
    {
        /// <summary>Where the gate's animations stand.</summary>
        private SequencedRoomObjectProgress<DownwardGateSegment, DownwardGateImage, NoAnimationCue> GateProgress =>
            PrivateState.Field<SequencedRoomObject<DownwardGateSegment, DownwardGateImage, NoAnimationCue>,
                SequencedRoomObjectProgress<DownwardGateSegment, DownwardGateImage, NoAnimationCue>>(gate, "_progress");

        /// <summary>The animation a gate plays.</summary>
        /// <exception cref="InvalidOperationException">The gate plays no animation.</exception>
        internal AnimationPlayer<DownwardGateSegment, DownwardGateImage, NoAnimationCue> Animation =>
            gate.GateProgress is SequencedRoomObjectProgress<DownwardGateSegment, DownwardGateImage, NoAnimationCue>.Playing playing
                ? playing.Player
                : throw new InvalidOperationException($"The downward gate plays no animation: {gate.GateProgress}.");
    }
}
