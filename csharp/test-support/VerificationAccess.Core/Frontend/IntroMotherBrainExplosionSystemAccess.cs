using SuperMetroid.Core.Frontend;

/// <summary>Verification access to <see cref="IntroMotherBrainExplosionSystem"/> members production does not use.</summary>
internal static class IntroMotherBrainExplosionSystemAccess
{
    /// <summary>Counts live actors retained by the intro Mother Brain explosion system.</summary>
    extension(IntroMotherBrainExplosionSystem self)
    {
        /// <summary>All eight actors remain allocated until the later page-two crossfade.</summary>
        internal int ActiveCount => self.actors.Count(actor => actor.IsActive);
    }
}
