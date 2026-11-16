namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One cue of the Ki-Hunter's animations: what it does, with the values it acts on.</summary>
internal abstract record KiHunterCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private KiHunterCue()
    {
    }

    /// <summary>Spits acid the cue's way and starts the spit wait (#2151).</summary>
    /// <param name="Direction">The way the acid flies.</param>
    internal sealed record Spit(HorizontalDirection Direction) : KiHunterCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(KiHunterCueKind Kind) : KiHunterCue;
}
