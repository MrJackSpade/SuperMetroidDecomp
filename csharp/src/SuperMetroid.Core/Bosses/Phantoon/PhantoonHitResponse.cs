namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>What a shot meeting Phantoon's hitboxes does in one of his phases (<see cref="PhantoonPhase.Hit"/>).</summary>
internal enum PhantoonHitResponse
{
    /// <summary>The shot hurts him without counting toward a round.</summary>
    Hurts,

    /// <summary>The shot does nothing: he waits on his eye, or is dying or dead.</summary>
    Passes,

    /// <summary>The shot hurts him in an eye's vulnerable window, its damage counting toward ending the round.</summary>
    OpensVulnerableWindow,

    /// <summary>The shot hurts him while he swoops, its damage counting toward ending the round.</summary>
    Swooping,
}
