namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Allocation and resting phases of the power-bomb explosion, plus Crystal Flash.</summary>
public abstract partial record PowerBombExplosionPhase
{
    /// <summary>Allows only the cases below.</summary>
    private PowerBombExplosionPhase()
    {
    }

    /// <summary>No explosion is running; an armed bomb may still be waiting for its fuse.</summary>
    public sealed partial record Inactive : PowerBombExplosionPhase;

    /// <summary>The phases of a running blast, from its start until its animation ends.</summary>
    public abstract partial record Running : PowerBombExplosionPhase
    {
        /// <summary>Allows only the cases below.</summary>
        private Running()
        {
        }

        /// <summary>
        /// The white pre-explosion: the harmless flash grows with decreasing speed to
        /// <see cref="Abilities.PowerBombExplosionDefinitions.PreExplosionWhiteLimit"/>.
        /// </summary>
        public sealed partial record PreExplosionWhite : Running;

        /// <summary>The yellow pre-explosion: advances the pre-scaled lead-in shapes before initializing the damaging radius.</summary>
        public sealed partial record PreExplosionYellow : Running;

        /// <summary>The yellow explosion: the damaging radius accelerates while the yellow blast grows with it.</summary>
        public sealed partial record ExplosionYellow : Running;

        /// <summary>
        /// The white explosion: runs its pre-scaled expanding shapes, one an update for
        /// <see cref="Abilities.PowerBombExplosionDefinitions.WhiteOvalRadii"/>, keeping the last for the fade.
        /// </summary>
        public sealed partial record ExplosionWhite : Running;

        /// <summary>
        /// The afterglow: fades the glow colour's components through its <see cref="Abilities.PowerBombExplosionDefinitions.AfterglowFades"/>
        /// steps before cleanup and a possible Crystal Flash handoff.
        /// </summary>
        public sealed partial record Afterglow : Running;

        /// <summary>
        /// The Crystal Flash explosion: reuses the yellow expansion curve only up to
        /// <see cref="Abilities.PowerBombExplosionDefinitions.CrystalFlashRadiusLimit"/>, without arming another Power Bomb.
        /// </summary>
        public sealed partial record CrystalFlashExplosion : Running;

        /// <summary>The Crystal Flash afterglow: keeps the small final blast while colours fade, then clears the shared explosion state.</summary>
        public sealed partial record CrystalFlashAfterglow : Running;

        /// <summary>Started explosion awaiting its first effect update and its pre-explosion setup.</summary>
        public sealed partial record PendingPreExplosionSetup : Running;
    }
}
