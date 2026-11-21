using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Beams fired as Samus's Shoot input fires them, through the beam input's own handling
/// (<see cref="BeamFiring.HandleBeamInput"/>), for checks of what a fire does; each tells whether the fire took a shot, which
/// the shot count shows: a fire counts its shot, and a refused fire counts none or takes back the one it counted.
/// </summary>
internal static class BeamShotFiring
{
    /// <summary>Fires beams as Samus's Shoot input does.</summary>
    extension(SamusProjectileSystem self)
    {
        /// <summary>
        /// Fires an uncharged beam as Samus holding Shoot does, <paramref name="newInput"/> the update's new presses: without
        /// Charge equipped every held update fires, and with it the first held update of a charge does.
        /// </summary>
        /// <returns>Whether the fire took a shot.</returns>
        internal bool FireUnchargedBeam(SamusTerrain terrain, SamusState samus, SamusBombProjectileSystem bombs,
            GameplayAction newInput = GameplayAction.Shoot) =>
            TakesShot(self, () => self.Firing.HandleBeamInput(terrain, samus, GameplayAction.Shoot, newInput, bombs));

        /// <summary>
        /// Fires a charged beam as Samus with Charge equipped releasing Shoot from a full charge does: the charge counts
        /// each update of a held charge until it is full, and the release holds no Shoot, <paramref name="newInput"/> the
        /// update's new presses.
        /// </summary>
        /// <returns>Whether the fire took a shot.</returns>
        /// <exception cref="InvalidOperationException">Samus has no Charge equipped, which a charged release needs.</exception>
        internal bool FireChargedBeam(SamusTerrain terrain, SamusState samus, SamusBombProjectileSystem bombs,
            GameplayAction newInput = GameplayAction.None)
        {
            if (!samus.EquippedBeams.HasAny(SamusBeamFlags.Charge))
                throw new InvalidOperationException("A charged beam is released only with Charge equipped.");
            for (int update = 0; update < BeamChargeDefinitions.FullyChargedCounter; update++)
                self.Charge.CountChargeUpdate();
            return TakesShot(self, () => self.Firing.HandleBeamInput(terrain, samus, GameplayAction.None, newInput, bombs));
        }

        /// <summary>Fires the Hyper Beam as Samus with it enabled holding Shoot does.</summary>
        /// <returns>Whether the fire took a shot.</returns>
        /// <exception cref="InvalidOperationException">Samus has no Hyper Beam.</exception>
        internal bool FireHyperBeam(SamusTerrain terrain, SamusState samus, SamusBombProjectileSystem bombs)
        {
            if (!samus.HyperBeamEnabled)
                throw new InvalidOperationException("The Hyper Beam fires only once it is enabled.");
            return TakesShot(self, () => self.Firing.HandleBeamInput(terrain, samus, GameplayAction.Shoot, GameplayAction.None, bombs));
        }
    }

    /// <summary>Whether <paramref name="fire"/> takes one of <paramref name="projectiles"/>' shots: their shot count stands one higher after it.</summary>
    private static bool TakesShot(SamusProjectileSystem projectiles, Func<BeamFiringResult?> fire)
    {
        int before = projectiles.Pool.ShotCount.Count;
        _ = fire();
        return projectiles.Pool.ShotCount.Count == before + 1;
    }
}
