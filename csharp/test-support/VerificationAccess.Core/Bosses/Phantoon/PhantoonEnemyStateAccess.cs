using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification access to <see cref="PhantoonEnemyState"/> members production does not use.</summary>
internal static class PhantoonEnemyStateAccess
{
    /// <summary>Reads the program a Phantoon part plays.</summary>
    extension(PhantoonEnemyState self)
    {
        /// <summary>
        /// The program <paramref name="part"/> is playing: the segment its playing phase plays, or, before its first play,
        /// the one its animation starts at.
        /// </summary>
        /// <exception cref="InvalidOperationException"><paramref name="part"/> is not one of Phantoon's parts.</exception>
        internal PhantoonPartSegment PlayingProgram(RoomEnemySlot part)
        {
            AnimationPlayback<PhantoonPartSegment> playback = self.PartPlayback(part);
            return playback.Animation.Phases[playback.Place.Phase].Segment;
        }
    }
}
