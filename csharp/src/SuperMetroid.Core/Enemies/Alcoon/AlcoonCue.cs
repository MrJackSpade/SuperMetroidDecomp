namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One cue of the Alcoon's animations: what it does, with the values it acts on.</summary>
internal abstract partial record AlcoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private AlcoonCue()
    {
    }

    /// <summary>Spits a fireball by the cue's launch and plays the fire sound (#2151).</summary>
    /// <param name="Launch">How the fireball leaves: down, straight ahead or up.</param>
    internal sealed partial record Fire(AlcoonFireballLaunch Launch) : AlcoonCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(AlcoonCueKind Kind) : AlcoonCue;
}
