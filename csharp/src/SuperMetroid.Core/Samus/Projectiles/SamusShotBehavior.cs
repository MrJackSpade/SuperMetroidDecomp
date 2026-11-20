namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What one of Samus's shots does on its update, before its animation advances (#1923): a beam's flight, the Wave Beam's
/// with its trail interval, the Hyper Beam's, a missile's or Super Missile's and its link's, a special attack particle's,
/// a shinespark echo's, or what a Spazer and Plasma beam's shot runs (<see cref="SamusBeamShotBehaviorDefinitions"/>).
/// Firing, a special attack, an echo's release, a Super Missile's link or a reflection installs it on the shot with the
/// state its kind keeps (<see cref="SamusProjectile.Install"/>), and a particle's change of phase moves it on to the next
/// behaviour of its kind; the shot update runs it without naming any of them. Each behaviour is one shared instance
/// (<see cref="SamusShotBehaviors"/>) when its kind keeps nothing of its own; a kind that keeps a record of its own
/// (<see cref="SamusShotKindState"/>) runs a behaviour created with that record, which hands it to the update it runs
/// (#2554). What changes from update to update is the shot's.
/// </summary>
internal abstract class SamusShotBehavior
{
    /// <summary>
    /// What the shot's kind keeps beyond what every shot keeps, which a behaviour of a kind that keeps a record is created
    /// with; null for a behaviour whose kind keeps nothing beyond the shot's own state.
    /// </summary>
    internal virtual SamusShotKindState? KindState => null;

    /// <summary>Runs the shot's update.</summary>
    /// <param name="update">What the shot update hands every behaviour.</param>
    internal abstract void Run(SamusShotUpdate update);

    /// <summary>
    /// Fails loudly when a shot that runs this behaviour cannot be fired: nothing for every behaviour a shot fires with;
    /// a behaviour whose shot runs data rather than the game's code refuses the shot as it is fired
    /// (<see cref="ShotRefusedWhenFired"/>).
    /// </summary>
    internal virtual void AdmitFiring()
    {
    }
}
