namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>What a cue of Choot's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum ChootCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen (<c>CommonEnemyInstruction.EnableOffScreenProcessing</c>, $A0:8173).</summary>
    ProcessOffScreen,
    /// <summary>Stops processing the enemy while it is off screen (<c>CommonEnemyInstruction.DisableOffScreenProcessing</c>, $A0:817D).</summary>
    StopProcessingOffScreen,
}
