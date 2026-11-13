namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusProjectileSystem"/> members production does not use.</summary>
internal static class SamusProjectileSystemAccess
{
    /// <summary>Reads the system's live trails.</summary>
    extension(SamusProjectileSystem self)
    {
        /// <summary>The live trails, oldest first.</summary>
        internal IReadOnlyList<SamusProjectileTrail> LiveTrails => self.Trails.trails;

        /// <summary>Starts a trail already under way, as a restored game does, and returns it.</summary>
        /// <param name="picturedBeams">The beam combination whose pictures the trail shows.</param>
        internal SamusProjectileTrail AddTrail(SuperMetroid.Core.Progression.SamusBeamCombination picturedBeams = SuperMetroid.Core.Progression.SamusBeamCombination.Power)
        {
            var trail = new SamusProjectileTrail(picturedBeams);
            self.Trails.Add(trail);
            return trail;
        }
    }
}
