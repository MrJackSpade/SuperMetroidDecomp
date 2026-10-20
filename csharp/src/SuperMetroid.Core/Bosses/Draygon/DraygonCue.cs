using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A gameplay cue of Draygon's programs.</summary>
internal abstract record DraygonCue
{
    /// <summary>Starts a program on each of the four parts, as <c>Instruction_Draygon_SetInstList_Body_Eye_Tail_Arms</c> does.</summary>
    internal sealed record StartPartPrograms(DraygonProgram Body, DraygonProgram Eye, DraygonProgram Tail, DraygonProgram Arms) : DraygonCue;

    /// <summary>Selects the room-loading interrupt's HUD-draw command.</summary>
    internal sealed record BeginHudDraw : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static BeginHudDraw Instance { get; } = new();
    }

    /// <summary>Gives the eye its tracking function.</summary>
    internal sealed record SetEyeFunction(DraygonEyeFunction Function) : DraygonCue;

    /// <summary>Gives the part running the program (the eye) its tracking function.</summary>
    internal sealed record SetOwnEyeFunction(DraygonEyeFunction Function) : DraygonCue;

    /// <summary>Displaces the body's BG2 graphics while the tail whips.</summary>
    internal sealed record DisplaceBodyGraphics(Offset16<Bg2Pixels> Offset) : DraygonCue;

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

    /// <summary>Gives the body a new AI function.</summary>
    internal sealed record SetBodyFunction(DraygonAiFunction Function) : DraygonCue;

    /// <summary>Applies a tail whip's hit to a grabbed Samus.</summary>
    internal sealed record TailWhipHit : DraygonCue
    {
        /// <summary>The cue's only value.</summary>
        internal static TailWhipHit Instance { get; } = new();
    }

    /// <summary>Spawns a goop projectile.</summary>
    internal sealed record SpawnGoop(HorizontalDirection Direction) : DraygonCue;
}
