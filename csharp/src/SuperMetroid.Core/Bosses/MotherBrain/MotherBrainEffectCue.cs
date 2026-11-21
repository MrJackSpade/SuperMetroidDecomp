namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle animations: what it does, with the values it acts on.</summary>
internal abstract partial record MotherBrainEffectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainEffectCue()
    {
    }

    /// <summary>Sets how far the projectile collides across and down from its centre.</summary>
    /// <param name="Radius">The collision half-extents <see cref="SetRadius"/> installs.</param>
    internal sealed partial record SetRadius(Extent16<RoomPixels> Radius) : MotherBrainEffectCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(MotherBrainEffectCueKind Kind) : MotherBrainEffectCue;
}
