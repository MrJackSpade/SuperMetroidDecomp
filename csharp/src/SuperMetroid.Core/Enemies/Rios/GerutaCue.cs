namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One cue of the Geruta's animations: what it does, with the value it acts on (#2379).</summary>
internal abstract record GerutaCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private GerutaCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this geruta cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(GerutaCueReactions reactions);


    /// <summary>Places the <paramref name="Flames"/> <paramref name="Below"/> pixels below the Geruta.</summary>
    /// <param name="Below">How many pixels below the Geruta its flames go, negative above it.</param>
    /// <param name="Flames">Which flames its follower shows there (#2450).</param>
    internal sealed record PlaceFlames(int Below, GerutaFlames Flames) : GerutaCue
    {
        /// <inheritdoc/>
        internal override void React(GerutaCueReactions reactions) => Ensure.NotNull(reactions).PlaceFlames(this);
    }

    /// <summary>Tells main AI the swoop's start animation has finished.</summary>
    internal sealed record SwoopStartFinished : GerutaCue
    {
        /// <inheritdoc/>
        internal override void React(GerutaCueReactions reactions) => Ensure.NotNull(reactions).SwoopStartFinished(this);
    }
}

/// <summary>What the owner of a <see cref="GerutaCue"/> does on each of its cases (#2553).</summary>
/// <param name="PlaceFlames">Reacts to a <see cref="GerutaCue.PlaceFlames"/>.</param>
/// <param name="SwoopStartFinished">Reacts to a <see cref="GerutaCue.SwoopStartFinished"/>.</param>
internal sealed record GerutaCueReactions(Action<GerutaCue.PlaceFlames> PlaceFlames, Action<GerutaCue.SwoopStartFinished> SwoopStartFinished);
