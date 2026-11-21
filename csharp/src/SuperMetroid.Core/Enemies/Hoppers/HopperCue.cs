namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One cue of the Sidehoppers' and Dessgeegas' animations: what it does, with the values it acts on.</summary>
internal abstract partial record HopperCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private HopperCue()
    {
    }

    /// <summary>Queues an enemy-library sound.</summary>
    /// <param name="Sound">The sound the cue queues.</param>
    internal sealed partial record PlaySound(SoundEffectId Sound) : HopperCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(HopperCueKind Kind) : HopperCue;
}
