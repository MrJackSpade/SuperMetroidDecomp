namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What a cue of a cinematic sprite program does: the shared pre-instruction install and each owner instruction the lists use.</summary>
internal enum CinematicSpriteCueKind
{
    /// <summary>Installs <see cref="CinematicSpriteCue.PreInstruction"/> (the shared pre-instruction install at $8B:944C).</summary>
    SetPreInstruction,
    /// <summary>Ends the Zebes explosion (<c>Instruction_EndZebesExplosion</c>, $8B:F32B).</summary>
    EndZebesExplosion,
    /// <summary>Starts the explosion finale (<c>Instruction_ZebesExplosionFinale</c>, $8B:F2FA).</summary>
    ExplosionFinale,
    /// <summary>Fades the exploding planet's palette (<c>Instruction_FadeOutZoomedOutExplodingZebes</c>, $8B:F284).</summary>
    FadeExplosionPalette,
    /// <summary>Fades the PLANET ZEBES title in (<c>Instruction_FadeInPlanetZebesText</c>, $8B:C9A5).</summary>
    FadeInZebesText,
    /// <summary>Fades the PLANET ZEBES title out (<c>Instruction_FadeOutPlanetZebesText</c>, $8B:C9BD).</summary>
    FadeOutZebesText,
    /// <summary>Requests the icon's grey-out crossfade (<c>Instruction_GreyOutSuperMetroidIcon</c>, $8B:F25E).</summary>
    GreyOut,
    /// <summary>Launches Samus's jump (<c>Instruction_CinematicSpriteObject_MakeEndingSamusJump</c>, $8B:F651).</summary>
    Launch,
    /// <summary>Launches the suited Samus's head (<c>Instruction_CinematicSpriteObject_PositionSamusHeadToJump</c>, $8B:F5BA).</summary>
    LaunchHead,
    /// <summary>Prepares the suited Samus's separate head (<c>Instruction_CineSpriteObject_PositionSuitedHeadToPrepareJump</c>, $8B:F597).</summary>
    PrepareHead,
    /// <summary>Fires Samus's post-credits shot (<c>Instruction_CinematicSpriteObject_SamusShootsScreen</c>, $8B:F604).</summary>
    Shoot,
    /// <summary>Spawns CLEAR TIME (<c>Instruction_CinematicSpriteObject_SpawnClearTime</c>, $8B:F3CE).</summary>
    SpawnClearTime,
    /// <summary>Spawns the clear time's colon (<c>Instruction_CinematicSpriteObject_SpawnClearTime_Colon</c>, $8B:F42D).</summary>
    SpawnColon,
    /// <summary>Spawns COMPLETED SUCCESSFULLY (<c>Instruction_CinematicSpriteObject_SpawnCompletedSuccessfully</c>, $8B:F3B0).</summary>
    SpawnCompletedText,
    /// <summary>Spawns the explosion silhouette (<c>Instruction_CineSpriteObjectSpawnZebesExplosionSilhouette</c>, $8B:F295).</summary>
    SpawnExplosionSilhouette,
    /// <summary>Spawns Ceres's final large-burst explosion wave ($8B:C50C).</summary>
    SpawnFinalExplosionWave,
    /// <summary>Spawns the clear time's hours tens digit (<c>Instruction_CineSpriteObject_SpawnClearTime_Hours_TensDigit</c>, $8B:F41B).</summary>
    SpawnHoursTens,
    /// <summary>Spawns the clear time's hours ones digit (<c>Instruction_CineSpriteObject_SpawnClearTime_Hours_OnesDigit</c>, $8B:F424).</summary>
    SpawnHoursUnits,
    /// <summary>Spawns Ceres's initial square-and-center explosion wave ($8B:C404).</summary>
    SpawnInitialExplosionWave,
    /// <summary>Spawns the Japanese title when Japanese text is selected (<c>Instruction_SpawnPlanetZebesSubtitleIfNeeded</c>, $8B:C9AF).</summary>
    SpawnJapaneseZebesTextIfNeeded,
    /// <summary>Spawns the clear time's minutes tens digit (<c>Inst_CineSpriteObject_SpawnClearTime_Minutes_TensDigit</c>, $8B:F436).</summary>
    SpawnMinutesTens,
    /// <summary>Spawns the clear time's minutes ones digit (<c>Inst_CineSpriteObject_SpawnClearTime_Minutes_OnesDigit</c>, $8B:F43F).</summary>
    SpawnMinutesUnits,
    /// <summary>Spawns the hatching egg's particles (<c>Instruction_SpawnMetroidEggParticles</c>, $8B:A918).</summary>
    SpawnParticles,
    /// <summary>Spawns the first pair of intro Rinkas (<c>Instruction_Spawn_IntroRinkas_0_1</c>, $8B:BA21).</summary>
    SpawnRinkas0And1,
    /// <summary>Spawns the second pair of intro Rinkas (<c>Instruction_Spawn_IntroRinkas_2_3</c>, $8B:BA36).</summary>
    SpawnRinkas2And3,
    /// <summary>Hands the suited gesture off to its jump (<c>Instruction_CinematicSpriteObject_SpawnSuitedSamusJump</c>, $8B:F554).</summary>
    SpawnSuitedJump,
    /// <summary>Hands the suitless gesture off to its jump (<c>Instruction_CinematicSpriteObject_SpawnSuitlessSamusJump</c>, $8B:F51D).</summary>
    SpawnSuitlessJump,
    /// <summary>Starts the flight to Zebes (<c>Instruction_StartFlyingToZebes</c>, $8B:C9C7).</summary>
    StartFlyingToZebes,
    /// <summary>Starts the intro's second page (<c>Instruction_StartIntroPage2</c>, $8B:B336).</summary>
    StartIntroPage2,
    /// <summary>Starts the intro's third page (<c>Instruction_StartIntroPage3</c>, $8B:B33E).</summary>
    StartIntroPage3,
    /// <summary>Starts the intro's fourth page (<c>Instruction_StartIntroPage4</c>, $8B:B346).</summary>
    StartIntroPage4,
    /// <summary>Starts the intro's fifth page (<c>Instruction_StartIntroPage5</c>, $8B:B34E).</summary>
    StartIntroPage5,
    /// <summary>Starts an intro Rinka moving (<c>Instruction_StartMoving_IntroRinka</c>, $8B:B8C5).</summary>
    StartRinkaMoving,
    /// <summary>Starts the Zebes explosion (<c>Instruction_CinematicSpriteObject_StartZebesExplosion</c>, $8B:F2B7).</summary>
    StartZebesExplosion,
    /// <summary>Moves on to the credits (<c>Instruction_CinematicSpriteObject_TransitionToCredits</c>, $8B:F448).</summary>
    TransitionToCredits,
}
