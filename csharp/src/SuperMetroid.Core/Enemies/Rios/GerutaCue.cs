namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One cue of the Geruta's animations: what it does, with the value it acts on (#2379).</summary>
internal abstract record GerutaCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private GerutaCue()
    {
    }

    /// <summary>Places the <paramref name="Flames"/> <paramref name="Below"/> pixels below the Geruta.</summary>
    /// <param name="Below">How many pixels below the Geruta its flames go, negative above it.</param>
    /// <param name="Flames">Which flames its follower shows there (#2450).</param>
    internal sealed record PlaceFlames(int Below, GerutaFlames Flames) : GerutaCue;

    /// <summary>Tells main AI the swoop's start animation has finished.</summary>
    internal sealed record SwoopStartFinished : GerutaCue;
}
