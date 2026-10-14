namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>What a cue of the Rio's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum RioCueKind
{
    /// <summary>Tells main AI the landing or swoop animation has finished (<c>Instruction_Rio_SetAnimationFinishedFlag</c>, $A2:BBC3).</summary>
    AnimationFinished,
}
