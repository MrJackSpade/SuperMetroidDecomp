namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access to <see cref="IntroRinkaSystem"/> members production does not use.</summary>
internal static class IntroRinkaSystemAccess
{
    /// <summary>Reads the intro Rinkas.</summary>
    extension(IntroRinkaSystem self)
    {
        /// <summary>The sprites of the live Rinkas, in spawn order.</summary>
        internal IReadOnlyList<IntroDiscoverySprite> LiveRinkas =>
            self.rinkas.Select(static rinka => rinka.Sprite).ToArray();

        /// <summary>Number of currently active Rinkas excluding the spawner actor.</summary>
        internal int ActiveCount => self.LiveRinkas.Count(static rinka => rinka.IsActive);

        /// <summary>The sprite of the live Rinka that strikes Samus, the first one launched.</summary>
        internal IntroDiscoverySprite HittingRinka =>
            self.rinkas.Single(static rinka => rinka.Flight == CinematicSpriteFrameBehavior.IntroRinkaMovingHitsSamus).Sprite;
    }
}
