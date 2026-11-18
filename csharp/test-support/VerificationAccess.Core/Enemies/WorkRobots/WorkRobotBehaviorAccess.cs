namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Verification access to <see cref="WorkRobotBehavior"/> members production does not use.</summary>
internal static class WorkRobotBehaviorAccess
{
    /// <summary>Exposes <see cref="WorkRobotBehavior"/> state that only verification reads.</summary>
    extension(WorkRobotBehavior self)
    {
        /// <summary>The updates the glow's step has left before the glow shows its next, while it runs.</summary>
        internal int UpdatesUntilNextGlowStep => self.glow.UpdatesLeft;
    }
}
