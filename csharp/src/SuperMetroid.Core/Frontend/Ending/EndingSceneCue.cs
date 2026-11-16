namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What the ending scenes' animations have the escape scenes do (#2197): the explosion's and the result text's cues.</summary>
internal enum EndingSceneCue
{
    /// <summary>Has the right-hand explosion stars wait for the flyaway to carry them away.</summary>
    WaitForFlyaway,
    /// <summary>Fades the exploding planet's colours.</summary>
    FadeExplosionColours,
    /// <summary>Spawns the explosion silhouette.</summary>
    SpawnExplosionSilhouette,
    /// <summary>Starts the Zebes explosion.</summary>
    StartZebesExplosion,
    /// <summary>Starts the explosion finale.</summary>
    ExplosionFinale,
    /// <summary>Ends the Zebes explosion.</summary>
    EndZebesExplosion,
    /// <summary>Spawns COMPLETED SUCCESSFULLY.</summary>
    SpawnCompletedText,
    /// <summary>Spawns CLEAR TIME.</summary>
    SpawnClearTime,
    /// <summary>Spawns the clear time's hours tens digit.</summary>
    SpawnHoursTens,
    /// <summary>Spawns the clear time's hours ones digit.</summary>
    SpawnHoursUnits,
    /// <summary>Spawns the clear time's colon.</summary>
    SpawnColon,
    /// <summary>Spawns the clear time's minutes tens digit.</summary>
    SpawnMinutesTens,
    /// <summary>Spawns the clear time's minutes ones digit.</summary>
    SpawnMinutesUnits,
    /// <summary>Moves on to the credits.</summary>
    TransitionToCredits,
}
