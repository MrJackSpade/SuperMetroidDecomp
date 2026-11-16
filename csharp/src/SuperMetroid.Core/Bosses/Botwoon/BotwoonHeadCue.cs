namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One gameplay cue of Botwoon's head animations: what it does, with the values it acts on.</summary>
internal abstract record BotwoonHeadCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private BotwoonHeadCue()
    {
    }

    /// <summary>Sets the head's collision radius for the cue's facing (#2175): the facing of the animation that raises it.</summary>
    /// <param name="Facing">The way the head faces.</param>
    internal sealed record ApplyFacingRadius(CompassDirection Facing) : BotwoonHeadCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(BotwoonHeadCueKind Kind) : BotwoonHeadCue;
}
