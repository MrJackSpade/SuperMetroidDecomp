namespace SuperMetroid.Core.Samus;

/// <summary>
/// Seeds the counts a check's Samus starts with, which her producers otherwise reach only through their own sequences: a
/// beam charge the projectile pass would have published, and a hurt flash part-way through its run.
/// </summary>
internal static class SamusCompatibilityAccess
{
    /// <summary>Seeds Samus's shared counts while she is being set up.</summary>
    extension(SamusState self)
    {
        /// <summary>Publishes <paramref name="count"/> as her beam charge, as the projectile pass does, and returns her.</summary>
        internal SamusState WithBeamChargeCount(int count)
        {
            self.BeamCharge.FlareCounter = count;
            return self;
        }

        /// <summary>Holds her hurt flash at <paramref name="count"/> and returns her.</summary>
        internal SamusState WithHurtFlashCount(int count)
        {
            self.HurtFlash.HoldAt(count);
            return self;
        }
    }
}
