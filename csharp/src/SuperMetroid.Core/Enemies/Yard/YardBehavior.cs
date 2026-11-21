namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// What a Yard is doing. Its look-at, solidity, touch and airborne decisions each read what the behaviour's definition
/// states (<see cref="YardBehaviorDefinitions.Behaviors"/>).
/// </summary>
public enum YardBehavior
{
    /// <summary>Crawling along its surface at its normal pace.</summary>
    NormalCrawling,

    /// <summary>Crawling aggressively after a dropped Yard lands.</summary>
    AggressiveCrawling,

    /// <summary>Hiding in its shell.</summary>
    Hiding,

    /// <summary>Dropped from its surface and falling.</summary>
    Dropped,

    /// <summary>Kicked by Samus.</summary>
    Kicked,

    /// <summary>Launched by a beam.</summary>
    BeamLaunched,
}
