using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification access to <see cref="PhantoonPart"/> members production does not use.</summary>
internal static class PhantoonEnemyStateAccess
{
    /// <summary>Reads the program a Phantoon part plays.</summary>
    extension(PhantoonPart self)
    {
        /// <summary>
        /// The program the part is playing: the segment its playing phase plays, or, before its first play, the one its
        /// animation starts at.
        /// </summary>
        internal PhantoonPartSegment PlayingProgram()
        {
            AnimationPlayback<PhantoonPartSegment> playback = self.Playback;
            return playback.Animation.Phases[playback.Place.Phase].Segment;
        }
    }
}
