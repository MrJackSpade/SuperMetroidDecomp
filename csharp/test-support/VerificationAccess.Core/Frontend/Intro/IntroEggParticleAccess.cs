namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access to <see cref="IntroEggParticle"/> members production does not use.</summary>
internal static class IntroEggParticleAccess
{
    /// <summary>Reads whether an egg particle's sprite remains active.</summary>
    extension(IntroEggParticle self)
    {
        /// <summary>Whether the particle's internal discovery sprite is active.</summary>
        internal bool IsActive => self.sprite.IsActive;
    }
}
