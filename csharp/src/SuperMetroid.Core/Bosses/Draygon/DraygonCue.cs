using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A gameplay cue of Draygon's animations.</summary>
internal abstract record DraygonCue
{
    /// <summary>Closes the hierarchy to the cases declared here.</summary>
    private DraygonCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this draygon cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context);

    /// <summary>Starts an animation on each of the four parts: body, eye, tail and arms.</summary>
    internal sealed record StartPartAnimations(DraygonSegment Body, DraygonSegment Eye, DraygonSegment Tail, DraygonSegment Arms) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).StartPartAnimations(this, context);
    }

    /// <summary>
    /// Sets the eye tracking Samus facing the cue's way (#2165, #2401). Only the eye's animations track, so whether an
    /// animation names the eye or the part running it, the eye tracks.
    /// </summary>
    /// <param name="Facing">The way the eye faces.</param>
    internal sealed record TrackWithEye(HorizontalDirection Facing) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).TrackWithEye(this, context);
    }

    /// <summary>Shifts the body's picture against the body while the tail whips.</summary>
    internal sealed record ShiftBodyPicture(Offset16<RoomPixels> Shift) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ShiftBodyPicture(this, context);
    }

    /// <summary>Queues a sound effect at normal priority.</summary>
    internal sealed record QueueSound(SoundEffectId Sound) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).QueueSound(this, context);
    }

    /// <summary>Spawns a dying-Draygon effect actor at a random point of the body.</summary>
    internal sealed record SpawnDyingEffect(RoomEffectActorKind Kind) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SpawnDyingEffect(this, context);
    }

    /// <summary>Rests the tail and arms on their last frames.</summary>
    internal sealed record ParalyseTailAndArms : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ParalyseTailAndArms(this, context);

        /// <summary>The cue's only value.</summary>
        internal static ParalyseTailAndArms Instance { get; } = new();
    }

    /// <summary>Makes the body ignore Samus collision.</summary>
    internal sealed record SetBodyIntangible : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetBodyIntangible(this, context);

        /// <summary>The cue's only value.</summary>
        internal static SetBodyIntangible Instance { get; } = new();
    }

    /// <summary>Gives the body a new phase.</summary>
    internal sealed record SetBodyPhase(DraygonPhase Phase) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetBodyPhase(this, context);
    }

    /// <summary>Applies a tail whip's hit to a grabbed Samus.</summary>
    internal sealed record TailWhipHit : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).TailWhipHit(this, context);

        /// <summary>The cue's only value.</summary>
        internal static TailWhipHit Instance { get; } = new();
    }

    /// <summary>Spawns a goop projectile.</summary>
    internal sealed record SpawnGoop(HorizontalDirection Direction) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SpawnGoop(this, context);
    }
}
