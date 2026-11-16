namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Where the station's escape from Ceres Ridley's room stands (#2228).</summary>
internal enum CeresEscapeStage
{
    /// <summary>Ridley has not got away: the room shows no getaway and no warning.</summary>
    AwaitingGetaway,

    /// <summary>The getaway flies Ridley off over the turning floor; the room main runs it.</summary>
    Getaway,

    /// <summary>The self-destruct readies and types its warning, in Ridley's place in the enemy update.</summary>
    SelfDestruct,

    /// <summary>The warning is complete: only its alarm's colours keep stepping, in Ridley's place in the enemy update.</summary>
    AlarmOnly,
}
