namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One cue of Spore Spawn's body animations: what it does, with the values it acts on.</summary>
internal abstract record SporeSpawnCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private SporeSpawnCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this spore spawn cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(SporeSpawnCueReactions<TContext> reactions, TContext context);


    /// <summary>
    /// Shows a step of his death fade over his body and his room: each step of his hardening, and the last as a defeated
    /// room loads him, which the door's fade then brings in.
    /// </summary>
    /// <param name="DeathShadeStep">The step of his death fade the cue shows.</param>
    internal sealed record ShowDeathShade(SporeSpawnDeathFadeStep DeathShadeStep) : SporeSpawnCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(SporeSpawnCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ShowDeathShade(this, context);
    }

    /// <summary>Requests a sound.</summary>
    /// <param name="Sound">The sound a <see cref="QueueSound"/> cue queues.</param>
    internal sealed record QueueSound(SoundEffectId Sound) : SporeSpawnCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(SporeSpawnCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).QueueSound(this, context);
    }

    /// <summary>Selects the main-AI movement.</summary>
    /// <param name="Movement">The main-AI movement a <see cref="SetMovement"/> cue selects.</param>
    internal sealed record SetMovement(SporeSpawnMovement Movement) : SporeSpawnCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(SporeSpawnCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetMovement(this, context);
    }

    /// <summary>Sets how wide he sways and how his sway's phase turns.</summary>
    /// <param name="Radius">How wide a <see cref="SetSway"/> cue makes him sway.</param>
    /// <param name="Step">How a <see cref="SetSway"/> cue makes his sway's phase turn each movement update.</param>
    internal sealed record SetSway(SporeSpawnSwayRadius Radius, TurnStep Step) : SporeSpawnCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(SporeSpawnCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetSway(this, context);
    }

    /// <summary>Starts or stops spore generation.</summary>
    /// <param name="SporeGenerationSuppressed">Whether a <see cref="SetSporeGeneration"/> cue stops the ceiling spore emitters.</param>
    [IndependentFlag("whether an authored cue suppresses spore generation, a fact of the cue data")]
    internal sealed record SetSporeGeneration(bool SporeGenerationSuppressed) : SporeSpawnCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(SporeSpawnCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetSporeGeneration(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(SporeSpawnCueKind Kind) : SporeSpawnCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(SporeSpawnCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
