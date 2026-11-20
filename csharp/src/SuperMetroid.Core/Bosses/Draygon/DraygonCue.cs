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
    internal abstract void React(DraygonCueReactions reactions);

    /// <summary>Starts an animation on each of the four parts: body, eye, tail and arms.</summary>
    internal sealed record StartPartAnimations(DraygonSegment Body, DraygonSegment Eye, DraygonSegment Tail, DraygonSegment Arms) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).StartPartAnimations(this);
    }

    /// <summary>
    /// Sets the eye tracking Samus facing the cue's way (#2165, #2401). Only the eye's animations track, so whether an
    /// animation names the eye or the part running it, the eye tracks.
    /// </summary>
    /// <param name="Facing">The way the eye faces.</param>
    internal sealed record TrackWithEye(HorizontalDirection Facing) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).TrackWithEye(this);
    }

    /// <summary>Shifts the body's picture against the body while the tail whips.</summary>
    internal sealed record ShiftBodyPicture(Offset16<RoomPixels> Shift) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).ShiftBodyPicture(this);
    }

    /// <summary>Queues a sound effect at normal priority.</summary>
    internal sealed record QueueSound(SoundEffectId Sound) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).QueueSound(this);
    }

    /// <summary>Spawns a dying-Draygon effect actor at a random point of the body.</summary>
    internal sealed record SpawnDyingEffect(RoomEffectActorKind Kind) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).SpawnDyingEffect(this);
    }

    /// <summary>Rests the tail and arms on their last frames.</summary>
    internal sealed record ParalyseTailAndArms : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).ParalyseTailAndArms(this);

        /// <summary>The cue's only value.</summary>
        internal static ParalyseTailAndArms Instance { get; } = new();
    }

    /// <summary>Makes the body ignore Samus collision.</summary>
    internal sealed record SetBodyIntangible : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).SetBodyIntangible(this);

        /// <summary>The cue's only value.</summary>
        internal static SetBodyIntangible Instance { get; } = new();
    }

    /// <summary>Gives the body a new phase.</summary>
    internal sealed record SetBodyPhase(DraygonPhase Phase) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).SetBodyPhase(this);
    }

    /// <summary>Applies a tail whip's hit to a grabbed Samus.</summary>
    internal sealed record TailWhipHit : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).TailWhipHit(this);

        /// <summary>The cue's only value.</summary>
        internal static TailWhipHit Instance { get; } = new();
    }

    /// <summary>Spawns a goop projectile.</summary>
    internal sealed record SpawnGoop(HorizontalDirection Direction) : DraygonCue
    {
        /// <inheritdoc/>
        internal override void React(DraygonCueReactions reactions) => Ensure.NotNull(reactions).SpawnGoop(this);
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
/// <param name="SetBodyPhase">Reacts to a <see cref="DraygonCue.SetBodyPhase"/>.</param>
/// <param name="TailWhipHit">Reacts to a <see cref="DraygonCue.TailWhipHit"/>.</param>
/// <param name="SpawnGoop">Reacts to a <see cref="DraygonCue.SpawnGoop"/>.</param>
internal sealed record DraygonCueReactions(Action<DraygonCue.StartPartAnimations> StartPartAnimations, Action<DraygonCue.TrackWithEye> TrackWithEye, Action<DraygonCue.ShiftBodyPicture> ShiftBodyPicture, Action<DraygonCue.QueueSound> QueueSound, Action<DraygonCue.SpawnDyingEffect> SpawnDyingEffect, Action<DraygonCue.ParalyseTailAndArms> ParalyseTailAndArms, Action<DraygonCue.SetBodyIntangible> SetBodyIntangible, Action<DraygonCue.SetBodyPhase> SetBodyPhase, Action<DraygonCue.TailWhipHit> TailWhipHit, Action<DraygonCue.SpawnGoop> SpawnGoop);
