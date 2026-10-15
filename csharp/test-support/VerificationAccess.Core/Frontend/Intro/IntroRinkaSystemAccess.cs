namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access to <see cref="IntroRinkaSystem"/> members production does not use.</summary>
internal static class IntroRinkaSystemAccess
{
    /// <summary>Reads the intro Rinka slots and their native spawn identities.</summary>
    extension(IntroRinkaSystem self)
    {
        /// <summary>Rinkas holding a cinematic slot, in the native descending slot order.</summary>
        internal IReadOnlyList<IntroDiscoverySprite> LiveRinkas
        {
            get
            {
                IntroDiscoverySprite?[] slots = self.slots;
                IntroDiscoverySprite spawner = self.spawner;
                return slots.Reverse().OfType<IntroDiscoverySprite>()
                    .Where(actor => !ReferenceEquals(actor, spawner)).ToArray();
            }
        }

        /// <summary>Number of currently active Rinkas excluding the spawner actor.</summary>
        internal int ActiveCount => self.LiveRinkas.Count(static rinka => rinka.IsActive);

        /// <summary>The live Rinka spawned with init parameter <paramref name="parameter"/>.</summary>
        internal IntroDiscoverySprite Rinka(int parameter) =>
            self.LiveRinkas.Single(rinka => rinka.GeneralTimer == parameter);
    }
}
