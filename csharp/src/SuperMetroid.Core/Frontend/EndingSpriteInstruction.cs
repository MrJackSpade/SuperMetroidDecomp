namespace SuperMetroid.Core.Frontend;

/// <summary>Private instructions of the ending's Zebes-explosion and clear-time actors.</summary>
internal enum EndingSpriteInstruction : ushort
{
    /// <summary><c>Instruction_FadeOutZoomedOutExplodingZebes</c> at $8B:F284.</summary>
    FadeExplosionPalette = 0xf284,

    /// <summary><c>Instruction_CineSpriteObjectSpawnZebesExplosionSilhouette</c> at $8B:F295.</summary>
    SpawnExplosionSilhouette = 0xf295,

    /// <summary><c>Instruction_CinematicSpriteObject_StartZebesExplosion</c> at $8B:F2B7.</summary>
    StartZebesExplosion = 0xf2b7,

    /// <summary><c>Instruction_ZebesExplosionFinale</c> at $8B:F2FA.</summary>
    ExplosionFinale = 0xf2fa,

    /// <summary><c>Instruction_EndZebesExplosion</c> at $8B:F32B.</summary>
    EndZebesExplosion = 0xf32b,

    /// <summary><c>Instruction_CinematicSpriteObject_SpawnCompletedSuccessfully</c> at $8B:F3B0.</summary>
    SpawnCompletedText = 0xf3b0,

    /// <summary><c>Instruction_CinematicSpriteObject_SpawnClearTime</c> at $8B:F3CE.</summary>
    SpawnClearTime = 0xf3ce,

    /// <summary><c>Instruction_CineSpriteObject_SpawnClearTime_Hours_TensDigit</c> at $8B:F41B.</summary>
    SpawnHoursTens = 0xf41b,

    /// <summary><c>Instruction_CineSpriteObject_SpawnClearTime_Hours_OnesDigit</c> at $8B:F424.</summary>
    SpawnHoursUnits = 0xf424,

    /// <summary><c>Instruction_CinematicSpriteObject_SpawnClearTime_Colon</c> at $8B:F42D.</summary>
    SpawnColon = 0xf42d,

    /// <summary><c>Inst_CineSpriteObject_SpawnClearTime_Minutes_TensDigit</c> at $8B:F436.</summary>
    SpawnMinutesTens = 0xf436,

    /// <summary><c>Inst_CineSpriteObject_SpawnClearTime_Minutes_OnesDigit</c> at $8B:F43F.</summary>
    SpawnMinutesUnits = 0xf43f,

    /// <summary><c>Instruction_CinematicSpriteObject_TransitionToCredits</c> at $8B:F448.</summary>
    TransitionToCredits = 0xf448,
}
