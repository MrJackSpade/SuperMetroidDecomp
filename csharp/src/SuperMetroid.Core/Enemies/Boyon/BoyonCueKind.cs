namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>What a cue of Boyon's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum BoyonCueKind
{
    /// <summary>Keeps processing the enemy while it is off screen (<c>CommonEnemyInstruction.EnableOffScreenProcessing</c>, $A0:8173).</summary>
    ProcessOffScreen,
    /// <summary>Returns at once without effect (<c>Return</c>, $A2:88C5).</summary>
    Return,
    /// <summary>Plays the bounce sound and lets main AI start the bounce arc (<c>StartBounce</c>, $A2:88C6).</summary>
    StartBounce,
    /// <summary>Stops processing the enemy while it is off screen (<c>CommonEnemyInstruction.DisableOffScreenProcessing</c>, $A0:817D).</summary>
    StopProcessingOffScreen,
}
