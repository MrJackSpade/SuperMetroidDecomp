namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One cue of the Geruta's animations: what it does, with the value it acts on (#2379).</summary>
internal abstract partial record GerutaCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private GerutaCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this geruta cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(GerutaCueReactions<TContext> reactions, TContext context);


    /// <summary>Places the <paramref name="Flames"/> <paramref name="Below"/> pixels below the Geruta.</summary>
    /// <param name="Below">How many pixels below the Geruta its flames go, negative above it.</param>
    /// <param name="Flames">Which flames its follower shows there (#2450).</param>
    internal sealed partial record PlaceFlames(int Below, GerutaFlames Flames) : GerutaCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(GerutaCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).PlaceFlames(this, context);
    }

    /// <summary>Tells main AI the swoop's start animation has finished.</summary>
    internal sealed partial record SwoopStartFinished : GerutaCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(GerutaCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SwoopStartFinished(this, context);
    }
}

/// <summary>What the owner of a <see cref="GerutaCue"/> does on each of its cases (#2553).</summary>
/// <param name="PlaceFlames">Reacts to a <see cref="GerutaCue.PlaceFlames"/>.</param>
/// <param name="SwoopStartFinished">Reacts to a <see cref="GerutaCue.SwoopStartFinished"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record GerutaCueReactions<TContext>(Action<GerutaCue.PlaceFlames, TContext> PlaceFlames, Action<GerutaCue.SwoopStartFinished, TContext> SwoopStartFinished);
