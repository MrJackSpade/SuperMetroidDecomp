namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One cue of the Magdollite tracking overlay's animations: what it does, with the values it acts on (#2071, #2554).</summary>
internal abstract record MagdolliteOverlayCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MagdolliteOverlayCue()
    {
    }

    /// <summary>Records an enemy-library sound while the overlay is on screen.</summary>
    /// <param name="Sound">The sound the cue plays.</param>
    internal sealed record PlaySoundIfOnScreen(SoundEffectId Sound) : MagdolliteOverlayCue;

    /// <summary>Places the overlay at the throw origin shifted by the cue's offset.</summary>
    /// <param name="Offset">Where from the throw origin the overlay is placed.</param>
    internal sealed record ShiftFromThrowOrigin(Offset16<RoomPixels> Offset) : MagdolliteOverlayCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MagdolliteOverlayCueKind Kind) : MagdolliteOverlayCue;
}
