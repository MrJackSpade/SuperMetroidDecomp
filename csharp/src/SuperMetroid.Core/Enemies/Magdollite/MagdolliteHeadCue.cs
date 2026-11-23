namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One cue of the Magdollite head's animations: what it does, with the values it acts on (#2071, #2554).</summary>
internal abstract partial record MagdolliteHeadCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MagdolliteHeadCue()
    {
    }

    /// <summary>Moves the head.</summary>
    /// <param name="Offset">How far the head moves.</param>
    internal sealed partial record Move(Offset16<RoomPixels> Offset) : MagdolliteHeadCue;

    /// <summary>Moves the head and the body following it, the pillar it stands on.</summary>
    /// <param name="Offset">How far both parts move.</param>
    internal sealed partial record MoveWithPillar(Offset16<RoomPixels> Offset) : MagdolliteHeadCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(MagdolliteHeadCueKind Kind) : MagdolliteHeadCue;
}
