namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One cue of Ridley's body animations: what it does, with the values it acts on.</summary>
internal abstract record RidleyBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private RidleyBodyCue()
    {
    }

    /// <summary>
    /// In Ceres, when Samus has under 30 energy, shortens the hover to eight updates and continues at the cue's target.
    /// </summary>
    /// <param name="Target">The segment the cue continues at.</param>
    internal sealed record GoToIfCeresAndSamusLowEnergy(RidleyBodySegment Target) : RidleyBodyCue;

    /// <summary>Continues at the cue's target unless Ridley faces left.</summary>
    /// <param name="Target">The segment the cue continues at.</param>
    internal sealed record GoToUnlessFacingLeft(RidleyBodySegment Target) : RidleyBodyCue;

    /// <summary>Moves Ridley by the cue's displacement.</summary>
    /// <param name="Displacement">How far a <see cref="Move"/> cue moves Ridley.</param>
    internal sealed record Move(Offset16<RoomPixels> Displacement) : RidleyBodyCue;

    /// <summary>Selects how far Ridley's legs reach, which sets his claws' height.</summary>
    /// <param name="Height">The claw height a <see cref="SetClawHeight"/> cue selects.</param>
    internal sealed record SetClawHeight(RidleyClawHeight Height) : RidleyBodyCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(RidleyBodyCueKind Kind) : RidleyBodyCue;
}
