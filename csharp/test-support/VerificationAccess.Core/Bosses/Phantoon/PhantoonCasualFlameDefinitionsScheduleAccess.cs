
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification access to <see cref="PhantoonCasualFlameSchedule"/> members production does not use.</summary>
internal static class PhantoonCasualFlameDefinitionsScheduleAccess
{
    /// <summary>Exposes the number of casual-flame emissions in Phantoon's authored schedule.</summary>
    extension(PhantoonCasualFlameSchedule self)
    {
        /// <summary>Number of casual-flame spawn entries in this schedule.</summary>
        internal int Length => self.Count;
    }
}
