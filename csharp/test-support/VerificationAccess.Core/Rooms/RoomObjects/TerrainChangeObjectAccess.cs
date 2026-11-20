using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a terrain-change object does with its change (#2570): production keeps the change only as the frames it plays, so
/// checks read the terrain it draws, and when it draws first, from the frames playing.
/// </summary>
internal static class TerrainChangeObjectAccess
{
    /// <summary>Reads the terrain the object draws.</summary>
    extension(TerrainChangeObject self)
    {
        /// <summary>The frames the object draws, in the order it draws them, from its first.</summary>
        internal IReadOnlyList<TerrainChangeFrame> DrawnFrames => FramesOf(self).PlayedFrames;

        /// <summary>Whether the object draws <paramref name="change"/>'s frames: the terrain that change writes, in its order.</summary>
        internal bool Draws(TerrainChange change) => self.DrawnFrames.SequenceEqual(Ensure.NotNull(change).Frames);

        /// <summary>
        /// Whether the object, before its first update, starts <paramref name="change"/>: it draws that change's frames and
        /// draws its first once that change's first delay runs out.
        /// </summary>
        internal bool Starts(TerrainChange change) =>
            FramesOf(self).Shown is null && FramesOf(self).UpdatesLeft == Ensure.NotNull(change).FirstFrameDelay && self.Draws(change);
    }

    /// <summary>The frames <paramref name="terrainChange"/> plays.</summary>
    private static AnimationPlayer<RoomObjectFrameRun, TerrainChangeFrame, NoAnimationCue> FramesOf(TerrainChangeObject terrainChange) =>
        PrivateState.Field<TerrainChangeObject, AnimationPlayer<RoomObjectFrameRun, TerrainChangeFrame, NoAnimationCue>>(terrainChange, "_frames");
}
