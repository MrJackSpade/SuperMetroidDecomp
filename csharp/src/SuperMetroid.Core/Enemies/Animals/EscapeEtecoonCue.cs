namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Etecoons' programs, with its typed operands.</summary>
internal abstract record EscapeEtecoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EscapeEtecoonCue()
    {
    }

    /// <summary>Continues at the target once the rising acid is above Y 206.</summary>
    /// <param name="Target">The program a branch continues at.</param>
    internal sealed record GoToIfAcidHigh(EscapeEtecoonProgram Target) : EscapeEtecoonCue;

    /// <summary>Moves the Etecoon horizontally.</summary>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    internal sealed record Move(int Offset) : EscapeEtecoonCue;

    /// <summary>Installs the Etecoon's movement behaviour.</summary>
    /// <param name="FrameBehavior">The cue's FrameBehavior operand.</param>
    internal sealed record SetFrameBehavior(EscapeEtecoonFrameBehavior FrameBehavior) : EscapeEtecoonCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(EscapeEtecoonCueKind Kind) : EscapeEtecoonCue;
}
