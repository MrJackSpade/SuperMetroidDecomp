namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>What a cue of Owtch's programs does: one per bank-$A2 instruction its lists use.</summary>
internal enum OwtchCueKind
{
    /// <summary>Makes main AI move Owtch left (<c>SetMovingLeft</c>, $A2:A56D).</summary>
    SetMovingLeft,
    /// <summary>Makes main AI move Owtch right (<c>SetMovingRight</c>, $A2:A571).</summary>
    SetMovingRight,
}
