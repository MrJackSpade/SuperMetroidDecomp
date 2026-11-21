namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One cue of Ridley's body animations: what it does, with the values it acts on.</summary>
internal abstract partial record RidleyBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private RidleyBodyCue()
    {
    }

    /// <summary>Moves Ridley by the cue's displacement.</summary>
    /// <param name="Displacement">How far a <see cref="Move"/> cue moves Ridley.</param>
    internal sealed partial record Move(Offset16<RoomPixels> Displacement) : RidleyBodyCue;

    /// <summary>Selects how far Ridley's legs reach, which sets his claws' height.</summary>
    /// <param name="Height">The claw height a <see cref="SetClawHeight"/> cue selects.</param>
    internal sealed partial record SetClawHeight(RidleyClawHeight Height) : RidleyBodyCue;

    /// <summary>Faces the cue's way and mirrors the tail (#2165).</summary>
    /// <param name="Facing">The way Ridley then faces.</param>
    internal sealed partial record Flip(HorizontalDirection Facing) : RidleyBodyCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(RidleyBodyCueKind Kind) : RidleyBodyCue;
}
