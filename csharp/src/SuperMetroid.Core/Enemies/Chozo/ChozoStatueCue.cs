namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One cue of the Chozo statue's animations, with its typed operands.</summary>
internal abstract record ChozoStatueCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ChozoStatueCue()
    {
    }

    /// <summary>Clears spikes under the operand's footstep offset.</summary>
    /// <param name="Offset">Where a <see cref="Footstep"/> cue probes for spikes, relative to the statue; only its X is meaningful.</param>
    internal sealed record Footstep(Offset16<RoomPixels> Offset) : ChozoStatueCue;

    /// <summary>Moves the statue by the operand's carry step and carries Samus.</summary>
    /// <param name="Step">The facing and carry pose a <see cref="Move"/> cue moves by.</param>
    internal sealed record Move(ChozoCarryStep Step) : ChozoStatueCue;

    /// <summary>Sets the frame behaviour main AI runs.</summary>
    /// <param name="FrameBehavior">The cue's FrameBehavior operand.</param>
    internal sealed record SetFrameBehavior(ChozoStatueFrameBehavior FrameBehavior) : ChozoStatueCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(ChozoStatueCueKind Kind) : ChozoStatueCue;
}
