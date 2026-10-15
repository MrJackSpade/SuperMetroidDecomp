namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>What a cue of the Fune's and Namihe's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum FuneNamiheCueKind
{
    /// <summary>Returns to the idle animation and the waiting function (<c>Instruction_FuneNamihe_FinishActivity</c>, $A8:9695, and its byte-identical duplicate at $A8:96B4).</summary>
    FinishActivity,
    /// <summary>Spits a Fune fireball to the left (<c>FuneSpawnFireballFacingLeft</c>, $A8:9663).</summary>
    FuneFireLeft,
    /// <summary>Spits a Fune fireball to the right (<c>FuneSpawnFireballFacingRight</c>, $A8:967C).</summary>
    FuneFireRight,
    /// <summary>Spits a Namihe fireball to the left (<c>NamiheSpawnFireballFacingLeft</c>, $A8:9631).</summary>
    NamiheFireLeft,
    /// <summary>Spits a Namihe fireball to the right (<c>NamiheSpawnFireballFacingRight</c>, $A8:964A).</summary>
    NamiheFireRight,
    /// <summary>Plays the spit sound (<c>QueueSpitSFX</c>, $A8:9625).</summary>
    SpitSound,
}
