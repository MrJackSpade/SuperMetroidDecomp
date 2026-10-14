namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>What a cue of the Norfair lava jumper's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum NorfairLavaJumperCueKind
{
    /// <summary>Tells main AI the jump animation has finished (<c>SetAnimationFinished</c>, $A2:BE8E).</summary>
    FinishAnimation,
}
