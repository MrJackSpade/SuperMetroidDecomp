namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One cue of the Yard's programs, with the operands its native instruction carries.</summary>
internal abstract record YardCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private YardCue()
    {
    }

    /// <summary>Moves the Yard by a signed pixel offset in X and Y (<c>MoveByPixelsInY</c>, $A3:CC5F).</summary>
    /// <param name="Offset">The cue's signed pixel offset, horizontal and vertical.</param>
    internal sealed record MoveBy(Offset16<RoomPixels> Offset) : YardCue;

    /// <summary>Sets the attachment direction and the airborne facing it implies (<c>DirectionInY</c>, $A3:CC48).</summary>
    /// <param name="Direction">The surface direction, 0 through 7, a <see cref="SetDirection"/> cue attaches the Yard in.</param>
    internal sealed record SetDirection([property: NativeWord("surface orientation word 0-7 an animation opcode installs; YardDirectionDefinitions decodes it")] int Direction) : YardCue;

    /// <summary>Names the animation the Yard hides with, or none (<c>HidingInstListInY</c>, $A3:CC3F).</summary>
    /// <param name="OptionalTarget">The program the cue names, or none where the native operand is the $CF5F sentinel.</param>
    internal sealed record SetHidingProgram(YardProgram? OptionalTarget) : YardCue;

    /// <summary>Selects the movement function main AI runs (<c>MovementFunctionInY</c>, $A3:CC36).</summary>
    /// <param name="MovementFunction">The cue's MovementFunction operand.</param>
    internal sealed record SetMovementFunction(YardMovementFunction MovementFunction) : YardCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(YardCueKind Kind) : YardCue;
}
