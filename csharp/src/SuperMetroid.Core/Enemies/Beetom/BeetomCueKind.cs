namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>What a cue of the Beetom's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum BeetomCueKind
{
    /// <summary>Does nothing: the native RTS stub the drain animation calls before its loop (<c>Nothing</c>, $A8:B75E).</summary>
    Nothing,
    /// <summary>Keeps processing the enemy while it is off screen (<c>CommonEnemyInstruction.EnableOffScreenProcessing</c>, $A0:8173).</summary>
    ProcessOffScreen,
    /// <summary>Stops processing the enemy while it is off screen (<c>CommonEnemyInstruction.DisableOffScreenProcessing</c>, $A0:817D).</summary>
    StopProcessingOffScreen,
}
