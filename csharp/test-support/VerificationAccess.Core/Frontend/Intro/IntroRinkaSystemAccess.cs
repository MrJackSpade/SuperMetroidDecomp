namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access to <see cref="IntroRinkaSystem"/> members production does not use.</summary>
internal static class IntroRinkaSystemAccess
{
    /// <summary>Reads the intro Rinka slots.</summary>
    extension(IntroRinkaSystem self)
    {
        /// <summary>The sprites of the Rinkas holding a cinematic slot, in the native descending slot order.</summary>
        internal IReadOnlyList<IntroDiscoverySprite> LiveRinkas =>
            self.slots.Reverse().OfType<IntroRinka>().Select(static rinka => rinka.Sprite).ToArray();

        /// <summary>Number of currently active Rinkas excluding the spawner actor.</summary>
        internal int ActiveCount => self.LiveRinkas.Count(static rinka => rinka.IsActive);

        /// <summary>The sprite of the live Rinka that strikes Samus, the first one launched.</summary>
        internal IntroDiscoverySprite HittingRinka =>
            self.slots.OfType<IntroRinka>().Single(static rinka => rinka.HitsSamus).Sprite;
    }
}
