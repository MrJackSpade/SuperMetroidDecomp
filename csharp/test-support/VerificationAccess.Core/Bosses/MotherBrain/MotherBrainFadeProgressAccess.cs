namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="MotherBrainFadeProgress"/> members production does not use.</summary>
internal static class MotherBrainFadeProgressAccess
{
    /// <summary>Exposes <see cref="MotherBrainFadeProgress"/> state that only verification reads.</summary>
    extension(MotherBrainFadeProgress self)
    {
        /// <summary>The image the fade shows, or null before its first.</summary>
        public MotherBrainFadeImage? Shown => self.player.Shown;
    }
}
