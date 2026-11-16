namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Where a reward actor starts and which colours it draws with.
/// </summary>
internal enum EndingRewardInitialization
{
    /// <summary>Suitless actors, at (120,136) in the suitless colours.</summary>
    Suitless,
    /// <summary>The suited body and arm, at (120,152) in the suit's colours.</summary>
    SuitedBody,
    /// <summary>The helmeted head, at (124,108) in the suit's colours.</summary>
    HelmetedHead,
    /// <summary>The helmetless head, at (121,107) in the suitless colours.</summary>
    HelmetlessHead,
}
