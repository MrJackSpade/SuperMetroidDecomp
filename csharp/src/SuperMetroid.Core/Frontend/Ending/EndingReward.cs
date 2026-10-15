namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Clear-time-selected post-credits Samus presentation.</summary>
internal enum EndingReward
{
    /// <summary>Fastest clear-time tier uses the suitless reward actor and palette.</summary>
    Suitless,
    /// <summary>Middle clear-time tier reveals Samus without her helmet.</summary>
    Helmetless,
    /// <summary>Remaining clear times retain the complete armor.</summary>
    Armored,
}
