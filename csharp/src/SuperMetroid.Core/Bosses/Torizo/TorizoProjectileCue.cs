using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's projectile animations: what it does, with the values it acts on.</summary>
internal abstract record TorizoProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TorizoProjectileCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of this torizo projectile cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the torizo projectile cue is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(TorizoProjectileCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Requests a sound at the priority the cue states.</summary>
    /// <param name="Sound">The sound a <see cref="PlaySound"/> cue queues.</param>
    /// <param name="Priority">How strongly the sound competes for a place in its library's request queue.</param>
    internal sealed record PlaySound(SoundEffectId Sound, SoundRequestPriority Priority) : TorizoProjectileCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(TorizoProjectileCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).PlaySound(this, context);
    }

    /// <summary>Scatters the projectile randomly about its position.</summary>
    /// <param name="Spread">The random scatter a <see cref="Scatter"/> cue applies.</param>
    internal sealed record Scatter(EnemyProjectileScatter Spread) : TorizoProjectileCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(TorizoProjectileCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Scatter(this, context);
    }

    /// <summary>Sets the projectile's collision radius.</summary>
    /// <param name="Radius">The collision radius a <see cref="SetRadius"/> cue sets.</param>
    internal sealed record SetRadius(Extent16<RoomPixels> Radius) : TorizoProjectileCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(TorizoProjectileCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).SetRadius(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(TorizoProjectileCueKind Kind) : TorizoProjectileCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(TorizoProjectileCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}

/// <summary>What each case of a <see cref="TorizoProjectileCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the torizo projectile cue is made into.</typeparam>
/// <param name="PlaySound">Makes a <see cref="TorizoProjectileCue.PlaySound"/> into the result.</param>
/// <param name="Scatter">Makes a <see cref="TorizoProjectileCue.Scatter"/> into the result.</param>
/// <param name="SetRadius">Makes a <see cref="TorizoProjectileCue.SetRadius"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="TorizoProjectileCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record TorizoProjectileCueCases<TContext, TResult>(Func<TorizoProjectileCue.PlaySound, TContext, TResult> PlaySound, Func<TorizoProjectileCue.Scatter, TContext, TResult> Scatter, Func<TorizoProjectileCue.SetRadius, TContext, TResult> SetRadius, Func<TorizoProjectileCue.Signal, TContext, TResult> Signal);
