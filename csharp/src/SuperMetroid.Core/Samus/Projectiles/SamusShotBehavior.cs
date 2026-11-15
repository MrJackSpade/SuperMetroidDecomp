namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What one of Samus's shots does on its update, before its animation advances (#1923): a beam's flight, the Wave Beam's
/// with its trail interval, the Hyper Beam's, a missile's or Super Missile's and its link's, a special attack particle's,
/// a shinespark echo's, or what a Spazer and Plasma beam's shot runs (<see cref="SamusBeamFrameBehaviorDefinitions"/>).
/// Firing, a special attack, an echo's release, a reflection or a particle's change of phase installs it on the shot
/// (<see cref="SamusProjectile.Behavior"/>); the shot update runs it without naming any of them. Each behaviour is one
/// shared instance (<see cref="SamusShotBehaviors"/>) and keeps no state: what changes from update to update is the
/// shot's.
/// </summary>
internal abstract class SamusShotBehavior
{
    /// <summary>Runs the shot's update.</summary>
    /// <param name="update">What the shot update hands every behaviour.</param>
    internal abstract void Run(SamusShotUpdate update);

    /// <summary>
    /// Fails loudly when a shot that runs this behaviour cannot be fired: nothing for every behaviour the port reproduces;
    /// a behaviour that runs data rather than the game's code refuses the shot as it is fired
    /// (<see cref="ShotRefusedWhenFired"/>).
    /// </summary>
    internal virtual void AdmitFiring()
    {
    }
}
