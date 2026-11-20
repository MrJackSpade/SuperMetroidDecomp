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

        /// <summary>
        /// Runs one update as <see cref="SamusProjectileSystem.StepFrame"/> does, the HUD producer's firing pass and then
        /// the shots, and returns the sound request the firing pass made through the system's audio requests, or none when
        /// it made none: the firing sound and priority the producer queued, as its requests received them.
        /// </summary>
        /// <param name="terrain">The room terrain the shots collide with and react to.</param>
        /// <param name="samus">Samus, who fires.</param>
        /// <param name="buttons">The actions the producer reads.</param>
        /// <param name="cameraPosition">The camera's place, for the shots' offscreen handling.</param>
        /// <param name="samusBombs">Samus's bombs, carrying the shared projectile cooldown.</param>
        /// <param name="progression">The progression a Spacetime shot's overwrite turns on and off.</param>
        internal SoundRequest? StepFrameFiringSound(
            global::SuperMetroid.Core.Samus.Movement.SamusTerrain terrain,
            global::SuperMetroid.Core.Samus.SamusState samus,
            ProjectileButtonInput buttons,
            Position16<RoomPixels> cameraPosition,
            SamusBombProjectileSystem samusBombs,
            global::SuperMetroid.Core.Progression.SharedGameState progression)
        {
            self.AudioRequests.BeginRecordedUpdate();
            self.FireFromHud(terrain, samus, buttons, samusBombs, projectileProducerEnabled: true);
            IReadOnlyList<SoundRequest> firing = [.. self.AudioRequests.SoundRequests];
            self.HandleShots(terrain, samus, cameraPosition, samusBombs, progression);
            return firing switch
            {
                [] => null,
                [var request] => request,
                _ => throw new InvalidOperationException(
                    $"The firing pass made {firing.Count} sound requests where it queues at most its firing sound: [{string.Join(", ", firing)}]."),
            };
        }
    }
}
