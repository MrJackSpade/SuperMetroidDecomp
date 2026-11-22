namespace SuperMetroid.Core.Samus.Projectiles;

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
