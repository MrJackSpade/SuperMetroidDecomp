namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One cue of the Fune's and Namihe's animations: what it does, with the values it acts on.</summary>
internal abstract record FuneNamiheCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private FuneNamiheCue()
    {
    }

    /// <summary>Spits the spitter's species' fireball (#2151).</summary>
    /// <param name="Direction">The way the fireball flies.</param>
    internal sealed record Fire(HorizontalDirection Direction) : FuneNamiheCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(FuneNamiheCueKind Kind) : FuneNamiheCue;
}
