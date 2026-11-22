namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What one of Samus's shots does on its update, before its animation advances (#1923): a beam's flight, the Wave Beam's
/// with its trail interval, the Hyper Beam's, a missile's or Super Missile's and its link's, a special attack particle's,
/// a shinespark echo's, or what a Spazer and Plasma beam's shot runs (<see cref="SamusBeamShotBehaviorDefinitions"/>).
/// Firing, a special attack, an echo's release, a Super Missile's link or a reflection installs it on the shot with the
/// state its kind keeps (<see cref="SamusProjectileOccupant.Install{TState}"/>), and a particle's change of phase moves it on to the next
/// behaviour of its kind; the shot update runs it without naming any of them. Each behaviour is one shared instance
/// (<see cref="SamusShotBehaviors"/>): what changes from update to update is the shot's, and what only its kind keeps
/// is the kind's own record (<see cref="SamusShotKindState"/>).
/// </summary>
internal abstract partial class SamusShotBehavior : SamusShotFiring
{
    /// <summary>Runs the shot's update.</summary>
    /// <param name="update">What the shot update hands every behaviour.</param>
    internal abstract void Run(SamusShotUpdate update);

    /// <summary>A fired beam shot runs this behaviour from its next update, keeping no state of its kind.</summary>
    /// <param name="held">The fired shot.</param>
    internal sealed override void Fire(SamusProjectileOccupant held) => Ensure.NotNull(held).Install(this);

    /// <summary>
    /// Whether the shot ends itself on its own next update once an enemy's collision marks it: a special attack particle
    /// does, its hit deletion (and Spazer's paired deletion) its own; every other shot is turned into its impact at once.
    /// </summary>
    internal virtual bool EndsItselfAfterEnemyHit => false;
}

#error SME6400: 'SamusShotBehavior<TState>' and 'SamusShotBehavior' both belong in SamusShotBehavior.cs; one of them needs another name
/// <summary>
/// What one of Samus's shots whose kind keeps <typeparamref name="TState"/> does on its update (#1923, #2554): a missile's,
/// a Super Missile's or its link's, a special attack particle's or a shinespark echo's. Installing it binds it to the state
/// its kind keeps (<see cref="SamusProjectileOccupant.Install{TState}"/>), so each run receives that state.
/// </summary>
/// <typeparam name="TState">The state the shot's kind keeps.</typeparam>
internal abstract partial class SamusShotBehavior<TState>
    where TState : SamusShotKindState
{
    /// <summary>Runs the shot's update.</summary>
    /// <param name="update">What the shot update hands every behaviour.</param>
    /// <param name="state">The state the shot's kind keeps.</param>
    internal abstract void Run(SamusShotUpdate update, TState state);

    /// <summary>The behaviour bound to <paramref name="state"/>, as the shot runs it.</summary>
    /// <param name="state">The state the shot's kind keeps.</param>
    internal SamusShotBehavior BoundTo(TState state) => new Bound(this, state);

    /// <summary>A behaviour bound to the state its shot's kind keeps.</summary>
    /// <param name="behavior">The behaviour.</param>
    /// <param name="state">The state it runs with.</param>
    private sealed partial class Bound(SamusShotBehavior<TState> behavior, TState state) : SamusShotBehavior
    {
        /// <inheritdoc/>
        internal override void Run(SamusShotUpdate update) => behavior.Run(update, state);
    }
}
