namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusProjectileSystem"/> members production does not use.</summary>
internal static class SamusProjectileSystemAccess
{
    /// <summary>Reads the system's live trails.</summary>
    extension(SamusProjectileSystem self)
    {
        /// <summary>The live trails, oldest first.</summary>
        internal IReadOnlyList<SamusProjectileTrail> LiveTrails =>
            PrivateState.Field<ProjectileTrails, List<SamusProjectileTrail>>(self.Trails, "trails");

        /// <summary>Starts a trail already under way, as a restored game does, and returns it.</summary>
        /// <param name="origin">The place of the shot that left the trail, which both sparkles stand from.</param>
        /// <param name="picturedBeams">The beam combination whose pictures the trail shows.</param>
        internal SamusProjectileTrail AddTrail(Position16<RoomPixels> origin,
            SuperMetroid.Core.Progression.SamusBeamCombination picturedBeams = SuperMetroid.Core.Progression.SamusBeamCombination.Power)
        {
            var trail = new SamusProjectileTrail(picturedBeams, origin);
            self.Trails.Add(trail);
            return trail;
        }
    }
}
