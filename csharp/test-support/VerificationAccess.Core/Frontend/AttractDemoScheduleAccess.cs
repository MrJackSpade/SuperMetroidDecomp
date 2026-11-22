using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verification questions about which demonstrations the title plays, asked by playing its schedule.</summary>
internal static class AttractDemoScheduleAccess
{
    /// <summary>Questions about the demonstrations a schedule plays.</summary>
    extension(AttractDemoSchedule self)
    {
        /// <summary>
        /// Whether the title plays <paramref name="playlist"/>: from a soft reset, each set's scenes in order as each
        /// demonstration plays to its end, set after set, the first set coming round again after the last. The schedule is
        /// soft reset again afterwards, where a boot leaves it.
        /// </summary>
        internal bool Plays(IReadOnlyList<AttractDemoSet> playlist)
        {
            self.Reset();
            bool plays = playlist.Append(playlist[0]).All(set => PlaysNext(self, set));
            self.Reset();
            return plays;
        }
    }

    /// <summary>Whether the next set <paramref name="schedule"/> plays, from its first scene to its last, is <paramref name="set"/>.</summary>
    private static bool PlaysNext(AttractDemoSchedule schedule, AttractDemoSet set)
    {
        var scenes = new List<AttractDemoScene> { schedule.RequestFromTitle() };
        while (schedule.RouteAfterScene(AttractPlaybackEnd.Completed) is AttractDemoRoute.NextScene next)
            scenes.Add(next.Scene);
        return scenes.SequenceEqual(set.Scenes);
    }
}
