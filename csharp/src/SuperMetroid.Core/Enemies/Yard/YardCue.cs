namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One cue of the Yard's animations, with its typed operands.</summary>
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
    /// <param name="OptionalTarget">The segment the cue names, or none where the operand is the empty-behaviour sentinel.</param>
    internal sealed record SetHidingAnimation(YardSegment? OptionalTarget) : YardCue;

    /// <summary>Selects the movement function main AI runs.</summary>
    /// <param name="MovementFunction">The cue's MovementFunction operand.</param>
    internal sealed record SetMovementFunction(YardMovementFunction MovementFunction) : YardCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(YardCueKind Kind) : YardCue;
}
