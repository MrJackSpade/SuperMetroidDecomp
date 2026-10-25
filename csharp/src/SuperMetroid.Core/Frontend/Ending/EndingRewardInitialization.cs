namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Where a reward actor starts and which palette it draws with (#1416: the values are arbitrary; the bank-$8B
/// initialization routine words that named them are verification's <c>EndingRewardNativeWords</c>).
/// </summary>
internal enum EndingRewardInitialization
{
    /// <summary>Suitless actors, at (120,136) with sprite palette five.</summary>
    Suitless = 1,
    /// <summary>The suited body and arm, at (120,152) with sprite palette six.</summary>
    SuitedBody,
    /// <summary>The helmeted head, at (124,108) with sprite palette six.</summary>
    HelmetedHead,
    /// <summary>The helmetless head, at (121,107) with sprite palette five.</summary>
    HelmetlessHead,
}
