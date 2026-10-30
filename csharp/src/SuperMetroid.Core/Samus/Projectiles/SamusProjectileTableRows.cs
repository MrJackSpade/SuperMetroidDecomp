using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where a Samus projectile's rows lie in the beam firing tables.</summary>
internal static class SamusProjectileTableRows
{
    /// <summary>The firing-table rows of a projectile identity.</summary>
    extension(SamusProjectileType type)
    {
        /// <summary>
        /// The row the beam firing tables (cooldowns, trail programs) are read at for this projectile: the beam
        /// combination, in the charged block when charged, and in the echo block for an echo. Missile-family trails read
        /// their own rows.
        /// </summary>
        internal int BeamTableRow =>
            type.BeamCombination.TableIndex +
            (type.IsChargedBeam ? SamusFiringTableDefinitions.ChargedRowOffset : 0) +
            (type.Echo is null ? 0 : SamusFiringTableDefinitions.EchoRowOffset);
    }
}
