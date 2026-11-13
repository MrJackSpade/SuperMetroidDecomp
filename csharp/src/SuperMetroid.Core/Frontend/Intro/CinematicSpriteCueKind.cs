namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What a cue of a cinematic sprite animation does: the shared frame-behaviour install and each owner cue the animations use.</summary>
internal enum CinematicSpriteCueKind
{
    /// <summary>Installs <see cref="CinematicSpriteCue.FrameBehavior"/> (the shared frame-behaviour install).</summary>
    SetFrameBehavior,
    /// <summary>Ends the Zebes explosion.</summary>
    EndZebesExplosion,
    /// <summary>Starts the explosion finale.</summary>
    ExplosionFinale,
    /// <summary>Fades the exploding planet's palette.</summary>
    FadeExplosionPalette,
    /// <summary>Fades the PLANET ZEBES title in.</summary>
    FadeInZebesText,
    /// <summary>Fades the PLANET ZEBES title out.</summary>
    FadeOutZebesText,
    /// <summary>Requests the icon's grey-out crossfade.</summary>
    GreyOut,
    /// <summary>Launches Samus's jump.</summary>
    Launch,
    /// <summary>Launches the suited Samus's head.</summary>
    LaunchHead,
    /// <summary>Prepares the suited Samus's separate head.</summary>
    PrepareHead,
    /// <summary>Fires Samus's post-credits shot.</summary>
    Shoot,
    /// <summary>Spawns CLEAR TIME.</summary>
    SpawnClearTime,
    /// <summary>Spawns the clear time's colon.</summary>
    SpawnColon,
    /// <summary>Spawns COMPLETED SUCCESSFULLY.</summary>
    SpawnCompletedText,
    /// <summary>Spawns the explosion silhouette.</summary>
    SpawnExplosionSilhouette,
    /// <summary>Spawns Ceres's final large-burst explosion wave.</summary>
    SpawnFinalExplosionWave,
    /// <summary>Spawns the clear time's hours tens digit.</summary>
    SpawnHoursTens,
    /// <summary>Spawns the clear time's hours ones digit.</summary>
    SpawnHoursUnits,
    /// <summary>Spawns Ceres's initial square-and-center explosion wave.</summary>
    SpawnInitialExplosionWave,
    /// <summary>Spawns the Japanese title when Japanese text is selected.</summary>
    SpawnJapaneseZebesTextIfNeeded,
    /// <summary>Spawns the clear time's minutes tens digit.</summary>
    SpawnMinutesTens,
    /// <summary>Spawns the clear time's minutes ones digit.</summary>
    SpawnMinutesUnits,
    /// <summary>Spawns the hatching egg's particles.</summary>
    SpawnParticles,
    /// <summary>Spawns the first pair of intro Rinkas.</summary>
    SpawnRinkas0And1,
    /// <summary>Spawns the second pair of intro Rinkas.</summary>
    SpawnRinkas2And3,
    /// <summary>Hands the suited gesture off to its jump.</summary>
    SpawnSuitedJump,
    /// <summary>Hands the suitless gesture off to its jump.</summary>
    SpawnSuitlessJump,
    /// <summary>Starts the flight to Zebes.</summary>
    StartFlyingToZebes,
    /// <summary>Starts the intro's second page.</summary>
    StartIntroPage2,
    /// <summary>Starts the intro's third page.</summary>
    StartIntroPage3,
    /// <summary>Starts the intro's fourth page.</summary>
    StartIntroPage4,
    /// <summary>Starts the intro's fifth page.</summary>
    StartIntroPage5,
    /// <summary>Starts an intro Rinka moving.</summary>
    StartRinkaMoving,
    /// <summary>Starts the Zebes explosion.</summary>
    StartZebesExplosion,
    /// <summary>Moves on to the credits.</summary>
    TransitionToCredits,
}
