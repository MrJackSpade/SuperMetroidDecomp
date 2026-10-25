namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Etecoons' programs, with the operands its native instruction carries.</summary>
internal abstract record EscapeEtecoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EscapeEtecoonCue()
    {
    }

    /// <summary>Continues at the target once the rising acid is above Y $00CE (<c>GotoIfAcidYLessThanCE</c>, $B3:E545).</summary>
    /// <param name="Target">The program a branch continues at.</param>
    internal sealed record GoToIfAcidHigh(EscapeEtecoonProgram Target) : EscapeEtecoonCue;

    /// <summary>Moves the Etecoon horizontally (<c>AddXPosition</c>, $B3:E610).</summary>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    internal sealed record Move(int Offset) : EscapeEtecoonCue;

    /// <summary>Installs the Etecoon's movement routine (<c>SetPreInstruction</c>, $B3:806B).</summary>
    /// <param name="PreInstruction">The cue's PreInstruction operand.</param>
    internal sealed record SetPreInstruction(EscapeEtecoonPreInstruction PreInstruction) : EscapeEtecoonCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(EscapeEtecoonCueKind Kind) : EscapeEtecoonCue;
}
