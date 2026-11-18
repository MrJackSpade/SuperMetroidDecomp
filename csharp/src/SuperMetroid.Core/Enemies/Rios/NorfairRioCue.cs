namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One cue of the Norfair Rio's animations: what it does, with the value it acts on (#2379).</summary>
internal abstract record NorfairRioCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private NorfairRioCue()
    {
    }

    /// <summary>Places the <paramref name="Flames"/> <paramref name="Below"/> pixels below the Norfair Rio.</summary>
    /// <param name="Below">How many pixels below the Norfair Rio its flames go, negative above it.</param>
    /// <param name="Flames">Which flames its follower shows there (#2450).</param>
    internal sealed record PlaceFlames(int Below, NorfairRioFlames Flames) : NorfairRioCue;

    /// <summary>Tells main AI the swoop's start animation has finished.</summary>
    internal sealed record SwoopStartFinished : NorfairRioCue;
}
