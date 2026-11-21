using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A gameplay cue of Draygon's animations.</summary>
internal abstract partial record DraygonCue
{
    /// <summary>Starts an animation on each of the four parts: body, eye, tail and arms.</summary>
    internal sealed partial record StartPartAnimations(DraygonSegment Body, DraygonSegment Eye, DraygonSegment Tail, DraygonSegment Arms) : DraygonCue;

    /// <summary>
    /// Sets the eye tracking Samus facing the cue's way (#2165, #2401). Only the eye's animations track, so whether an
    /// animation names the eye or the part running it, the eye tracks.
    /// </summary>
    /// <param name="Facing">The way the eye faces.</param>
    internal sealed partial record TrackWithEye(HorizontalDirection Facing) : DraygonCue;

    /// <summary>Shifts the body's picture against the body while the tail whips.</summary>
    internal sealed partial record ShiftBodyPicture(Offset16<RoomPixels> Shift) : DraygonCue;

    /// <summary>Queues a sound effect at normal priority.</summary>
    internal sealed partial record QueueSound(SoundEffectId Sound) : DraygonCue;

    /// <summary>Spawns a dying-Draygon effect actor at a random point of the body.</summary>
    internal sealed partial record SpawnDyingEffect(RoomEffectActorKind Kind) : DraygonCue;

    /// <summary>Rests the tail and arms on their last frames.</summary>
    internal sealed partial record ParalyseTailAndArms : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static ParalyseTailAndArms Instance { get; } = new();
    }

    /// <summary>Makes the body ignore Samus collision.</summary>
    internal sealed partial record SetBodyIntangible : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static SetBodyIntangible Instance { get; } = new();
    }

    /// <summary>Gives the body a new phase.</summary>
    internal sealed partial record SetBodyPhase(DraygonPhase Phase) : DraygonCue;

    /// <summary>Applies a tail whip's hit to a grabbed Samus.</summary>
    internal sealed partial record TailWhipHit : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static TailWhipHit Instance { get; } = new();
    }

    /// <summary>Spawns a goop projectile.</summary>
    internal sealed partial record SpawnGoop(HorizontalDirection Direction) : DraygonCue;
}
