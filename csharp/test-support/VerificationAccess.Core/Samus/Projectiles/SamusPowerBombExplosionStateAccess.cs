namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Forces a power-bomb explosion's activity so a check can test what an explosion rejects (#1483).</summary>
internal static class SamusPowerBombExplosionStateAccess
{
    /// <summary>Marks the room's explosion active or inactive without an armed bomb.</summary>
    extension(SamusPowerBombExplosionState explosion)
    {
        /// <summary>
        /// Sets <see cref="SamusPowerBombExplosionState.IsActive"/> through its blast, which runs while its phase does
        /// (<see cref="PowerBombBlastTimeline.IsRunning"/>, #2560): marking it active starts the blast, awaiting its
        /// pre-explosion's setup, and clearing it returns the phase to <see cref="PowerBombExplosionPhase.Inactive"/>, as a
        /// finished explosion leaves it.
        /// </summary>
        internal void ForceActive(bool active)
        {
            if (active)
                explosion.Blast.Start();
            else if (explosion.Blast.Phase is not PowerBombExplosionPhase.Inactive)
                explosion.Blast.Reset();
        }
    }
}
