namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="BreathBubbleAndLavaSprayWaits"/> members production does not use.</summary>
internal static class BreathBubbleAndLavaSprayWaitsAccess
{
    /// <summary>Exposes <see cref="BreathBubbleAndLavaSprayWaits"/> state that only verification reads.</summary>
    extension(BreathBubbleAndLavaSprayWaits self)
    {
        /// <summary>Updates until the effect holding up the next breath bubble runs out; zero when none does.</summary>
        public int BubbleWaitUpdates => self.bubbleWait.UpdatesLeft;
        /// <summary>Updates until the effect holding up the next lava spray runs out; zero when none does.</summary>
        public int SprayWaitUpdates => self.sprayWait.UpdatesLeft;
    }
}
