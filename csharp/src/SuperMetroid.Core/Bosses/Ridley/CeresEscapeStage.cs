namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Where the station's escape from Ceres Ridley's room stands (#2228).</summary>
internal enum CeresEscapeStage
{
    /// <summary>
    /// The escape's first stage, which begins as Ridley hands the room to it: the getaway flies him off over the turning
    /// floor, which the room main runs while the floor shows.
    /// </summary>
    Getaway,

    /// <summary>The self-destruct readies and types its warning, in Ridley's place in the enemy update.</summary>
    SelfDestruct,

    /// <summary>The warning is complete: only its alarm's colours keep stepping, in Ridley's place in the enemy update.</summary>
    AlarmOnly,
}
