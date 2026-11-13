using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A gameplay cue of Draygon's animations.</summary>
internal abstract record DraygonCue
{
    /// <summary>Starts an animation on each of the four parts: body, eye, tail and arms.</summary>
    internal sealed record StartPartAnimations(DraygonSegment Body, DraygonSegment Eye, DraygonSegment Tail, DraygonSegment Arms) : DraygonCue;

    /// <summary>Gives the eye its tracking action.</summary>
    internal sealed record SetEyeAction(DraygonEyeAction Action) : DraygonCue;

    /// <summary>Gives the part running the animation (the eye) its tracking action.</summary>
    internal sealed record SetOwnEyeAction(DraygonEyeAction Action) : DraygonCue;

    /// <summary>Displaces the body's background graphics while the tail whips.</summary>
    internal sealed record DisplaceBodyGraphics(Offset16<BackgroundPixels> Offset) : DraygonCue;

    /// <summary>Queues a sound effect with a six-entry admission limit.</summary>
    internal sealed record QueueSound(SoundEffectId Sound) : DraygonCue;

    /// <summary>Spawns a dying-Draygon sprite object at a random point of the body.</summary>
    internal sealed record SpawnDyingObject(RoomSpriteObjectKind Kind) : DraygonCue;

    /// <summary>Puts the tail and arms to sleep.</summary>
    internal sealed record ParalyseTailAndArms : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static ParalyseTailAndArms Instance { get; } = new();
    }

    /// <summary>Makes the body ignore Samus collision.</summary>
    internal sealed record SetBodyIntangible : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static SetBodyIntangible Instance { get; } = new();
    }

    /// <summary>Gives the body a new phase.</summary>
    internal sealed record SetBodyPhase(DraygonPhase Phase) : DraygonCue;

    /// <summary>Applies a tail whip's hit to a grabbed Samus.</summary>
    internal sealed record TailWhipHit : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static TailWhipHit Instance { get; } = new();
    }

    /// <summary>Spawns a goop projectile.</summary>
    internal sealed record SpawnGoop(HorizontalDirection Direction) : DraygonCue;
}
