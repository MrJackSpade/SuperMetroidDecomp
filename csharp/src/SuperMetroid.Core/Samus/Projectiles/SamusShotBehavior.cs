namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What one of Samus's shots does on its update, before its animation advances (#1923): a beam's flight, the Wave Beam's
/// with its trail interval, the Hyper Beam's, a missile's or Super Missile's and its link's, a special attack particle's,
/// a shinespark echo's, or what a Spazer and Plasma beam's shot runs (<see cref="SamusBeamShotBehaviorDefinitions"/>).
/// Firing, a special attack, an echo's release, a Super Missile's link or a reflection installs it on the shot with the
/// state its kind keeps (<see cref="SamusProjectileOccupant.Install"/>), and a particle's change of phase moves it on to the next
/// behaviour of its kind; the shot update runs it without naming any of them. Each behaviour is one shared instance
/// (<see cref="SamusShotBehaviors"/>): what changes from update to update is the shot's, and what only its kind keeps
/// is the kind's own record (<see cref="SamusShotKindState"/>).
/// </summary>
internal abstract class SamusShotBehavior : SamusShotFiring
{
    /// <summary>Runs the shot's update.</summary>
    /// <param name="update">What the shot update hands every behaviour.</param>
    internal abstract void Run(SamusShotUpdate update);

    /// <summary>A fired beam shot runs this behaviour from its next update, keeping no state of its kind.</summary>
    /// <param name="held">The fired shot.</param>
    internal sealed override void Fire(SamusProjectileOccupant held) => Ensure.NotNull(held).Install(this, kindState: null);
}
