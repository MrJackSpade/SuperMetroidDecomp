namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One cue of the Yard's animations: what it does, with the values it acts on.</summary>
internal abstract record YardCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private YardCue()
    {
    }

    /// <summary>Moves the Yard by a signed pixel offset in X and Y.</summary>
    /// <param name="Offset">The cue's signed pixel offset, horizontal and vertical.</param>
    internal sealed record MoveBy(Offset16<RoomPixels> Offset) : YardCue;

    /// <summary>Sets the attachment direction and the airborne facing it implies.</summary>
    /// <param name="Direction">The surface direction a <see cref="SetDirection"/> cue attaches the Yard in.</param>
    internal sealed record SetDirection(YardDirection Direction) : YardCue;

    /// <summary>Names the animation the Yard hides with, or none.</summary>
    /// <param name="OptionalTarget">The segment the cue names, or none where the cue names the empty behaviour.</param>
    internal sealed record SetHidingAnimation(YardSegment? OptionalTarget) : YardCue;

    /// <summary>The Yard begins the crawl <paramref name="Crawl"/> along its surface, as its crawl animation starts.</summary>
    /// <param name="Crawl">The crawl the Yard begins: along which surface, which way.</param>
    internal sealed record BeginCrawling(YardMovement Crawl) : YardCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(YardCueKind Kind) : YardCue;
}
