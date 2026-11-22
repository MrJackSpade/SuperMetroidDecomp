using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A gameplay cue of Draygon's animations.</summary>
internal abstract partial record DraygonCue
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
    internal sealed partial record StartPartAnimations(DraygonSegment Body, DraygonSegment Eye, DraygonSegment Tail, DraygonSegment Arms) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).StartPartAnimations(this, context);
    }

    /// <summary>
    /// Sets the eye tracking Samus facing the cue's way (#2165, #2401). Only the eye's animations track, so whether an
    /// animation names the eye or the part running it, the eye tracks.
    /// </summary>
    /// <param name="Facing">The way the eye faces.</param>
    internal sealed partial record TrackWithEye(HorizontalDirection Facing) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).TrackWithEye(this, context);
    }

    /// <summary>Shifts the body's picture against the body while the tail whips.</summary>
    internal sealed partial record ShiftBodyPicture(Offset16<RoomPixels> Shift) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ShiftBodyPicture(this, context);
    }

    /// <summary>Queues a sound effect at normal priority.</summary>
    internal sealed partial record QueueSound(SoundEffectId Sound) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).QueueSound(this, context);
    }

    /// <summary>Spawns a dying-Draygon effect actor at a random point of the body.</summary>
    internal sealed partial record SpawnDyingEffect(RoomEffectActorKind Kind) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SpawnDyingEffect(this, context);
    }

    /// <summary>Rests the tail and arms on their last frames.</summary>
    internal sealed partial record ParalyseTailAndArms : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ParalyseTailAndArms(this, context);

        /// <summary>The cue's only value.</summary>
        internal static ParalyseTailAndArms Instance { get; } = new();
    }

    /// <summary>Makes the body ignore Samus collision.</summary>
    internal sealed partial record SetBodyIntangible : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetBodyIntangible(this, context);

        /// <summary>The cue's only value.</summary>
        internal static SetBodyIntangible Instance { get; } = new();
    }

    /// <summary>Gives the body its phase of releasing Samus.</summary>
    internal sealed partial record ReleaseSamus : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static ReleaseSamus Instance { get; } = new();

        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ReleaseSamus(this, context);
    }

    /// <summary>Applies a tail whip's hit to a grabbed Samus.</summary>
    internal sealed partial record TailWhipHit : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).TailWhipHit(this, context);

        /// <summary>The cue's only value.</summary>
        internal static TailWhipHit Instance { get; } = new();
    }

    /// <summary>Spawns a goop projectile.</summary>
    internal sealed partial record SpawnGoop(HorizontalDirection Direction) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DraygonCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SpawnGoop(this, context);
    }
}

/// <summary>What the owner of a <see cref="DraygonCue"/> does on each of its cases (#2553).</summary>
/// <param name="StartPartAnimations">Reacts to a <see cref="DraygonCue.StartPartAnimations"/>.</param>
/// <param name="TrackWithEye">Reacts to a <see cref="DraygonCue.TrackWithEye"/>.</param>
/// <param name="ShiftBodyPicture">Reacts to a <see cref="DraygonCue.ShiftBodyPicture"/>.</param>
/// <param name="QueueSound">Reacts to a <see cref="DraygonCue.QueueSound"/>.</param>
/// <param name="SpawnDyingEffect">Reacts to a <see cref="DraygonCue.SpawnDyingEffect"/>.</param>
/// <param name="ParalyseTailAndArms">Reacts to a <see cref="DraygonCue.ParalyseTailAndArms"/>.</param>
/// <param name="SetBodyIntangible">Reacts to a <see cref="DraygonCue.SetBodyIntangible"/>.</param>
/// <param name="ReleaseSamus">Reacts to a <see cref="DraygonCue.ReleaseSamus"/>.</param>
/// <param name="TailWhipHit">Reacts to a <see cref="DraygonCue.TailWhipHit"/>.</param>
/// <param name="SpawnGoop">Reacts to a <see cref="DraygonCue.SpawnGoop"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record DraygonCueReactions<TContext>(Action<DraygonCue.StartPartAnimations, TContext> StartPartAnimations, Action<DraygonCue.TrackWithEye, TContext> TrackWithEye, Action<DraygonCue.ShiftBodyPicture, TContext> ShiftBodyPicture, Action<DraygonCue.QueueSound, TContext> QueueSound, Action<DraygonCue.SpawnDyingEffect, TContext> SpawnDyingEffect, Action<DraygonCue.ParalyseTailAndArms, TContext> ParalyseTailAndArms, Action<DraygonCue.SetBodyIntangible, TContext> SetBodyIntangible, Action<DraygonCue.ReleaseSamus, TContext> ReleaseSamus, Action<DraygonCue.TailWhipHit, TContext> TailWhipHit, Action<DraygonCue.SpawnGoop, TContext> SpawnGoop);
