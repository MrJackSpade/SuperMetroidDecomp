namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>One cue of an enemy drop's animations (#2554): what it does.</summary>
internal abstract record EnemyDeathAndDropCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EnemyDeathAndDropCue()
    {
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(EnemyDeathAndDropCueKind Kind) : EnemyDeathAndDropCue;
}
